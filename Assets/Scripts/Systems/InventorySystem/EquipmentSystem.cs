using System.Collections.Generic;
using UnityEngine;
using WF.Gameplay.Core.Data;
using WF.Gameplay.Core.Events;
using WF.Gameplay.Core.Interfaces;

namespace WF.Gameplay.Systems.InventorySystem
{
    public class EquipmentSystem : MonoBehaviour
    {
        public static EquipmentSystem Instance { get; private set; }
        private readonly Dictionary<EquipmentSlotType, ItemStack> _slots = new Dictionary<EquipmentSlotType, ItemStack>();
        private void Awake() { if (Instance != null && Instance != this) { Destroy(gameObject); return; } Instance = this; DontDestroyOnLoad(gameObject); }
        public ItemStack Get(EquipmentSlotType slot) { _slots.TryGetValue(slot, out var v); return v; }
        
        public bool Equip(EquipmentSlotType slot, ItemStack item) 
        { 
            if (item == null) return false;
            
            // Check if item implements IEquippable
            if (item.Item is IEquippable equippable)
            {
                if (equippable.SlotType != slot) return false;
                if (!equippable.CanEquip(gameObject)) return false;
                
                equippable.OnEquip(gameObject);
            }
            else
            {
                // Fallback or reject if strict
                // For now allow if slots match (legacy behavior)
                if (item.EquipSlot != slot) return false;
            }

            _slots[slot] = item; 
            EventBus.Publish(new EquipmentUpdatedEvent()); 
            return true; 
        }
        
        public void Unequip(EquipmentSlotType slot) 
        { 
            if (_slots.ContainsKey(slot)) 
            { 
                var item = _slots[slot];
                if (item != null && item.Item is IEquippable equippable)
                {
                    equippable.OnUnequip(gameObject);
                }
                
                _slots.Remove(slot); 
                EventBus.Publish(new EquipmentUpdatedEvent()); 
            } 
        }
        public IReadOnlyDictionary<EquipmentSlotType, ItemStack> Slots => _slots;
    }
}
