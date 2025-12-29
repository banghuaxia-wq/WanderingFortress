using System;
using System.Collections.Generic;
using UnityEngine;
using WF.Gameplay.Core.Data;
using WF.Gameplay.Systems.ContainerSystem;

namespace WF.Gameplay.Systems.LootSystem
{
    public class LootSpawnPoint : MonoBehaviour
    {
        [Range(0f, 1f)]
        [SerializeField] private float spawnChance = 1f;
        [Min(0f)]
        [SerializeField] private float respawnSeconds = 0f;
        [SerializeField] private string containerIdOverride;
        [SerializeField] private List<ContainerType> allowedContainerTypes = new List<ContainerType>();
        [SerializeField] private List<GameObject> containerPrefabs = new List<GameObject>();

        private ZoneManager _zone;
        private string _resolvedContainerId;
        private GameObject _spawnedInstance;
        private float _nextRespawnAtUnscaledTime;

        public float SpawnChance => Mathf.Clamp01(spawnChance);
        public float RespawnSeconds => Mathf.Max(0f, respawnSeconds);
        public int ZoneLevel => _zone != null ? _zone.ZoneLevel : 1;
        public string ZoneId => _zone != null ? _zone.ZoneId : string.Empty;
        public string ContainerId => string.IsNullOrEmpty(_resolvedContainerId) ? ResolveContainerId() : _resolvedContainerId;

        public void SetZone(ZoneManager zone)
        {
            _zone = zone;
        }

        public bool HasSpawnedInstance => _spawnedInstance != null;

        public void MarkForRespawn(float delaySeconds)
        {
            float d = Mathf.Max(0f, delaySeconds);
            _nextRespawnAtUnscaledTime = Time.unscaledTime + d;
        }

        public bool IsRespawnReady()
        {
            if (RespawnSeconds <= 0f) return false;
            if (!HasSpawnedInstance) return true;
            return Time.unscaledTime >= _nextRespawnAtUnscaledTime && _nextRespawnAtUnscaledTime > 0f;
        }

        public GameObject SpawnContainer(System.Random rng)
        {
            var prefab = PickPrefab(rng);
            if (prefab == null) return null;

            var id = ResolveContainerId();
            _resolvedContainerId = id;

            var go = Instantiate(prefab, transform.position, transform.rotation, null);
            go.name = id;
            _spawnedInstance = go;
            _nextRespawnAtUnscaledTime = 0f;
            return go;
        }

        public ContainerActor GetSpawnedActor()
        {
            if (_spawnedInstance == null) return null;
            return _spawnedInstance.GetComponentInChildren<ContainerActor>(true);
        }

        public bool AllowsContainerType(ContainerType type)
        {
            if (allowedContainerTypes == null || allowedContainerTypes.Count == 0) return true;
            for (int i = 0; i < allowedContainerTypes.Count; i++)
            {
                if (allowedContainerTypes[i] == type) return true;
            }
            return false;
        }

        private GameObject PickPrefab(System.Random rng)
        {
            if (containerPrefabs == null || containerPrefabs.Count == 0) return null;
            if (allowedContainerTypes == null || allowedContainerTypes.Count == 0) return containerPrefabs[rng.Next(0, containerPrefabs.Count)];

            var candidates = ListPool<GameObject>.Get();
            for (int i = 0; i < containerPrefabs.Count; i++)
            {
                var p = containerPrefabs[i];
                if (p == null) continue;
                var actor = p.GetComponentInChildren<ContainerActor>(true);
                if (actor == null) continue;
                if (!AllowsContainerType(actor.Type)) continue;
                candidates.Add(p);
            }

            GameObject chosen = candidates.Count > 0 ? candidates[rng.Next(0, candidates.Count)] : null;
            ListPool<GameObject>.Release(candidates);
            return chosen;
        }

        private string ResolveContainerId()
        {
            if (!string.IsNullOrEmpty(containerIdOverride)) return containerIdOverride;
            string path = GetHierarchyPath(transform);
            string zone = string.IsNullOrEmpty(ZoneId) ? "Zone" : ZoneId;
            return $"{zone}:{path}";
        }

        private static string GetHierarchyPath(Transform t)
        {
            if (t == null) return "Null";
            var parts = ListPool<string>.Get();
            while (t != null)
            {
                parts.Add(SanitizeIdPart(t.name));
                t = t.parent;
            }
            parts.Reverse();
            string result = string.Join("/", parts);
            ListPool<string>.Release(parts);
            return result;
        }

        private static string SanitizeIdPart(string s)
        {
            if (string.IsNullOrEmpty(s)) return "Empty";
            s = s.Replace(":", "_").Replace("/", "_").Replace("\\", "_");
            return s;
        }

        private static class ListPool<T>
        {
            private static readonly Stack<List<T>> Pool = new Stack<List<T>>();

            public static List<T> Get()
            {
                if (Pool.Count > 0)
                {
                    var list = Pool.Pop();
                    list.Clear();
                    return list;
                }
                return new List<T>();
            }

            public static void Release(List<T> list)
            {
                if (list == null) return;
                list.Clear();
                Pool.Push(list);
            }
        }
    }
}

