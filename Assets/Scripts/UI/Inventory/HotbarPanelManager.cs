using UnityEngine;
using WF.Gameplay.Systems.InventorySystem;
using WF.Gameplay.Core.Events;

namespace WF.Gameplay.UI.Inventory
{
    public class HotbarPanelManager : MonoBehaviour
    {
        [SerializeField] private Transform content;
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private UISlotPoolManager slotPool;
        
        private void OnEnable() 
        { 
            EventBus.Subscribe<HotbarUpdatedEvent>(OnHotbarUpdated); 
            Refresh(); 
        }
        private void OnDisable() 
        { 
            EventBus.Unsubscribe<HotbarUpdatedEvent>(OnHotbarUpdated); 
        }
        
        private void OnHotbarUpdated(HotbarUpdatedEvent e) { Refresh(); }
        
        private void Refresh()
        {
            if (content == null) content = transform;
            if (slotPrefab == null || slotPool == null) return;
            var sys = HotbarSystem.Instance; if (sys == null) return;
            int childCount = content.childCount;
            for (int i = childCount - 1; i >= 0; i--) 
            { 
                var go = content.GetChild(i).gameObject; 
                var po = go.GetComponent<WF.Gameplay.Core.Utilities.Pooling.PooledObject>();
                if (po != null) slotPool.Release(go); else Destroy(go);
            }
            for (int i = 0; i < sys.Count; i++)
            {
                var slot = slotPool.Get(slotPrefab, content);
                slot.Bind(sys.Get(i));
                slot.SetMeta(WF.Gameplay.Core.Data.TransferSource.Hotbar, null, i);
            }
        }
    }
}
