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
            for (int i = content.childCount - 1; i >= 0; i--)
            {
                var child = content.GetChild(i);
                if (child == null) continue;
                if (child.name == "SelectedIndicator") continue;

                var go = child.gameObject;
                var po = go.GetComponent<WF.Gameplay.Core.Utilities.Pooling.PooledObject>();
                if (po != null)
                {
                    slotPool.Release(go);
                }
            }

            var reusableSlots = new System.Collections.Generic.List<PackageUISlotController>(content.childCount);
            for (int i = 0; i < content.childCount; i++)
            {
                var child = content.GetChild(i);
                if (child == null) continue;
                if (child.name == "SelectedIndicator") continue;

                var slot = child.GetComponent<PackageUISlotController>();
                if (slot == null)
                {
                    slot = child.GetComponentInChildren<PackageUISlotController>(true);
                    if (slot == null && child.Find("Icon") != null)
                    {
                        slot = child.gameObject.AddComponent<PackageUISlotController>();
                    }
                }

                if (slot != null) reusableSlots.Add(slot);
            }

            for (int i = 0; i < sys.Count; i++)
            {
                PackageUISlotController slot;
                if (i < reusableSlots.Count)
                {
                    slot = reusableSlots[i];
                }
                else
                {
                    slot = slotPool.Get(slotPrefab, content);
                }

                if (slot == null) continue;
                slot.Bind(sys.Get(i));
                slot.SetMeta(WF.Gameplay.Core.Data.TransferSource.Hotbar, null, i);
            }
        }
    }
}
