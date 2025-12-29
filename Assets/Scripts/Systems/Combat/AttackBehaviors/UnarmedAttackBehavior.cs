using UnityEngine;
using WF.Gameplay.Core.Data;
using WF.Gameplay.Core.Interfaces;

namespace WF.Gameplay.Systems.Combat.AttackBehaviors
{
    [CreateAssetMenu(fileName = "NewUnarmedAttack", menuName = "Combat/Attack Behaviors/Unarmed")]
    public class UnarmedAttackBehavior : AttackBehaviorBase
    {
        public override void Execute(GameObject owner, IWeaponItem weapon, AttackData data, Vector3 aimDirection, Vector3? origin = null)
        {
            TryTriggerAttackAnimation(owner, data.Type);
            
            // Unarmed punch logic
        }
    }
}
