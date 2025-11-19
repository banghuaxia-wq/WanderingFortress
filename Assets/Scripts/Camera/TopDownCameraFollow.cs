using UnityEngine;

/// <summary>
/// 控制摄像机以俯视视角平滑地跟随目标。
/// </summary>
public class TopDownCameraFollow : MonoBehaviour
{
    // 最小平滑时间，以避免卡顿。
    private const float MinSmoothingTime = 0.01f;
    // 最小观察方向向量长度的平方，用于优化计算。
    private const float MinLookDirectionSqr = 0.0001f;

    [Tooltip("摄像机要跟随的目标 Transform。")]
    [SerializeField] private Transform followTarget;
    [Tooltip("摄像机相对于目标的偏移量。")]
    [SerializeField] private Vector3 followOffset = new Vector3(0f, 15f, -10f);
    [Tooltip("摄像机位置平滑移动的时间。")]
    [SerializeField] private float positionSmoothTime = 0.2f;
    [Tooltip("是否启用摄像机朝向目标的功能。")]
    [SerializeField] private bool enableLookAtTarget = true;
    [Tooltip("摄像机观察目标的偏移量。")]
    [SerializeField] private Vector3 lookAtOffset = new Vector3(0f, 1.5f, 0f);
    [Tooltip("摄像机旋转以朝向目标的插值速度。")]
    [SerializeField] private float rotationLerpSpeed = 10f;

    // 用于 SmoothDamp 的当前速度向量。
    private Vector3 _currentVelocity;

    /// <summary>
    /// 在每帧的最后更新摄像机的位置和旋转。
    /// </summary>
    private void LateUpdate()
    {
        if (followTarget == null)
        {
            return;
        }

        Vector3 desiredPosition = followTarget.position + followOffset;
        float smoothTime = Mathf.Max(positionSmoothTime, MinSmoothingTime);
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _currentVelocity, smoothTime);

        if (!enableLookAtTarget)
        {
            return;
        }

        Vector3 focusPoint = followTarget.position + lookAtOffset;
        Vector3 lookDirection = focusPoint - transform.position;
        if (lookDirection.sqrMagnitude < MinLookDirectionSqr)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationLerpSpeed * Time.deltaTime);
    }

    /// <summary>
    /// 设置摄像机要跟随的目标。
    /// </summary>
    /// <param name="target">要跟随的目标 Transform。</param>
    public void SetFollowTarget(Transform target)
    {
        followTarget = target;
        // 立即同步位置，避免瞬移感
        if (followTarget != null)
        {
            transform.position = followTarget.position + followOffset;
        }
    }

    /// <summary>
    /// 更新摄像机跟随的偏移量。
    /// </summary>
    /// <param name="offset">新的跟随偏移量。</param>
    public void SetFollowOffset(Vector3 offset)
    {
        followOffset = offset;
    }
}

