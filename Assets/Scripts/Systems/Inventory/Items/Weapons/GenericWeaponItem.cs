using UnityEngine;
using WF.Gameplay.Core.Data;

namespace WF.Gameplay.Systems.Inventory.Items.Weapons
{
    [CreateAssetMenu(fileName = "NewWeaponItem", menuName = "Items/Weapon")]
    public class GenericWeaponItem : WeaponItem
    {
        protected override void OnValidate()
        {
            base.OnValidate();
            SetType(ItemType.Weapon);
        }
    }
}

