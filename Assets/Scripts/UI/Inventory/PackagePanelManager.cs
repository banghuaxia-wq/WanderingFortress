using UnityEngine;
using WF.Gameplay.Systems.InventorySystem;
using WF.Gameplay.Core.Events;
using WF.Gameplay.Core.Data;

namespace WF.Gameplay.UI.Inventory
{
    public class PackagePanelManager : MonoBehaviour
    {
        [SerializeField] private Transform content;
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private UISlotPoolManager slotPool;
        
        private void OnEnable() 
        { 
            EventBus.Subscribe<PlayerInventoryUpdatedEvent>(OnInventoryUpdated); 
            Refresh(); 
        }
        
        private void OnDisable() 
        { 
            EventBus.Unsubscribe<PlayerInventoryUpdatedEvent>(OnInventoryUpdated); 
        }
        
        private void OnInventoryUpdated(PlayerInventoryUpdatedEvent e)
        {
            Refresh();
        }
        
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
                slot.SetSlotType(UISlotType.Package);
                slot.SetMeta(TransferSource.Package, null, i);
                slot.Bind(item);
            }
        }
    }
}
