using UnityEngine;
using WF.Gameplay.Core.Data;
using WF.Gameplay.Core.Events;
using WF.Gameplay.Core.Interfaces;
using WF.Gameplay.Core.Utilities.Pooling;

namespace WF.Gameplay.Systems.Pochie
{
    public class PochieFactory : MonoBehaviour, IPochieFactory
    {
        public static PochieFactory Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // 根据Pochie数据创建实体（池化优先，返回实例的GameObject）
        public GameObject CreateFromData(SOPochie data, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (data == null || data.modelPrefab == null) return null;
            return CreateInternal(data.modelPrefab, data, position, rotation, parent);
        }

        // 根据预制体创建实体（可选传入数据以初始化属性）
        public GameObject CreateFromPrefab(GameObject prefab, SOPochie data, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (prefab == null) return null;
            return CreateInternal(prefab, data, position, rotation, parent);
        }

        // 静态便捷方法：依赖场景中的工厂实例
        public static GameObject CreateFromDataStatic(SOPochie data, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (Instance == null)
            {
                var go = new GameObject("PochieFactory");
                Instance = go.AddComponent<PochieFactory>();
                DontDestroyOnLoad(go);
            }
            return Instance.CreateFromData(data, position, rotation, parent);
        }

        // 静态便捷方法：依赖场景中的工厂实例
        public static GameObject CreateFromPrefabStatic(GameObject prefab, SOPochie data, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (Instance == null)
            {
                var go = new GameObject("PochieFactory");
                Instance = go.AddComponent<PochieFactory>();
                DontDestroyOnLoad(go);
            }
            return Instance.CreateFromPrefab(prefab, data, position, rotation, parent);
        }

        private GameObject CreateInternal(GameObject prefab, SOPochie data, Vector3 position, Quaternion rotation, Transform parent)
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
                if (po == null) po = go.AddComponent<PooledObject>();
                po.SourcePrefab = prefab;
            }

            // 组件确保与初始化
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

            // 事件：已生成
            EventBus.Publish(new PochieSpawnedEvent(go));
            return go;
        }
    }
}
