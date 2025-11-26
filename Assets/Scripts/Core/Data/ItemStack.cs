using UnityEngine;

namespace WF.Gameplay.Core.Data
{
    public class ItemStack
    {
        public string ItemId;
        public ItemType Type;
        public EquipmentSlotType EquipSlot;
        public int Count;
        public int MaxStack;
        public float WeightPerUnit;
        public Sprite Icon;
        public bool IsUsable;
        public float TotalWeight => Mathf.Max(0f, Count) * Mathf.Max(0f, WeightPerUnit);
    }
}
