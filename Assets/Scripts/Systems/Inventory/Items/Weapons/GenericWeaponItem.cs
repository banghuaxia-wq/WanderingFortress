using UnityEngine;
using WF.Gameplay.Core.Data;

namespace WF.Gameplay.Systems.Inventory.Items.Weapons
{
    public class GenericWeaponItem : WeaponItem
    {
        protected override void OnValidate()
        {
            base.OnValidate();
            SetType(ItemType.Weapon);
        }
    }
}
