using UnityEngine;

/// <summary>
/// 控制玩家的移动和旋转，包括行走、奔跑和朝向鼠标指针。
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerMove : MonoBehaviour
{
    // 移动输入的最小阈值，低于此值视为无移动
    private const float MovementThreshold = 0.01f;
    // 移动输入阈值的平方，用于优化计算
    private const float MovementThresholdSqr = MovementThreshold * MovementThreshold;
    // 默认的旋转插值速度
    private const float DefaultRotationSlerpSpeed = 15f;
    // 鼠标射线检测的最大距离
    private const float MouseRaycastDistance = 500f;

    // Animator 引用和参数哈希
    private Animator _animator;
    // 使用 Hash 可以提高性能
    private static readonly int MovementSpeedHash = Animator.StringToHash("MovementSpeed");
    private static readonly int IsRunningHash = Animator.StringToHash("IsRunning"); 

    [Header("Movement Settings")]
    [Tooltip("行走速度")]
    public float walkSpeed = 5.0f;
    [Tooltip("奔跑速度")]
    public float runSpeed = 10.0f;
    
    [Tooltip("角色朝向目标方向的旋转速度")]
    [SerializeField] private float rotationSlerpSpeed = DefaultRotationSlerpSpeed;

    // 私有状态变量
    // 是否处于冲刺状态
    private bool _isSprinting = false; 
    // 当前的移动速度
    private float _currentSpeed;
    
    // 移动输入变量
    // 玩家的移动输入向量
    private Vector3 _movementInput;
    // 玩家是否有移动输入
    private bool _isMoving;
    // 刚体组件的引用
    private Rigidbody _rb;
    // 游戏主摄像机
    private Camera _gameplayCamera;

    /// <summary>
    /// 初始化组件引用和默认值。
    /// </summary>
    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>(); // <-- 获取 Animator 组件
        _currentSpeed = walkSpeed;

        AcquireGameplayCamera();

        if (_animator == null)
        {
            Debug.LogError("Animator component not found on the GameObject.");
        }
    }

    /// <summary>
    /// 每帧更新，处理玩家输入和状态切换。
    /// </summary>
    void Update()
    {
        // 1. 获取WASD的输入值
        float horizontalInput = Input.GetAxis("Horizontal"); // A/D
        float verticalInput = Input.GetAxis("Vertical");     // W/S

        // 使用世界坐标轴计算移动方向
        // 注意：如果你希望角色面向摄像机方向移动，这里需要做额外的计算
        _movementInput = (horizontalInput * Vector3.right) + (verticalInput * Vector3.forward);

        // 检查是否有任何移动输入 (WASD是否被按住)
        _isMoving = _movementInput.sqrMagnitude > MovementThresholdSqr;

        
        // --- 2. 奔跑状态切换逻辑 ---
        
        // A. 奔跑锁定启动条件: 玩家正在移动 并且 Shift 键被按住 (GetKey)
        if (_isMoving && Input.GetKey(KeyCode.LeftShift))
        {
            _isSprinting = true;
        }

        // B. 奔跑锁定关闭条件: 玩家停止了移动
        if (!_isMoving)
        {
            _isSprinting = false;
        }
        
        // --- 3. 确定当前速度 ---
        if (_isSprinting)
        {
            _currentSpeed = runSpeed;
        }
        else
        {
            _currentSpeed = walkSpeed; 
        }

        // 限制对角线移动速度，将输入向量长度归一化为 1
        if (_movementInput.sqrMagnitude > 1)
        {
            _movementInput.Normalize();
        }
    }

    /// <summary>
    /// 在固定的时间间隔内更新，用于处理物理相关的移动、旋转和动画参数。
    /// </summary>
    void FixedUpdate()
    {
        // ========================
        // 1. 物理运动处理
        // ========================
        
        // 计算目标速度：移动方向 * 当前选择的速度 (_currentSpeed)
        Vector3 targetVelocity = _movementInput * _currentSpeed;
        
        // 应用速度
        _rb.velocity = new Vector3(targetVelocity.x, _rb.velocity.y, targetVelocity.z);

        Vector3 desiredDirection = Vector3.zero;

        if (_isSprinting && _isMoving)
        {
            desiredDirection = _movementInput.normalized;
        }
        else
        {
            if (_gameplayCamera == null)
            {
                AcquireGameplayCamera();
            }

            desiredDirection = GetMouseWorldDirection();
            if (desiredDirection.sqrMagnitude <= MovementThresholdSqr && _isMoving)
            {
                desiredDirection = _movementInput.normalized;
            }
        }

        if (desiredDirection.sqrMagnitude > MovementThresholdSqr)
        {
            Quaternion targetRotation = Quaternion.LookRotation(desiredDirection);
            
            // 3. 平滑地将角色旋转到目标方向
            // 使用 Quaternion.Slerp 在当前旋转和目标旋转之间进行平滑过渡
            // 速度可以自行调整，例如 15.0f * Time.fixedDeltaTime
            transform.rotation = Quaternion.Slerp(
                transform.rotation, 
                targetRotation, 
                rotationSlerpSpeed * Time.fixedDeltaTime
            );
        }
        
        // ========================
        // 2. 动画参数设置
        // ========================
        if (_animator != null)
        {
            // A. 计算 MovementSpeed (float)
            // 获取水平速度的大小 (忽略Y轴，只关心X-Z平面的移动)
            Vector3 horizontalVelocity = _rb.velocity;
            horizontalVelocity.y = 0;

            // 使用速度的绝对值
            float currentHorizontalSpeed = horizontalVelocity.magnitude;

            // 设置 MovementSpeed 参数
            // 注意：这里我们直接用速度的绝对值，而不是归一化。
            // 状态机通过阈值（如0.01）来判断是否进入Walk
            _animator.SetFloat(MovementSpeedHash, currentHorizontalSpeed);

            // B. 设置 IsRunning (bool)
            // 如果速度大于步行速度 (例如 walkSpeed - 1.0f)，并且正在移动，则设置为奔跑状态
            // 或者直接使用你代码中的 _isSprinting 变量
            _animator.SetBool(IsRunningHash, _isSprinting);
        }
    }

    /// <summary>
    /// 获取鼠标在世界空间中的方向（相对于玩家）。
    /// </summary>
    /// <returns>从玩家指向鼠标世界位置的归一化方向向量</returns>
    private Vector3 GetMouseWorldDirection()
    {
        if (_gameplayCamera == null)
        {
            return Vector3.zero;
        }

        Ray mouseRay = _gameplayCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f));

        if (groundPlane.Raycast(mouseRay, out float distance) && distance < MouseRaycastDistance)
        {
            Vector3 worldPoint = mouseRay.GetPoint(distance);
            Vector3 direction = worldPoint - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > MovementThresholdSqr)
            {
                direction.Normalize();
                return direction;
            }
        }

        return Vector3.zero;
    }

    /// <summary>
    /// 获取游戏主摄像机。
    /// </summary>
    private void AcquireGameplayCamera()
    {
        if (GameplayCameraProvider.TryGetGameplayCamera(out Camera camera))
        {
            _gameplayCamera = camera;
        }
        else
        {
            _gameplayCamera = Camera.main;
        }
    }
}