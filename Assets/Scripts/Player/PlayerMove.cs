using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMove : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5.0f;
    public float runSpeed = 10.0f;
    
    // 私有状态变量
    private bool _isSprinting = false; // 追踪当前是否处于奔跑锁定状态
    private float _currentSpeed;
    
    // 移动输入变量
    private Vector3 _movementInput;
    private Rigidbody _rb;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _currentSpeed = walkSpeed;
    }

    void Update()
    {
        // 1. 获取WASD的输入值
        float horizontalInput = Input.GetAxis("Horizontal"); // A/D
        float verticalInput = Input.GetAxis("Vertical");     // W/S

        // 使用世界坐标轴计算移动方向
        _movementInput = (horizontalInput * Vector3.right) + (verticalInput * Vector3.forward);

        // 检查是否有任何移动输入 (WASD是否被按住)
        bool isMoving = _movementInput.sqrMagnitude > 0.01f;

        
        // --- 2. 奔跑状态切换逻辑 ---

        // A. 奔跑锁定启动条件: 玩家正在移动 并且 Shift 键被按住 (GetKey)
        if (isMoving && Input.GetKey(KeyCode.LeftShift))
        {
            // 只要满足条件，就锁定为奔跑模式
            _isSprinting = true;
        }

        // B. 奔跑锁定关闭条件: 玩家停止了移动
        // 如果玩家停止移动 (松开了所有WASD键)，则重置为步行模式
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
            // 即使正在按 Shift，如果 _isSprinting 没被锁定（即角色是静止的），
            // 速度也仍然是步行速度。
            _currentSpeed = walkSpeed; 
        }

        // 限制对角线移动速度，将输入向量长度归一化为 1
        if (_movementInput.sqrMagnitude > 1)
        {
            _movementInput.Normalize();
        }
    }

    // FixedUpdate用于处理物理运动
    void FixedUpdate()
    {
        // 计算目标速度：移动方向 * 当前选择的速度 (_currentSpeed)
        Vector3 targetVelocity = _movementInput * _currentSpeed;
        
        // 应用速度
        _rb.velocity = new Vector3(targetVelocity.x, _rb.velocity.y, targetVelocity.z);
    }
}