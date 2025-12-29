using UnityEngine;
using WF.Gameplay.Core.Data;

namespace WF.Gameplay.Systems.Inventory.Items.Weapons
{
    public class MeleeItem : MeleeWeaponItem
    {
        protected override void OnValidate()
        {
            base.OnValidate();
            SetDefaultType(ItemType.Weapon);
        }
    }
}
