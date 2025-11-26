using UnityEngine;

using WF.Gameplay.Systems.Enemy;
using WF.Gameplay.Systems.Player;
using WF.Gameplay.Core.Data;

namespace WF.Gameplay.Systems.Buffs
{
    [CreateAssetMenu(menuName="WF/Buffs/Modules/CastDamageToEnemyByStack")]
    public class CastDamageToEnemyByStack : BuffModule
    {
        public float damagePerStack = 1f;
        public override void Execute(BuffRunTimeInfo info, BuffManager manager)
        {
            float amount = damagePerStack * Mathf.Max(1, info.CurStack);
            if (info.Target == null) return;
            var enemy = info.Target.GetComponent<EnemyCombatController>();
            if (enemy != null)
            {
                enemy.ReceiveBuffDamage(amount);
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
