using UnityEngine;
using WF.Gameplay.Core.Interfaces;

namespace WF.Gameplay.Systems.Inventory.Items.Weapons
{
    [CreateAssetMenu(fileName = "NewRangedWeapon", menuName = "Items/Weapons/Ranged")]
    public class RangedWeaponItem : WeaponItem
    {
        public override void TryReload(IPlayerInventory inventory)
        {
            if (inventory == null || AttackData == null || string.IsNullOrEmpty(AttackData.Cost.AmmoItemId)) return;
            
            int needed = MaxAmmo - CurrentAmmo;
            if (needed <= 0) return;
            
            // Check inventory for ammo
            int available = inventory.GetItemCount(AttackData.Cost.AmmoItemId);
            if (available > 0)
            {
                int toReload = Mathf.Min(needed, available);
                inventory.RemoveItem(AttackData.Cost.AmmoItemId, toReload);
                CurrentAmmo += toReload;
            }
        }
    }
}
