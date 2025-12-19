using UnityEngine;
using WF.Gameplay.Core.Data;
using WF.Gameplay.Core.Interfaces;

namespace WF.Gameplay.Systems.Inventory.Items
{
    public abstract class EquipmentItem : ItemBase, IEquippable
    {
        [SerializeField] private EquipmentSlotType slotType;
        
        public EquipmentSlotType SlotType => slotType;

        public virtual bool CanEquip(GameObject user) => true;
        
        public virtual void OnEquip(GameObject user) { }
        public virtual void OnUnequip(GameObject user) { }
    }
}
