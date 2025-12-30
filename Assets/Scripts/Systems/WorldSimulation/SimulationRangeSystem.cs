using System;
using System.Collections.Generic;
using UnityEngine;
using WF.Gameplay.Core.Data;
using WF.Gameplay.Core.Interfaces;
using WF.Gameplay.Systems.Player;

namespace WF.Gameplay.Systems.WorldSimulation
{
    public class SimulationRangeSystem : MonoBehaviour
    {
        public static SimulationRangeSystem Instance { get; private set; } // 运行时单例引用（用于便捷访问）

        [SerializeField] private SimulationRangeConfig config; // 距离分级配置（半径/分桶/刷新频率）
        [SerializeField] private Transform center; // 距离计算中心（通常为玩家）

        private readonly HashSet<ISimulationLodAgent> _agents = new HashSet<ISimulationLodAgent>(); // 已注册的可分级实体集合
        private readonly Dictionary<Vector2Int, List<ISimulationLodAgent>> _buckets = new Dictionary<Vector2Int, List<ISimulationLodAgent>>(); // 空间分桶：桶坐标 -> 实体列表
        private readonly Dictionary<ISimulationLodAgent, ForcedState> _forced = new Dictionary<ISimulationLodAgent, ForcedState>(); // 强制最低状态表（用于枪声/爆炸等短时唤醒）
        private readonly HashSet<ISimulationLodAgent> _candidates = new HashSet<ISimulationLodAgent>(); // 本次更新的候选实体集合（去重）

        private float _nextUpdateTime; // 下次允许刷新状态的时间点（节流用）

        private struct ForcedState
        {
            public SimulationLodState MinState; // 最低允许状态（中文注释）
            public float ExpiresAt; // 强制状态过期时间（Time.time）（中文注释）
        }

        // 初始化单例（中文注释）
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        // 启动时尝试自动绑定玩家为中心点（中文注释）
        private void Start()
        {
            ResolveCenter();
        }

        // 销毁时清理单例（中文注释）
        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // 注册一个可分级实体（中文注释）
        public void Register(ISimulationLodAgent agent)
        {
            if (agent == null) return;
            _agents.Add(agent);
        }

        // 注销一个可分级实体（中文注释）
        public void Unregister(ISimulationLodAgent agent)
        {
            if (agent == null) return;
            _agents.Remove(agent);
            _forced.Remove(agent);
        }

        // 手动指定距离中心（例如多角色/观战相机时使用）（中文注释）
        public void SetCenter(Transform value)
        {
            center = value;
        }

        // 基于某个世界位置对附近实体施加“最低状态”一段时间（用于枪声/爆炸/受击等事件唤醒）（中文注释）
        public void ForceMinimumState(Vector3 worldPosition, float activeRadiusMeters, float passiveRadiusMeters, float seconds)
        {
            if (config == null) return;

            float now = Time.time;
            float expiresAt = now + Mathf.Max(0f, seconds);

            float activeSqr = activeRadiusMeters * activeRadiusMeters;
            float passiveSqr = passiveRadiusMeters * passiveRadiusMeters;

            foreach (var agent in _agents)
            {
                if (agent == null || agent.Transform == null) continue;

                Vector3 delta = agent.Transform.position - worldPosition;
                float d2 = delta.x * delta.x + delta.z * delta.z;
                if (d2 <= activeSqr)
                {
                    SetForced(agent, SimulationLodState.Active, expiresAt);
                }
                else if (d2 <= passiveSqr)
                {
                    SetForced(agent, SimulationLodState.Passive, expiresAt);
                }
            }
        }

        // 按配置节流刷新：重建分桶 + 根据距离更新状态（中文注释）
        private void Update()
        {
            if (config == null) return;
            if (center == null) ResolveCenter();
            if (center == null) return;

            float now = Time.time;
            if (now < _nextUpdateTime) return;
            _nextUpdateTime = now + config.UpdateIntervalSeconds;

            RebuildBuckets();
            UpdateAgentStates(now);
        }

        // 更新所有候选实体的状态（中文注释）
        private void UpdateAgentStates(float now)
        {
            CleanupForced(now);
            CollectCandidates();

            float activeEnterSqr = config.ActiveEnterRadiusMeters * config.ActiveEnterRadiusMeters;
            float activeExitSqr = config.ActiveExitRadiusMeters * config.ActiveExitRadiusMeters;
            float passiveEnterSqr = config.PassiveEnterRadiusMeters * config.PassiveEnterRadiusMeters;
            float passiveExitSqr = config.PassiveExitRadiusMeters * config.PassiveExitRadiusMeters;

            Vector3 cpos = center.position;

            foreach (var agent in _candidates)
            {
                if (agent == null || agent.Transform == null) continue;

                Vector3 delta = agent.Transform.position - cpos;
                float d2 = delta.x * delta.x + delta.z * delta.z;

                SimulationLodState desired = DetermineDesiredState(agent.CurrentState, d2, activeEnterSqr, activeExitSqr, passiveEnterSqr, passiveExitSqr);
                desired = ApplyForcedMinimum(agent, desired, now);
                agent.ApplyState(desired);
            }

            _candidates.Clear();
        }

