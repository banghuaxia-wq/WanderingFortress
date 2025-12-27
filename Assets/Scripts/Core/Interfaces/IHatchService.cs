using UnityEngine;
using WF.Gameplay.Core.Data;

namespace WF.Gameplay.Core.Interfaces
{
    public interface IHatchService
    {
        GameObject SpawnFromData(SOHatch data, Vector3 position, Quaternion rotation, Transform parent = null);
        GameObject SpawnFromPrefab(GameObject prefab, SOHatch data, Vector3 position, Quaternion rotation, Transform parent = null);
        bool Despawn(GameObject instance);
    }
}


