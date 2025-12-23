using UnityEngine;
using WF.Gameplay.Core.Data;

namespace WF.Gameplay.Core.Interfaces
{
    public interface IPochieService
    {
        GameObject SpawnFromData(SOPochie data, Vector3 position, Quaternion rotation, Transform parent = null);
        GameObject SpawnFromPrefab(GameObject prefab, SOPochie data, Vector3 position, Quaternion rotation, Transform parent = null);
        bool Despawn(GameObject instance);
    }
}

