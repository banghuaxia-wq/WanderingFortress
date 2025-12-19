using UnityEngine;
using WF.Gameplay.Core.Data;

namespace WF.Gameplay.Core.Interfaces
{
    public interface IAttackBehavior
    {
        bool CanExecute(GameObject owner, IWeaponItem weapon, AttackData data);
        void Execute(GameObject owner, IWeaponItem weapon, AttackData data, Vector3 aimDirection, Vector3? origin = null);
        AttackData GetAttackData(IWeaponItem weapon);
    }
}
