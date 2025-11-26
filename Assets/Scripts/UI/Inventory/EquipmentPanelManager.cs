using System.Collections.Generic;
using UnityEngine;
using WF.Gameplay.Core.Data;
using WF.Gameplay.Systems.InventorySystem;
using WF.Gameplay.Systems.EventSystem;

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
        private void OnEnable() { GameEvents.EquipmentUpdated += Refresh; InitSlots(); Refresh(); }
        private void OnDisable() { GameEvents.EquipmentUpdated -= Refresh; }
        private void InitSlots()
        {
            _map.Clear();
            if (slotPool == null || slotPrefab == null) return;
            if (headSlot != null)
            {
                var child = headSlot.childCount > 0 ? headSlot.GetChild(0) : null;
                var ctrl = child != null ? child.GetComponent<PackageUISlotController>() : null;
                if (ctrl == null) ctrl = slotPool.Get(slotPrefab, headSlot);
                _map[EquipmentSlotType.Head] = ctrl;
            }
            if (armorSlot != null)
            {
                var child = armorSlot.childCount > 0 ? armorSlot.GetChild(0) : null;
                var ctrl = child != null ? child.GetComponent<PackageUISlotController>() : null;
                if (ctrl == null) ctrl = slotPool.Get(slotPrefab, armorSlot);
                _map[EquipmentSlotType.Armor] = ctrl;
            }
            if (gloveSlot != null)
            {
                var child = gloveSlot.childCount > 0 ? gloveSlot.GetChild(0) : null;
                var ctrl = child != null ? child.GetComponent<PackageUISlotController>() : null;
                if (ctrl == null) ctrl = slotPool.Get(slotPrefab, gloveSlot);
                _map[EquipmentSlotType.Glove] = ctrl;
            }
            if (pantSlot != null)
            {
                var child = pantSlot.childCount > 0 ? pantSlot.GetChild(0) : null;
                var ctrl = child != null ? child.GetComponent<PackageUISlotController>() : null;
                if (ctrl == null) ctrl = slotPool.Get(slotPrefab, pantSlot);
                _map[EquipmentSlotType.Pant] = ctrl;
            }
            if (shoeSlot != null)
            {
                var child = shoeSlot.childCount > 0 ? shoeSlot.GetChild(0) : null;
                var ctrl = child != null ? child.GetComponent<PackageUISlotController>() : null;
                if (ctrl == null) ctrl = slotPool.Get(slotPrefab, shoeSlot);
                _map[EquipmentSlotType.Shoe] = ctrl;
            }
        }
        private void Refresh()
        {
            var sys = EquipmentSystem.Instance; if (sys == null) return;
            foreach (var kv in _map) { var s = sys.Get(kv.Key); kv.Value.Bind(s); }
        }
    }
}
