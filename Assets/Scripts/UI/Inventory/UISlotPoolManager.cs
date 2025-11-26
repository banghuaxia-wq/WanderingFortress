using UnityEngine;
using WF.Gameplay.Core.Utilities.Pooling;

namespace WF.Gameplay.UI.Inventory
{
    public class UISlotPoolManager : MonoBehaviour
    {
        public PackageUISlotController Get(GameObject slotPrefab, Transform parent) { if (PoolManager.Instance == null) return null; var go = PoolManager.Instance.Get(slotPrefab, Vector3.zero, Quaternion.identity, parent); if (go == null) return null; var c = go.GetComponent<PackageUISlotController>(); if (c == null) c = go.AddComponent<PackageUISlotController>(); return c; }
        public void Release(GameObject obj) { if (PoolManager.Instance == null || obj == null) return; PoolManager.Instance.Release(obj); }
    }
}
