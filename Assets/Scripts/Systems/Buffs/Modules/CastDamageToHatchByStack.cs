using UnityEngine;

using WF.Gameplay.Systems.Hatch;
using WF.Gameplay.Systems.Player;
using WF.Gameplay.Core.Data;

namespace WF.Gameplay.Systems.Buffs
{
    [CreateAssetMenu(menuName="WF/Buffs/Modules/CastDamageToHatchByStack")]
    public class CastDamageToHatchByStack : BuffModule
    {
        public float damagePerStack = 1f;
        public override void Execute(BuffRunTimeInfo info, BuffManager manager)
        {
            float amount = damagePerStack * Mathf.Max(1, info.CurStack);
            if (info.Target == null) return;
            var hatch = info.Target.GetComponent<HatchCombatController>();
            if (hatch != null)
            {
                hatch.ReceiveBuffDamage(amount);
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

