using UnityEngine;
using WF.Gameplay.Core.Data;

namespace WF.Gameplay.Core.Interfaces
{
    public interface IPochieFactory
    {
        // 根据Pochie数据创建实体（返回实例的GameObject）
        GameObject CreateFromData(SOPochie data, Vector3 position, Quaternion rotation, Transform parent = null);

        // 根据预制体创建实体（可选传入数据以初始化属性）
        GameObject CreateFromPrefab(GameObject prefab, SOPochie data, Vector3 position, Quaternion rotation, Transform parent = null);
    }
}
