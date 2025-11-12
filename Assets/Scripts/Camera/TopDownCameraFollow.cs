using UnityEngine;

public class TopDownCameraFollow : MonoBehaviour
{
    private const float MinSmoothingTime = 0.01f;
    private const float MinLookDirectionSqr = 0.0001f;

    [SerializeField] private Transform followTarget;
    [SerializeField] private Vector3 followOffset = new Vector3(0f, 15f, -10f);
    [SerializeField] private float positionSmoothTime = 0.2f;
    [SerializeField] private bool enableLookAtTarget = true;
    [SerializeField] private Vector3 lookAtOffset = new Vector3(0f, 1.5f, 0f);
    [SerializeField] private float rotationLerpSpeed = 10f;

    private Vector3 _currentVelocity;

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
    public void SetFollowOffset(Vector3 offset)
    {
        followOffset = offset;
    }
}

