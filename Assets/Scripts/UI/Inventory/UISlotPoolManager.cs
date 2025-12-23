using UnityEngine;
using WF.Gameplay.Core.Utilities.Pooling;

namespace WF.Gameplay.UI.Inventory
{
    public class UISlotPoolManager : MonoBehaviour
    {
        public PackageUISlotController Get(GameObject slotPrefab, Transform parent)
        {
            if (slotPrefab == null || parent == null) return null;

            GameObject go;
            if (PoolManager.Instance != null)
            {
                go = PoolManager.Instance.Get(slotPrefab, Vector3.zero, Quaternion.identity, parent);
            }
            else
            {
                go = Instantiate(slotPrefab, parent);
            }

            if (go == null) return null;

            var c = go.GetComponent<PackageUISlotController>();
            if (c == null) c = go.AddComponent<PackageUISlotController>();
            return c;
        }

        public void Release(GameObject obj)
        {
            if (obj == null) return;
            if (PoolManager.Instance != null)
            {
                PoolManager.Instance.Release(obj);
            }
            else
            {
                Destroy(obj);
            }
        }
    }
}
