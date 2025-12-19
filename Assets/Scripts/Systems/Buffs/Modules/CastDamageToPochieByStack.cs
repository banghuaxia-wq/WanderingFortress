using UnityEngine;

using WF.Gameplay.Systems.Pochie;
using WF.Gameplay.Systems.Player;
using WF.Gameplay.Core.Data;

namespace WF.Gameplay.Systems.Buffs
{
    [CreateAssetMenu(menuName="WF/Buffs/Modules/CastDamageToPochieByStack")]
    public class CastDamageToPochieByStack : BuffModule
    {
        public float damagePerStack = 1f;
        public override void Execute(BuffRunTimeInfo info, BuffManager manager)
        {
            float amount = damagePerStack * Mathf.Max(1, info.CurStack);
            if (info.Target == null) return;
            var pochie = info.Target.GetComponent<PochieCombatController>();
            if (pochie != null)
            {
                pochie.ReceiveBuffDamage(amount);
                return;
            }
            var stats = info.Target.GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.ApplyDamage(amount);
            }
        }
    }
}
