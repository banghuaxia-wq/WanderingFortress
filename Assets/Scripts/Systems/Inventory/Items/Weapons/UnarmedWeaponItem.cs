using UnityEngine;
using WF.Gameplay.Core.Data;

namespace WF.Gameplay.Systems.Inventory.Items.Weapons
{
    [CreateAssetMenu(fileName = "NewUnarmedWeapon", menuName = "Items/Weapons/Unarmed")]
    public class UnarmedWeaponItem : WeaponItem
    {
        // Unarmed specific logic

        protected override void OnValidate()
        {
            base.OnValidate();
            SetCategory(WeaponCategory.Unarmed);
        }
    }
}
