using UnityEngine;
using WF.Gameplay.Core.Data;

namespace WF.Gameplay.Systems.Inventory.Items
{
    [CreateAssetMenu(fileName = "NewThrowableItem", menuName = "Items/Throwable")]
    public class ThrowableItem : ConsumableItem
    {
        protected override void OnValidate()
        {
            base.OnValidate();
            SetType(ItemType.Throwable);
        }
    }
}

