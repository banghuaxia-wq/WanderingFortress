using UnityEngine;

namespace WF.Gameplay.Systems.Enemy
{
    // 敌人视觉传感器：锥形范围 + 障碍物遮挡射线检测（中文注释）
    public class EnemyVisionSensor : MonoBehaviour
    {
        [Header("Vision")]
        [SerializeField] private Transform eye; // 视线起点（眼睛/头部）（中文注释）
        [SerializeField] private float viewDistance = 12f; // 视距（中文注释）
        [SerializeField] private float viewAngle = 90f; // 视野角度（度）（中文注释）
        [SerializeField] private float visionLoseDelaySeconds = 1.2f; // 视野丢失延迟（中文注释）
        [SerializeField] private float checkIntervalSeconds = 0.1f; // 检测间隔（节流）（中文注释）

        [Header("Occlusion")]
        [SerializeField] private LayerMask obstacleMask = ~0; // 遮挡层（墙体等）（中文注释）

        public float VisionLoseDelaySeconds => visionLoseDelaySeconds; // 丢失延迟（中文注释）
        public float CheckIntervalSeconds => checkIntervalSeconds; // 检测间隔（中文注释）

        // 是否能看见目标（距离/角度/遮挡全通过）（中文注释）
        public bool CanSee(Transform target)
        {
            if (target == null) return false;

            Vector3 eyePos = eye != null ? eye.position : transform.position;
            Vector3 targetPos = target.position;

            Vector3 delta = targetPos - eyePos;
            delta.y = 0f;
            float sqr = delta.sqrMagnitude;
            if (sqr <= 0.0001f) return true;

            float maxSqr = viewDistance * viewDistance;
            if (sqr > maxSqr) return false;

            Vector3 forward = transform.forward;
            forward.y = 0f;
            forward.Normalize();

            Vector3 dir = delta.normalized;
            float half = viewAngle * 0.5f;
            float angleToTarget = Vector3.Angle(forward, dir);
            if (angleToTarget > half) return false;

            float dist = Mathf.Sqrt(sqr);
            Vector3 rayOrigin = eyePos;
            Vector3 rayDir = (targetPos - rayOrigin).normalized;

            if (Physics.Raycast(rayOrigin, rayDir, out var hit, dist, obstacleMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.transform != target && !hit.transform.IsChildOf(target))
                {
                    return false;
                }
            }

            return true;
        }
    }
}

