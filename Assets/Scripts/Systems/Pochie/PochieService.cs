using UnityEngine;
using WF.Gameplay.Core.Data;
using WF.Gameplay.Core.Events;
using WF.Gameplay.Core.Interfaces;
using WF.Gameplay.Core.Utilities.Pooling;

namespace WF.Gameplay.Systems.Pochie
{
    public class PochieService : MonoBehaviour, IPochieService
    {
        public static IPochieService Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public static IPochieService EnsureInstance()
        {
            if (Instance != null)
            {
                return Instance;
            }

            var go = new GameObject("PochieService");
            Instance = go.AddComponent<PochieService>();
            DontDestroyOnLoad(go);
            return Instance;
        }

        public GameObject SpawnFromData(SOPochie data, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (data == null || data.modelPrefab == null)
            {
                return null;
            }

            return SpawnInternal(data.modelPrefab, data, position, rotation, parent);
        }

        public GameObject SpawnFromPrefab(GameObject prefab, SOPochie data, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (prefab == null)
            {
                return null;
            }

            return SpawnInternal(prefab, data, position, rotation, parent);
        }

        public bool Despawn(GameObject instance)
        {
            if (instance == null)
            {
                return false;
            }

            if (PoolManager.Instance != null)
            {
                PoolManager.Instance.Release(instance);
                return true;
            }

            Destroy(instance);
            return true;
        }

        private static GameObject SpawnInternal(GameObject prefab, SOPochie data, Vector3 position, Quaternion rotation, Transform parent)
        {
            GameObject go = null;

            if (PoolManager.Instance != null)
            {
                go = PoolManager.Instance.Get(prefab, position, rotation, parent);
            }

            if (go == null)
            {
                go = Object.Instantiate(prefab, position, rotation, parent);
                var po = go.GetComponent<PooledObject>();
                if (po == null)
                {
                    po = go.AddComponent<PooledObject>();
                }
                po.SourcePrefab = prefab;
            }

            if (go.GetComponent<WF.Gameplay.Systems.Buffs.BuffManager>() == null)
            {
                go.AddComponent<WF.Gameplay.Systems.Buffs.BuffManager>();
            }

            var combat = go.GetComponent<PochieCombatController>();
            if (combat == null)
            {
                combat = go.AddComponent<PochieCombatController>();
            }

            if (data != null)
            {
                combat.SetStats(data);
            }

            combat.InitializeCombatState();

            EventBus.Publish(new PochieSpawnedEvent(go));
            return go;
        }
    }
}

