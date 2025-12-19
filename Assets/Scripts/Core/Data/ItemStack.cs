using UnityEngine;
using WF.Gameplay.Core.Interfaces;

namespace WF.Gameplay.Core.Data
{
    [System.Serializable]
    public class ItemStack : IStackable
    {
        public string Id; // Unified ID field (formerly ItemId)
        public int Count;
        public IItem Item; // Reference to IItem instance

        // IStackable implementation
        public int CurrentStack { get => Count; set => Count = value; }
        public int MaxStack => Item is IStackable s ? s.MaxStack : (Item is IConsumable c ? c.MaxStack : 1);

        // Backward compatibility and helpers
        public string ItemId { get => Id; set => Id = value; }
        public ItemType Type => Item != null ? Item.Type : ItemType.Material;
        public EquipmentSlotType EquipSlot => Item is IEquippable e ? e.SlotType : (EquipmentSlotType)(-1);
        public float WeightPerUnit => Item != null ? Item.Weight : 0f;
        public Sprite Icon => Item != null ? Item.Icon : null;
        public bool IsUsable => Item != null && Item.CanUse(null);
        public float TotalWeight => Mathf.Max(0f, Count) * WeightPerUnit;

        public bool CanStackWith(IItem other)
        {
            if (Item == null || other == null) return false;
            return Item.ItemId == other.ItemId;
        }
        
        public static ItemStack Create(IItem item, int count)
        {
            return new ItemStack 
            { 
                Id = item.ItemId, 
                Item = item, 
                Count = count 
            };
        }
    }
}
