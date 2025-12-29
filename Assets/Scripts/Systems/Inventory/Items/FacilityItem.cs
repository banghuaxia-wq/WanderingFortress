using UnityEngine;
using WF.Gameplay.Core.Data;
using WF.Gameplay.Core.Interfaces;

namespace WF.Gameplay.Systems.Inventory.Items
{
    [CreateAssetMenu(fileName = "NewFacilityItem", menuName = "Items/Facility")]
    public class FacilityItem : ItemBase, IStackable
    {
        [SerializeField] private int maxStack = 10;

        public int CurrentStack { get; set; }
        public int MaxStack => maxStack;

        public bool CanStackWith(IItem other)
        {
            return other != null && other.ItemId == ItemId;
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            SetDefaultType(ItemType.Facility);
        }
    }
}