        // 根据当前状态与距离（含回滞阈值）计算目标状态（中文注释）
        private static SimulationLodState DetermineDesiredState(
            SimulationLodState current,
            float distanceSqr,
            float activeEnterSqr,
            float activeExitSqr,
            float passiveEnterSqr,
            float passiveExitSqr)
        {
            switch (current)
            {
                case SimulationLodState.Active:
                    if (distanceSqr <= activeExitSqr) return SimulationLodState.Active;
                    return distanceSqr <= passiveEnterSqr ? SimulationLodState.Passive : SimulationLodState.Dormant;

                case SimulationLodState.Passive:
                    if (distanceSqr <= activeEnterSqr) return SimulationLodState.Active;
                    if (distanceSqr <= passiveExitSqr) return SimulationLodState.Passive;
                    return SimulationLodState.Dormant;

                default:
                    if (distanceSqr <= activeEnterSqr) return SimulationLodState.Active;
                    if (distanceSqr <= passiveEnterSqr) return SimulationLodState.Passive;
                    return SimulationLodState.Dormant;
            }
        }

        // 重建空间分桶：将所有已注册实体按桶坐标归类（中文注释）
        private void RebuildBuckets()
        {
            _buckets.Clear();

            float bucketSize = config.BucketSizeMeters;
            foreach (var agent in _agents)
            {
                if (agent == null || agent.Transform == null) continue;
                Vector2Int key = ToBucketKey(agent.Transform.position, bucketSize);

                if (!_buckets.TryGetValue(key, out var list))
                {
                    list = new List<ISimulationLodAgent>(8);
                    _buckets.Add(key, list);
                }
                list.Add(agent);
            }
        }

        // 基于中心点半径，收集需要计算距离的候选实体（中文注释）
        private void CollectCandidates()
        {
            Vector3 cpos = center.position;
            float bucketSize = config.BucketSizeMeters;
            int r = Mathf.CeilToInt(config.MaxRadiusMeters / bucketSize);

            Vector2Int centerKey = ToBucketKey(cpos, bucketSize);

            for (int dx = -r; dx <= r; dx++)
            {
                for (int dy = -r; dy <= r; dy++)
                {
                    Vector2Int key = new Vector2Int(centerKey.x + dx, centerKey.y + dy);
                    if (!_buckets.TryGetValue(key, out var list)) continue;

                    for (int i = 0; i < list.Count; i++)
                    {
                        if (list[i] == null) continue;
                        _candidates.Add(list[i]);
                    }
                }
            }
        }

        // 将世界坐标映射为分桶坐标（中文注释）
        private static Vector2Int ToBucketKey(Vector3 worldPosition, float bucketSize)
        {
            int x = Mathf.FloorToInt(worldPosition.x / bucketSize);
            int y = Mathf.FloorToInt(worldPosition.z / bucketSize);
            return new Vector2Int(x, y);
        }

        // 自动寻找中心点：优先Tag=Player，其次PlayerMove（中文注释）
        private void ResolveCenter()
        {
            if (center != null) return;

            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) center = player.transform;
            if (center != null) return;

            var move = FindObjectOfType<PlayerMove>();
            if (move != null) center = move.transform;
        }

        // 清理过期的强制状态（中文注释）
        private void CleanupForced(float now)
        {
            if (_forced.Count == 0) return;

            var toRemove = (List<ISimulationLodAgent>)null;
            foreach (var kvp in _forced)
            {
                if (kvp.Value.ExpiresAt > now) continue;
                toRemove ??= new List<ISimulationLodAgent>();
                toRemove.Add(kvp.Key);
            }

            if (toRemove == null) return;
            for (int i = 0; i < toRemove.Count; i++)
            {
                _forced.Remove(toRemove[i]);
            }
        }

        // 写入/合并强制最低状态（中文注释）
        private void SetForced(ISimulationLodAgent agent, SimulationLodState minState, float expiresAt)
        {
            if (_forced.TryGetValue(agent, out var existing))
            {
                var state = (SimulationLodState)Mathf.Max((int)existing.MinState, (int)minState);
                _forced[agent] = new ForcedState { MinState = state, ExpiresAt = Mathf.Max(existing.ExpiresAt, expiresAt) };
                return;
            }

            _forced.Add(agent, new ForcedState { MinState = minState, ExpiresAt = expiresAt });
        }

        // 将强制最低状态应用到目标状态上（中文注释）
        private SimulationLodState ApplyForcedMinimum(ISimulationLodAgent agent, SimulationLodState desired, float now)
        {
            if (!_forced.TryGetValue(agent, out var forced)) return desired;
            if (forced.ExpiresAt <= now)
            {
                _forced.Remove(agent);
                return desired;
            }

            return (SimulationLodState)Mathf.Max((int)desired, (int)forced.MinState);
        }
    }
}
