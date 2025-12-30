using UnityEngine;
using WF.Gameplay.Core.Data;
using WF.Gameplay.Core.Interfaces;

namespace WF.Gameplay.Systems.Inventory.Items
{
    [CreateAssetMenu(fileName = "NewBuildingItem", menuName = "Items/Building")]
    public class BuildingItem : ItemBase, IStackable
    {
        [SerializeField] private int maxStack = 10;
        [SerializeField] private BuildingDefinition buildingDefinition; // 对应的建筑定义（用于建造模式放置预制体）（中文注释）

        public int CurrentStack { get; set; }
        public int MaxStack => maxStack;
        public BuildingDefinition BuildingDefinition => buildingDefinition; // 建筑定义引用（中文注释）

        public bool CanStackWith(IItem other)
        {
            return other != null && other.ItemId == ItemId;
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            SetDefaultType(ItemType.Building);
        }
    }
}
