using System.Collections.Generic;
using UnityEngine;
using WF.Gameplay.Core.Data;
using WF.Gameplay.Systems.InventorySystem;
using WF.Gameplay.Core.Events;

namespace WF.Gameplay.UI.Inventory
{
    public class EquipmentPanelManager : MonoBehaviour
    {
        [SerializeField] private Transform headSlot;
        [SerializeField] private Transform armorSlot;
        [SerializeField] private Transform gloveSlot;
        [SerializeField] private Transform pantSlot;
        [SerializeField] private Transform shoeSlot;
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private UISlotPoolManager slotPool;
        private readonly Dictionary<EquipmentSlotType, PackageUISlotController> _map = new Dictionary<EquipmentSlotType, PackageUISlotController>();
        
        private void OnEnable() 
        { 
            EventBus.Subscribe<EquipmentUpdatedEvent>(OnEquipmentUpdated); 
            InitSlots(); 
            Refresh(); 
        }
        private void OnDisable() 
        { 
            EventBus.Unsubscribe<EquipmentUpdatedEvent>(OnEquipmentUpdated); 
        }
        
        private void OnEquipmentUpdated(EquipmentUpdatedEvent e) { Refresh(); }
        
        private void InitSlots()
        {
            _map.Clear();
            if (slotPool == null || slotPrefab == null) return;
            SetupSlot(headSlot, EquipmentSlotType.Head);
            SetupSlot(armorSlot, EquipmentSlotType.Armor);
            SetupSlot(gloveSlot, EquipmentSlotType.Glove);
            SetupSlot(pantSlot, EquipmentSlotType.Pant);
            SetupSlot(shoeSlot, EquipmentSlotType.Shoe);
        }
        
        private void SetupSlot(Transform parent, EquipmentSlotType type)
        {
            if (parent == null) return;
            var child = parent.childCount > 0 ? parent.GetChild(0) : null;
            var ctrl = child != null ? child.GetComponent<PackageUISlotController>() : null;
            if (ctrl == null) ctrl = slotPool.Get(slotPrefab, parent);
            _map[type] = ctrl;
            ctrl.SetMeta(TransferSource.Equipment, null, (int)type);
        }
        
        private void Refresh()
        {
            var sys = EquipmentSystem.Instance; if (sys == null) return;
            foreach (var kv in _map) { var s = sys.Get(kv.Key); kv.Value.Bind(s); }
        }
    }
}
