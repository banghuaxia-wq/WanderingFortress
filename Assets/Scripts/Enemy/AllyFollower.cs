using UnityEngine;

/// <summary>
/// 控制盟友单位跟随指定目标。
/// </summary>
public class AllyFollower : MonoBehaviour
{
    // 移动方向向量的最小平方长度，用于避免零向量问题
    private const float MinDirectionSqr = 0.0001f;

    [Tooltip("跟随目标时的移动速度")]
    [SerializeField] private float followSpeed = 4f;
    [Tooltip("与目标保持的最小距离")]
    [SerializeField] private float stoppingDistance = 1.5f;
    [Tooltip("朝向目标时的旋转速度")]
    [SerializeField] private float rotationSpeed = 10f;
    [Tooltip("敌人的刚体组件")]
    [SerializeField] private Rigidbody enemyRigidbody;

    // 当前跟随的目标
    private Transform _followTarget;
    // 是否激活跟随行为
    private bool _isActive;

    /// <summary>
    /// 初始化组件引用。
    /// </summary>
    private void Awake()
    {
        if (enemyRigidbody == null)
        {
            enemyRigidbody = GetComponent<Rigidbody>();
        }
    }

    /// <summary>
    /// 在固定的时间间隔内更新，用于处理物理相关的移动和旋转。
    /// </summary>
    private void FixedUpdate()
    {
        if (!_isActive || _followTarget == null)
        {
            StopHorizontalMovement();
            return;
        }

        Vector3 offsetToTarget = _followTarget.position - transform.position;
        offsetToTarget.y = 0f;
        float stopDistanceSqr = stoppingDistance * stoppingDistance;

        if (offsetToTarget.sqrMagnitude <= stopDistanceSqr)
        {
            StopHorizontalMovement();
            return;
        }

        Vector3 moveDirection = offsetToTarget.normalized;
        Vector3 desiredVelocity = moveDirection * followSpeed;

        if (enemyRigidbody != null)
        {
            Vector3 currentVelocity = enemyRigidbody.velocity;
            enemyRigidbody.velocity = new Vector3(desiredVelocity.x, currentVelocity.y, desiredVelocity.z);
        }
        else
        {
            transform.position += desiredVelocity * Time.fixedDeltaTime;
        }

        if (moveDirection.sqrMagnitude > MinDirectionSqr)
        {
            Quaternion desiredRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    /// <summary>
    /// 激活友军跟随逻辑。
    /// </summary>
    /// <param name="target">要跟随的目标</param>
    public void ActivateFollow(Transform target)
    {
        _followTarget = target;
        _isActive = true;
    }

    /// <summary>
    /// 停止跟随并清理速度。
    /// </summary>
    public void DeactivateFollow()
    {
        _isActive = false;
        _followTarget = null;
        StopHorizontalMovement();
    }

    /// <summary>
    /// 停止单位的水平移动。
    /// </summary>
    private void StopHorizontalMovement()
    {
        if (enemyRigidbody == null)
        {
            return;
        }

        Vector3 currentVelocity = enemyRigidbody.velocity;
        enemyRigidbody.velocity = new Vector3(0f, currentVelocity.y, 0f);
    }
}
