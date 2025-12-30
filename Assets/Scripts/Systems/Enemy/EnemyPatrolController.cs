using UnityEngine;
using WF.Gameplay.Core.Data;

namespace WF.Gameplay.Systems.Enemy
{
    // 敌人巡逻控制器：管理随机巡逻点与到点停留计时（中文注释）
    public class EnemyPatrolController : MonoBehaviour
    {
        [SerializeField] private EnemyPatrolRandomSampler sampler; // 巡逻点采样器（中文注释）
        [SerializeField] private EnemyNavMovementController movement; // 移动控制器（中文注释）
        [SerializeField] private EnemyMoveSpeedProfile patrolSpeedProfile = EnemyMoveSpeedProfile.Patrol; // 巡逻速度档（中文注释）

        private Vector3 _currentPatrolPoint; // 当前巡逻目标点（中文注释）
        private bool _hasPatrolPoint; // 是否已选择巡逻点（中文注释）
        private float _idleEndTime; // 巡逻停留结束时间（中文注释）

        public Vector3 CurrentPatrolPoint => _currentPatrolPoint; // 当前巡逻点（中文注释）
        public bool HasPatrolPoint => _hasPatrolPoint; // 是否有巡逻点（中文注释）

        private void Awake()
        {
            if (sampler == null) sampler = GetComponent<EnemyPatrolRandomSampler>();
            if (movement == null) movement = GetComponent<EnemyNavMovementController>();
        }

        // 选择下一个巡逻点并开始移动（中文注释）
        public bool SelectNextPatrolPointAndMove()
        {
            if (sampler == null || movement == null) return false;
            if (!sampler.TryGetNextPoint(out var p)) return false;

            _currentPatrolPoint = p;
            _hasPatrolPoint = true;
            movement.MoveTo(_currentPatrolPoint, patrolSpeedProfile);
            return true;
        }

        // 行为树：是否到达巡逻点（中文注释）
        public bool IsArrived()
        {
            if (movement == null) return true;
            return movement.IsArrived();
        }

        // 行为树：开始到点停留计时（中文注释）
        public void StartIdle()
        {
            float seconds = sampler != null ? sampler.IdleSeconds : 0f;
            _idleEndTime = Time.time + Mathf.Max(0f, seconds);
        }

        // 行为树：停留时间是否结束（中文注释）
        public bool IsIdleDone()
        {
            return Time.time >= _idleEndTime;
        }

        // 行为树：清空当前巡逻点（中文注释）
        public void ClearPatrolPoint()
        {
            _hasPatrolPoint = false;
        }
    }
}

