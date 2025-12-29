using UnityEngine;
using WF.Gameplay.Core.Data;

namespace WF.Gameplay.Systems.Inventory.Items
{
    [CreateAssetMenu(fileName = "NewArmorItem", menuName = "Items/Armor")]
    public class ArmorItem : EquipmentItem
    {
        protected override void OnValidate()
        {
            base.OnValidate();
            SetDefaultType(ItemType.Armor);
        }
    }
}
