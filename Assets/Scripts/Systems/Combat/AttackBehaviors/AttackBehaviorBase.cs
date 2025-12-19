using UnityEngine;
using WF.Gameplay.Core.Data;
using WF.Gameplay.Core.Interfaces;

namespace WF.Gameplay.Systems.Combat.AttackBehaviors
{
    public abstract class AttackBehaviorBase : ScriptableObject, IAttackBehavior
    {
        public virtual bool CanExecute(GameObject owner, IWeaponItem weapon, AttackData data)
        {
            if (weapon == null || data == null) return false;
            
            // Check ammo
            if (data.Cost.AmmoCount > 0 && weapon.CurrentAmmo < data.Cost.AmmoCount)
            {
                return false;
            }
            
            // Check stamina (if owner has stamina system)
            // ...
            
            return true;
        }

        public abstract void Execute(GameObject owner, IWeaponItem weapon, AttackData data, Vector3 aimDirection, Vector3? origin = null);

        public virtual AttackData GetAttackData(IWeaponItem weapon)
        {
            return weapon?.AttackData;
        }
    }
}
