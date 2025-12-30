using UnityEngine;
using WF.Gameplay.Core.Data;
using WF.Gameplay.Core.Interfaces;

namespace WF.Gameplay.Systems.Hatch
{
    public class HatchFactory : MonoBehaviour, IHatchFactory
    {
        public static HatchFactory Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // 根据Hatch数据创建实体（池化优先，返回实例的GameObject）
        public GameObject CreateFromData(SOHatch data, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            return HatchService.EnsureInstance().SpawnFromData(data, position, rotation, parent);
        }

        // 根据预制体创建实体（可选传入数据以初始化属性）
        public GameObject CreateFromPrefab(GameObject prefab, SOHatch data, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            return HatchService.EnsureInstance().SpawnFromPrefab(prefab, data, position, rotation, parent);
        }

        // 静态便捷方法：依赖场景中的工厂实例
        public static GameObject CreateFromDataStatic(SOHatch data, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            return HatchService.EnsureInstance().SpawnFromData(data, position, rotation, parent);
        }

        // 静态便捷方法：依赖场景中的工厂实例
        public static GameObject CreateFromPrefabStatic(GameObject prefab, SOHatch data, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            return HatchService.EnsureInstance().SpawnFromPrefab(prefab, data, position, rotation, parent);
        }
    }
}

