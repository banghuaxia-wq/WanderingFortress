using UnityEngine;

public class AllyFollower : MonoBehaviour
{
    private const float MinDirectionSqr = 0.0001f;

    [SerializeField] private float followSpeed = 4f;
    [SerializeField] private float stoppingDistance = 1.5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private Rigidbody enemyRigidbody;

    private Transform _followTarget;
    private bool _isActive;

    private void Awake()
    {
        if (enemyRigidbody == null)
        {
            enemyRigidbody = GetComponent<Rigidbody>();
        }
    }

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
