using UnityEngine;
using WF.Gameplay.Core.Data;
using WF.Gameplay.Core.Interfaces;

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
            return PochieService.EnsureInstance().SpawnFromData(data, position, rotation, parent);
        }

        // 根据预制体创建实体（可选传入数据以初始化属性）
        public GameObject CreateFromPrefab(GameObject prefab, SOPochie data, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            return PochieService.EnsureInstance().SpawnFromPrefab(prefab, data, position, rotation, parent);
        }

        // 静态便捷方法：依赖场景中的工厂实例
        public static GameObject CreateFromDataStatic(SOPochie data, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            return PochieService.EnsureInstance().SpawnFromData(data, position, rotation, parent);
        }

        // 静态便捷方法：依赖场景中的工厂实例
        public static GameObject CreateFromPrefabStatic(GameObject prefab, SOPochie data, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            return PochieService.EnsureInstance().SpawnFromPrefab(prefab, data, position, rotation, parent);
        }
    }
}
