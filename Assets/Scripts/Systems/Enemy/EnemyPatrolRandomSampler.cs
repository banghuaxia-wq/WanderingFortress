using UnityEngine;
using UnityEngine.AI;

namespace WF.Gameplay.Systems.Enemy
{
    // 随机巡逻点采样：以出生点为中心在NavMesh上取随机点（中文注释）
    public class EnemyPatrolRandomSampler : MonoBehaviour
    {
        [Header("Patrol")]
        [SerializeField] private float patrolRadius = 6f; // 巡逻半径（中文注释）
        [SerializeField] private float idleSeconds = 1.5f; // 到点后停留时间（中文注释）
        [SerializeField] private int sampleTries = 8; // 采样重试次数（中文注释）
        [SerializeField] private Transform centerOverride; // 巡逻中心覆盖（为空则用出生点）（中文注释）

        private Vector3 _spawnPosition; // 出生点位置缓存（中文注释）

        public float IdleSeconds => idleSeconds; // 停留时间（中文注释）

        private void Awake()
        {
            _spawnPosition = transform.position;
        }

        // 获取下一个巡逻点（中文注释）
        public bool TryGetNextPoint(out Vector3 point)
        {
            Vector3 center = centerOverride != null ? centerOverride.position : _spawnPosition;
            for (int i = 0; i < Mathf.Max(1, sampleTries); i++)
            {
                Vector2 rnd = Random.insideUnitCircle * Mathf.Max(0f, patrolRadius);
                Vector3 candidate = new Vector3(center.x + rnd.x, center.y, center.z + rnd.y);

                if (NavMesh.SamplePosition(candidate, out var hit, 2f, NavMesh.AllAreas))
                {
                    point = hit.position;
                    return true;
                }
            }

            point = center;
            return false;
        }
    }
}

