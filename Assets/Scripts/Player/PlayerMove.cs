using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMove : MonoBehaviour
{
    // Animator 引用和参数哈希
    private Animator _animator;
    // 使用 Hash 可以提高性能
    private static readonly int MovementSpeedHash = Animator.StringToHash("MovementSpeed");
    private static readonly int IsRunningHash = Animator.StringToHash("IsRunning"); 

    [Header("Movement Settings")]
    public float walkSpeed = 5.0f;
    public float runSpeed = 10.0f;
    
    // 私有状态变量
    private bool _isSprinting = false; 
    private float _currentSpeed;
    
    // 移动输入变量
    private Vector3 _movementInput;
    private Rigidbody _rb;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>(); // <-- 获取 Animator 组件
        _currentSpeed = walkSpeed;

        if (_animator == null)
        {
            Debug.LogError("Animator component not found on the GameObject.");
        }
    }

    void Update()
    {
        // 1. 获取WASD的输入值
        float horizontalInput = Input.GetAxis("Horizontal"); // A/D
        float verticalInput = Input.GetAxis("Vertical");     // W/S

        // 使用世界坐标轴计算移动方向
        // 注意：如果你希望角色面向摄像机方向移动，这里需要做额外的计算
        _movementInput = (horizontalInput * Vector3.right) + (verticalInput * Vector3.forward);

        // 检查是否有任何移动输入 (WASD是否被按住)
        bool isMoving = _movementInput.sqrMagnitude > 0.01f;

        
        // --- 2. 奔跑状态切换逻辑 ---
        
        // A. 奔跑锁定启动条件: 玩家正在移动 并且 Shift 键被按住 (GetKey)
        if (isMoving && Input.GetKey(KeyCode.LeftShift))
        {
            _isSprinting = true;
        }

        // B. 奔跑锁定关闭条件: 玩家停止了移动
        if (!isMoving)
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

    // FixedUpdate用于处理物理运动和动画参数设置
    void FixedUpdate()
    {
        // ========================
        // 1. 物理运动处理
        // ========================
        
        // 计算目标速度：移动方向 * 当前选择的速度 (_currentSpeed)
        Vector3 targetVelocity = _movementInput * _currentSpeed;
        
        // 应用速度
        _rb.velocity = new Vector3(targetVelocity.x, _rb.velocity.y, targetVelocity.z);

        if (_movementInput.sqrMagnitude > 0.01f)
        {
            // 1. 计算目标旋转方向
            // _movementInput 已经是 X-Z 平面上的方向向量
            Vector3 lookDirection = _movementInput.normalized; 
            
            // 2. 根据方向向量计算目标四元数（Quaternion）
            // LookRotation 会返回从 Z 轴指向 lookDirection 的旋转
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            
            // 3. 平滑地将角色旋转到目标方向
            // 使用 Quaternion.Slerp 在当前旋转和目标旋转之间进行平滑过渡
            // 速度可以自行调整，例如 15.0f * Time.fixedDeltaTime
            float rotationSpeed = 15.0f; // 你可以把这个速度定义为 public 变量
            transform.rotation = Quaternion.Slerp(
                transform.rotation, 
                targetRotation, 
                rotationSpeed * Time.fixedDeltaTime
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
}