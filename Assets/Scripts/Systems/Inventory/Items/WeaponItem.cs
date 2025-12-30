using UnityEngine;
using WF.Gameplay.Core.Data;
using WF.Gameplay.Core.Interfaces;

namespace WF.Gameplay.Systems.Inventory.Items
{
    public abstract class WeaponItem : ItemBase, IEquippable
    {
        [SerializeField] private EquipmentSlotType slotType = EquipmentSlotType.MainHand;
        
        public EquipmentSlotType SlotType => slotType;

        public virtual bool CanEquip(GameObject user) => true;
        
        public virtual void OnEquip(GameObject user) 
        {
            // Logic handled by WeaponManager usually
        }
        
        public virtual void OnUnequip(GameObject user) 
        {
            // Logic handled by WeaponManager usually
        }
    }
}
