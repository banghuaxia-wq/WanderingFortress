using UnityEngine;
using WF.Gameplay.Core.Data;

namespace WF.Gameplay.Core.Interfaces
{
    public interface IHatchFactory
    {
        // 根据Hatch数据创建实体（返回实例的GameObject）
        GameObject CreateFromData(SOHatch data, Vector3 position, Quaternion rotation, Transform parent = null);

        // 根据预制体创建实体（可选传入数据以初始化属性）
        GameObject CreateFromPrefab(GameObject prefab, SOHatch data, Vector3 position, Quaternion rotation, Transform parent = null);
    }
}

