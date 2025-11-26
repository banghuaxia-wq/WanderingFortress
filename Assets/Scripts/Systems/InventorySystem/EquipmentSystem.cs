using System.Collections.Generic;
using UnityEngine;
using WF.Gameplay.Core.Data;
using WF.Gameplay.Systems.EventSystem;

namespace WF.Gameplay.Systems.InventorySystem
{
    public class EquipmentSystem : MonoBehaviour
    {
        public static EquipmentSystem Instance { get; private set; }
        private readonly Dictionary<EquipmentSlotType, ItemStack> _slots = new Dictionary<EquipmentSlotType, ItemStack>();
        private void Awake() { if (Instance != null && Instance != this) { Destroy(gameObject); return; } Instance = this; DontDestroyOnLoad(gameObject); }
        public ItemStack Get(EquipmentSlotType slot) { _slots.TryGetValue(slot, out var v); return v; }
        public bool Equip(EquipmentSlotType slot, ItemStack item) { if (item == null) return false; if (item.Type != ItemType.Armor && slot != EquipmentSlotType.Head) { if (item.Type != ItemType.Weapon && slot != EquipmentSlotType.Glove) { } }
            _slots[slot] = item; GameEvents.RaiseEquipmentUpdated(); return true; }
        public void Unequip(EquipmentSlotType slot) { if (_slots.ContainsKey(slot)) { _slots.Remove(slot); GameEvents.RaiseEquipmentUpdated(); } }
        public IReadOnlyDictionary<EquipmentSlotType, ItemStack> Slots => _slots;
    }
}
