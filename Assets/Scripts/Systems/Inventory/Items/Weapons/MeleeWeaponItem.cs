using UnityEngine;
using WF.Gameplay.Core.Data;

namespace WF.Gameplay.Systems.Inventory.Items.Weapons
{
    [CreateAssetMenu(fileName = "NewMeleeWeapon", menuName = "Items/Weapons/Melee")]
    public class MeleeWeaponItem : WeaponItem
    {
        // Melee specific logic (durability, etc.)

        protected override void OnValidate()
        {
            base.OnValidate();
            SetCategory(WeaponCategory.Melee);
        }
    }
}
