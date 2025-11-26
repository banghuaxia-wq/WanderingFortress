using System;
using System.Collections.Generic;
using UnityEngine;

namespace WF.Gameplay.Core.Utilities.Pooling
{
    public class PoolManager : MonoBehaviour
    {
        public static PoolManager Instance { get; private set; }

        [Serializable]
        public class PoolEntry
        {
            public string id;
            public GameObject prefab;
            public int initialSize = 10;
            public int maxSize = 100;
            public bool expandable = true;
            public Transform parent;
        }

        [SerializeField] private List<PoolEntry> pools = new List<PoolEntry>();
        [SerializeField] private Transform worldPoolRoot;
        [SerializeField] private Transform uiPoolRoot;
        [SerializeField] private Transform hudCanvasRoot;

        private readonly Dictionary<GameObject, Queue<GameObject>> _poolByPrefab = new Dictionary<GameObject, Queue<GameObject>>();
        private readonly Dictionary<string, GameObject> _prefabById = new Dictionary<string, GameObject>();
        private readonly Dictionary<GameObject, PoolEntry> _entryByPrefab = new Dictionary<GameObject, PoolEntry>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Prewarm();
        }

        private void Prewarm()
        {
            for (int i = 0; i < pools.Count; i++)
            {
                PoolEntry entry = pools[i];
                if (entry == null || entry.prefab == null) continue;
                _prefabById[entry.id] = entry.prefab;
                _entryByPrefab[entry.prefab] = entry;
                if (!_poolByPrefab.TryGetValue(entry.prefab, out var q))
                {
                    q = new Queue<GameObject>(Mathf.Max(1, entry.initialSize));
                    _poolByPrefab[entry.prefab] = q;
                }
                Transform parent = entry.parent != null ? entry.parent : (IsUIPrefab(entry.prefab) ? (hudCanvasRoot != null ? hudCanvasRoot : uiPoolRoot) : worldPoolRoot);
                for (int c = 0; c < entry.initialSize; c++)
                {
                    GameObject inst = Instantiate(entry.prefab, parent);
                    inst.SetActive(false);
                    EnsurePooledObject(inst, entry.prefab);
                    q.Enqueue(inst);
                }
            }
        }

        public GameObject Get(string id)
        {
            if (string.IsNullOrEmpty(id) || !_prefabById.TryGetValue(id, out var prefab)) return null;
            return Get(prefab);
        }

        public GameObject Get(GameObject prefab)
        {
            GameObject obj = AcquireInactive(prefab);
            if (obj == null) return null;
            obj.SetActive(true);
            var po = obj.GetComponent<PooledObject>();
            if (po != null) po.OnSpawned();
            return obj;
        }

        public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation, Transform parentOverride = null)
        {
            var obj = AcquireInactive(prefab);
            if (obj == null) return null;
            if (parentOverride != null)
            {
                obj.transform.SetParent(parentOverride, false);
            }
            obj.transform.SetPositionAndRotation(position, rotation);
            obj.SetActive(true);
            var po = obj.GetComponent<PooledObject>();
            if (po != null) po.OnSpawned();
            return obj;
        }

        public GameObject Get(string id, Vector3 position, Quaternion rotation, Transform parentOverride = null)
        {
            if (string.IsNullOrEmpty(id) || !_prefabById.TryGetValue(id, out var prefab)) return null;
            return Get(prefab, position, rotation, parentOverride);
        }

        private GameObject AcquireInactive(GameObject prefab)
        {
            if (prefab == null) return null;
            if (!_poolByPrefab.TryGetValue(prefab, out var q))
            {
                q = new Queue<GameObject>();
                _poolByPrefab[prefab] = q;
                _entryByPrefab[prefab] = new PoolEntry { id = prefab.name, prefab = prefab, initialSize = 0, maxSize = int.MaxValue, expandable = true, parent = worldPoolRoot };
            }
            GameObject obj = null;
            if (q.Count > 0)
            {
                obj = q.Dequeue();
            }
            else
            {
                var entry = _entryByPrefab[prefab];
                if (entry.expandable)
                {
                    Transform parent = entry.parent != null ? entry.parent : (IsUIPrefab(prefab) ? (hudCanvasRoot != null ? hudCanvasRoot : uiPoolRoot) : worldPoolRoot);
                    obj = Instantiate(prefab, parent);
                    EnsurePooledObject(obj, prefab);
                    obj.SetActive(false);
                }
            }
            return obj;
        }

        public void Release(GameObject obj)
        {
            if (obj == null) return;
            var po = obj.GetComponent<PooledObject>();
            if (po == null || po.SourcePrefab == null)
            {
                obj.SetActive(false);
                return;
            }
            GameObject prefab = po.SourcePrefab;
            if (!_poolByPrefab.TryGetValue(prefab, out var q))
            {
                q = new Queue<GameObject>();
                _poolByPrefab[prefab] = q;
            }
            var entry = _entryByPrefab.TryGetValue(prefab, out var e) ? e : null;
            Transform parent = (entry != null && entry.parent != null) ? entry.parent : (IsUIPrefab(obj) ? (hudCanvasRoot != null ? hudCanvasRoot : uiPoolRoot) : worldPoolRoot);
            obj.transform.SetParent(parent, false);
            po.OnDespawned();
            obj.SetActive(false);
            if (entry != null && q.Count >= entry.maxSize)
            {
                Destroy(obj);
            }
            else
            {
                q.Enqueue(obj);
            }
        }

        private void EnsurePooledObject(GameObject inst, GameObject prefab)
        {
            var po = inst.GetComponent<PooledObject>();
            if (po == null) po = inst.AddComponent<PooledObject>();
            po.SourcePrefab = prefab;
        }

        private static bool IsUIPrefab(GameObject go)
        {
            if (go == null) return false;
            return go.GetComponent<RectTransform>() != null || go.GetComponent<CanvasRenderer>() != null;
        }
    }
}

