using UnityEngine;
using System.Collections.Generic;
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

        private readonly List<PackageUISlotController> _slots = new List<PackageUISlotController>();
        
        private void OnEnable() 
        { 
            EventBus.Subscribe<PlayerInventoryUpdatedEvent>(OnInventoryUpdated); 
            EnsureSlots();
            RefreshBindings(); 
        }
        
        private void OnDisable() 
        { 
            EventBus.Unsubscribe<PlayerInventoryUpdatedEvent>(OnInventoryUpdated); 
        }
        
        private void OnInventoryUpdated(PlayerInventoryUpdatedEvent e)
        {
            EnsureSlots();
            RefreshBindings();
        }
        
        private void EnsureSlots()
        {
            if (content == null || slotPrefab == null || slotPool == null) return;
            var inv = PlayerInventory.Instance; if (inv == null) return;

            if (_slots.Count == 0 && content.childCount > 0)
            {
                for (int i = 0; i < content.childCount; i++)
                {
                    var child = content.GetChild(i);
                    var slot = child.GetComponent<PackageUISlotController>();
                    if (slot != null) _slots.Add(slot);
                }
            }

            int desiredSlotCount = Mathf.Max(0, inv.Capacity);

            while (_slots.Count > desiredSlotCount)
            {
                int lastIndex = _slots.Count - 1;
                var slot = _slots[lastIndex];
                _slots.RemoveAt(lastIndex);
                if (slot != null) slotPool.Release(slot.gameObject);
            }

            while (_slots.Count < desiredSlotCount)
            {
                var slot = slotPool.Get(slotPrefab, content);
                if (slot == null) break;
                slot.SetSlotType(UISlotType.Package);
                slot.SetMeta(TransferSource.Package, null, _slots.Count);
                slot.Bind((ItemStack)null);
                _slots.Add(slot);
            }
        }

        private void RefreshBindings()
        {
            if (content == null || slotPrefab == null || slotPool == null) return;
            var inv = PlayerInventory.Instance; if (inv == null) return;

            int itemCount = inv.Items != null ? inv.Items.Count : 0;
            for (int i = 0; i < _slots.Count; i++)
            {
                var slot = _slots[i];
                if (slot == null) continue;
                slot.SetSlotType(UISlotType.Package);
                slot.SetMeta(TransferSource.Package, null, i);
                if (i < itemCount) slot.Bind(inv.Items[i]);
                else slot.Bind((ItemStack)null);
            }
        }
    }
}
