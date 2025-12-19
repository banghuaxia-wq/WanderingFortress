using UnityEngine;
using WF.Gameplay.Core.Data;
using WF.Gameplay.Core.Interfaces;

namespace WF.Gameplay.Systems.Combat.AttackBehaviors
{
    [CreateAssetMenu(fileName = "NewMeleeAttack", menuName = "Combat/Attack Behaviors/Melee")]
    public class MeleeAttackBehavior : AttackBehaviorBase
    {
        public override void Execute(GameObject owner, IWeaponItem weapon, AttackData data, Vector3 aimDirection, Vector3? origin = null)
        {
            // Trigger Animation
            var animator = owner.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }

            // Melee attack logic (overlap sphere/box cast)
            // Implementation placeholder
        }
    }
}
