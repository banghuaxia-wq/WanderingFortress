using System;
using System.Collections.Generic;
using UnityEngine;
using WF.Gameplay.Core.Data;
using WF.Gameplay.Core.Events;
using WF.Gameplay.Systems.ContainerSystem;

namespace WF.Gameplay.Systems.LootSystem
{
    public class LootManager : MonoBehaviour
    {
        public static LootManager Instance { get; private set; }

        [SerializeField] private ContainerLootConfigSO containerLootConfig;
        [SerializeField] private float baseLuck = 0f;
        [SerializeField] private float luckSoftCap = 0.75f;
        [SerializeField] private int globalSeed = 0;
        [SerializeField] private float zoneLuckSensitivity = 1f;
        [SerializeField] private bool autoCollectZonesOnStart = true;
        [SerializeField] private List<ZoneManager> zones = new List<ZoneManager>();

        private readonly Dictionary<string, LootSpawnPoint> _containerToPoint = new Dictionary<string, LootSpawnPoint>();
        private readonly List<LootSpawnPoint> _allPoints = new List<LootSpawnPoint>();
        private readonly List<RespawnJob> _respawns = new List<RespawnJob>();
        private int _sessionSeed;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            if (globalSeed == 0)
            {
                unchecked
                {
                    _sessionSeed = Environment.TickCount ^ Guid.NewGuid().GetHashCode();
                    if (_sessionSeed == 0) _sessionSeed = 1;
                }
            }
        }

        private void OnEnable()
        {
            EventBus.Subscribe<ContainerUpdatedEvent>(OnContainerUpdated);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<ContainerUpdatedEvent>(OnContainerUpdated);
        }

        private void Start()
        {
            Initialize();
        }

        [ContextMenu("Initialize Loot")]
        public void Initialize()
        {
            if (autoCollectZonesOnStart) CollectZones();
            CollectPointsFromZones();
            ActivatePoints();
        }

        private void Update()
        {
            UpdateRespawns();
        }

        private void CollectZones()
        {
            zones.Clear();
            var found = FindObjectsByType<ZoneManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < found.Length; i++)
            {
                if (found[i] != null) zones.Add(found[i]);
            }
        }

        private void CollectPointsFromZones()
        {
            _allPoints.Clear();
            _containerToPoint.Clear();

            for (int i = 0; i < zones.Count; i++)
            {
                var z = zones[i];
                if (z == null) continue;
                z.CollectPointsInBounds();
                var ps = z.Points;
                for (int j = 0; j < ps.Count; j++)
                {
                    var p = ps[j];
                    if (p == null) continue;
                    _allPoints.Add(p);
                }
            }
        }

        private void ActivatePoints()
        {
            for (int i = 0; i < _allPoints.Count; i++)
            {
                var p = _allPoints[i];
                if (p == null) continue;

                var rng = CreateRngForPoint(p);
                if (rng.NextDouble() > p.SpawnChance) continue;

                SpawnAndFill(p, rng, false);
            }
        }

        private void SpawnAndFill(LootSpawnPoint point, System.Random rng, bool isRespawn)
        {
            if (point == null) return;
            var cm = ContainerManager.Instance;
            if (cm == null) return;
            if (containerLootConfig == null) return;

            if (!point.HasSpawnedInstance)
            {
                point.SpawnContainer(rng);
            }

            var actor = point.GetSpawnedActor();
            if (actor == null) return;
            if (!point.AllowsContainerType(actor.Type)) return;

            string containerId = actor.ContainerId;
            if (string.IsNullOrEmpty(containerId)) return;

            var data = cm.Get(containerId);
            if (data == null)
            {
                data = new ContainerData { Id = containerId, Type = actor.Type, SlotLimit = actor.SlotLimit };
                cm.Register(data);
            }
            else
            {
                data.Type = actor.Type;
                data.SlotLimit = actor.SlotLimit;
            }

            if (isRespawn)
            {
                data.Items.Clear();
                data.HasGeneratedLoot = false;
            }

            if (data.HasGeneratedLoot) return;

            var table = containerLootConfig.GetLootTable(actor.Type);
            if (table == null)
            {
                data.HasGeneratedLoot = true;
                return;
            }

            int seed = ComputeContainerSeed(containerId);
            ContainerGenerator.FillContainerWithLoot(
                data,
                table,
                baseLuck,
                seed,
                luckSoftCap,
                point.ZoneLevel,
                zoneLuckSensitivity);

            data.HasGeneratedLoot = true;
            _containerToPoint[containerId] = point;
            EventBus.Publish(new ContainerUpdatedEvent(data));
        }

        private void OnContainerUpdated(ContainerUpdatedEvent e)
        {
            var d = e.Data;
            if (d == null) return;
            if (d.Items == null || d.Items.Count > 0) return;

            if (!_containerToPoint.TryGetValue(d.Id, out var point)) return;
            if (point == null) return;
            if (point.RespawnSeconds <= 0f) return;

            ScheduleRespawn(point, d.Id, point.RespawnSeconds);
        }

        private void ScheduleRespawn(LootSpawnPoint point, string containerId, float delaySeconds)
        {
            float at = Time.unscaledTime + Mathf.Max(0.01f, delaySeconds);
            for (int i = 0; i < _respawns.Count; i++)
            {
                if (_respawns[i].ContainerId == containerId)
                {
                    _respawns[i] = new RespawnJob(containerId, point, at);
                    return;
                }
            }
            _respawns.Add(new RespawnJob(containerId, point, at));
        }

        private void UpdateRespawns()
        {
            if (_respawns.Count == 0) return;
            var cm = ContainerManager.Instance;
            if (cm == null) return;

            float now = Time.unscaledTime;
            for (int i = _respawns.Count - 1; i >= 0; i--)
            {
                var job = _respawns[i];
                if (now < job.RespawnAtUnscaledTime) continue;
                _respawns.RemoveAt(i);

                if (job.Point == null) continue;
                var rng = CreateRngForPoint(job.Point);
                var data = cm.Get(job.ContainerId);
                if (data != null) data.HasGeneratedLoot = false;
                SpawnAndFill(job.Point, rng, true);
            }
        }

        private System.Random CreateRngForPoint(LootSpawnPoint point)
        {
            string key = point != null ? point.ContainerId : string.Empty;
            int hash = StableStringHash(key);
            int baseSeed = globalSeed == 0 ? _sessionSeed : globalSeed;
            int seed = baseSeed ^ hash;
            return seed == 0 ? new System.Random() : new System.Random(seed);
        }

        private int ComputeContainerSeed(string containerId)
        {
            int idHash = StableStringHash(containerId);
            int baseSeed = globalSeed == 0 ? _sessionSeed : globalSeed;
            return baseSeed ^ idHash;
        }

        private static int StableStringHash(string s)
        {
            if (string.IsNullOrEmpty(s)) return 0;
            unchecked
            {
                int hash = 23;
                for (int i = 0; i < s.Length; i++)
                {
                    hash = (hash * 31) + s[i];
                }
                return hash;
            }
        }

        private readonly struct RespawnJob
        {
            public readonly string ContainerId;
            public readonly LootSpawnPoint Point;
            public readonly float RespawnAtUnscaledTime;

            public RespawnJob(string containerId, LootSpawnPoint point, float respawnAtUnscaledTime)
            {
                ContainerId = containerId;
                Point = point;
                RespawnAtUnscaledTime = respawnAtUnscaledTime;
            }
        }
    }
}
