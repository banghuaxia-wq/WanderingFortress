using UnityEngine;
using WF.Gameplay.Core.Data;

namespace WF.Gameplay.Systems.Inventory.Items
{
    [CreateAssetMenu(fileName = "NewConsumableItem", menuName = "Items/Consumable")]
    public class BasicConsumableItem : ConsumableItem
    {
        protected override void OnValidate()
        {
            base.OnValidate();
            SetDefaultType(ItemType.Consumable);
        }
    }
}
