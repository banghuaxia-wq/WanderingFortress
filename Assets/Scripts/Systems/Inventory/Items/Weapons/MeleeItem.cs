using UnityEngine;
using WF.Gameplay.Core.Data;

namespace WF.Gameplay.Systems.Inventory.Items.Weapons
{
    [CreateAssetMenu(fileName = "NewMeleeItem", menuName = "Items/Melee")]
    public class MeleeItem : MeleeWeaponItem
    {
        protected override void OnValidate()
        {
            base.OnValidate();
            SetType(ItemType.Melee);
        }
    }
}

