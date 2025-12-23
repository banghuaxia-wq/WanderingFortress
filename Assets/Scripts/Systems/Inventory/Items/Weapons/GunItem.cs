using UnityEngine;
using WF.Gameplay.Core.Data;

namespace WF.Gameplay.Systems.Inventory.Items.Weapons
{
    [CreateAssetMenu(fileName = "NewGunItem", menuName = "Items/Gun")]
    public class GunItem : RangedWeaponItem
    {
        protected override void OnValidate()
        {
            base.OnValidate();
            SetType(ItemType.Gun);
        }
    }
}

