using UnityEngine;
using UnityEngine.AI;
using WF.Gameplay.Core.Data;

namespace WF.Gameplay.Systems.Enemy
{
    // 基于NavMeshAgent的移动控制器：封装Move/Stop/Arrive与速度档位（中文注释）
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyNavMovementController : MonoBehaviour
    {
        [Header("Agent")]
        [SerializeField] private NavMeshAgent agent; // NavMeshAgent引用（中文注释）
        [SerializeField] private float arriveDistance = 0.25f; // 到达阈值（中文注释）

        [Header("Speeds")]
        [SerializeField] private float patrolSpeed = 1.8f; // 巡逻速度（中文注释）
        [SerializeField] private float chaseSpeed = 3.2f; // 追击速度（中文注释）
        [SerializeField] private float searchSpeed = 2.2f; // 搜索速度（中文注释）

        [Header("Hurt")]
        [SerializeField] private float hurtSpeedMultiplier = 0.6f; // 受击减速倍率（中文注释）

        private float _speedMultiplier = 1f; // 当前速度倍率（中文注释）
        private EnemyMoveSpeedProfile _currentSpeedProfile = EnemyMoveSpeedProfile.Chase; // 当前速度档位（中文注释）

        private void Awake()
        {
            if (agent == null) agent = GetComponent<NavMeshAgent>();
        }

        // 移动到指定位置（中文注释）
        public void MoveTo(Vector3 position, EnemyMoveSpeedProfile profile)
        {
            if (!IsAgentReady()) return;
            _currentSpeedProfile = profile;
            ApplySpeed(profile);
            agent.isStopped = false;
            agent.SetDestination(position);
        }

        // 停止移动（中文注释）
        public void Stop()
        {
            if (!IsAgentReady()) return;
            agent.isStopped = true;
            if (agent.hasPath)
            {
                agent.ResetPath();
            }
        }

        // 进入受击减速（用于受击僵直期间“有移动则稍微减速”）（中文注释）
        public void EnterHurtSlow()
        {
            SetSpeedMultiplier(hurtSpeedMultiplier);
        }

        // 退出受击减速，恢复正常速度（中文注释）
        public void ExitHurtSlow()
        {
            SetSpeedMultiplier(1f);
        }

        // 是否到达目标（中文注释）
        public bool IsArrived()
        {
            if (!IsAgentReady()) return true;
            if (agent.pathPending) return false;
            float threshold = Mathf.Max(arriveDistance, agent.stoppingDistance);
            return agent.remainingDistance <= threshold;
        }

        // 获取当前速度（用于动画映射等）（中文注释）
        public float GetSpeed()
        {
            if (!IsAgentReady()) return 0f;
            return agent.velocity.magnitude;
        }

        private void SetSpeedMultiplier(float multiplier)
        {
            _speedMultiplier = Mathf.Clamp(multiplier, 0.05f, 1f);
            ApplySpeed(_currentSpeedProfile);
        }

        private void ApplySpeed(EnemyMoveSpeedProfile profile)
        {
            if (agent == null) return;
            float baseSpeed;
            switch (profile)
            {
                case EnemyMoveSpeedProfile.Patrol:
                    baseSpeed = patrolSpeed;
                    break;
                case EnemyMoveSpeedProfile.Search:
                    baseSpeed = searchSpeed;
                    break;
                default:
                    baseSpeed = chaseSpeed;
                    break;
            }

            agent.speed = baseSpeed * _speedMultiplier;
        }

        private bool IsAgentReady()
        {
            return agent != null && agent.enabled && agent.isOnNavMesh && agent.gameObject.activeInHierarchy;
        }
    }
}
