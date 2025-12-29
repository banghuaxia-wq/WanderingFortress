using UnityEngine;
using WF.Gameplay.Core.Data;

namespace WF.Gameplay.Systems.Inventory.Items
{
    [CreateAssetMenu(fileName = "NewFoodItem", menuName = "Items/Food")]
    public class FoodItem : ConsumableItem
    {
        protected override void OnValidate()
        {
            base.OnValidate();
            SetDefaultType(ItemType.Food);
        }
    }
}
