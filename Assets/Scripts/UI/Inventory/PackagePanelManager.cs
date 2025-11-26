using UnityEngine;
using WF.Gameplay.Systems.InventorySystem;
using WF.Gameplay.Systems.EventSystem;
using WF.Gameplay.Core.Data;

namespace WF.Gameplay.UI.Inventory
{
    public class PackagePanelManager : MonoBehaviour
    {
        [SerializeField] private Transform content;
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private UISlotPoolManager slotPool;
        private void OnEnable() { GameEvents.PlayerInventoryUpdated += Refresh; Refresh(); }
        private void OnDisable() { GameEvents.PlayerInventoryUpdated -= Refresh; }
        private void Refresh()
        {
            if (content == null || slotPrefab == null || slotPool == null) return;
            var inv = PlayerInventory.Instance; if (inv == null) return;
            for (int i = content.childCount - 1; i >= 0; i--)
            {
                var go = content.GetChild(i).gameObject;
                var po = go.GetComponent<WF.Gameplay.Core.Utilities.Pooling.PooledObject>();
                if (po != null) slotPool.Release(go); else GameObject.Destroy(go);
            }
            for (int i = 0; i < inv.Items.Count; i++)
            {
                var slot = slotPool.Get(slotPrefab, content);
                var item = inv.Items[i];
                slot.Bind(item);
                slot.SetMeta(TransferSource.Package, null, i);
            }
        }
    }
}
