using UnityEngine;

using WF.Gameplay.Core.Data;
using WF.Gameplay.Systems.Pochie;

namespace WF.Gameplay.Systems.Buffs
{
    [CreateAssetMenu(menuName="WF/Buffs/Modules/SedativeStun")]
    public class SedativeStunModule : BuffModule
    {
        public override void Execute(BuffRunTimeInfo info, BuffManager manager)
        {
            if (info == null || info.Target == null) return;
            var pochie = info.Target.GetComponent<PochieCombatController>();
            if (pochie == null) return;

            if (callback == BuffCallback.OnCreate || callback == BuffCallback.OnAddStack)
            {
                pochie.SetStunDecayBlocked(true);
                return;
            }
            if (callback == BuffCallback.OnReduceStack)
            {
                int stack = Mathf.Max(0, info.CurStack);
                if (stack <= 0)
                {
                    pochie.SetStunDecayBlocked(false);
                }
                return;
            }
            if (callback == BuffCallback.OnRemove)
            {
                pochie.SetStunDecayBlocked(false);
                return;
            }
            if (callback == BuffCallback.OnTick)
            {
                float total = Mathf.Max(0f, info.ExtraValue);
                float duration = Mathf.Max(1f, info.BuffData.Duration);
                float perSecondBase = total / duration; // 每秒固定值，独立于帧率
                int stack = Mathf.Max(1, info.CurStack);
                float amount = perSecondBase * stack;
                var di = new DamageInfo { Source = manager.gameObject, Damage = 0f, InstantStun = amount, Type = DamageType.Physical };
                pochie.TakeDamage(di);
                if (manager != null && manager.IsLogEnabled)
                {
                    Debug.Log($"[BuffTick] {info.BuffData?.BuffName ?? info.BuffData?.Id} sec={info.ElapsedSeconds} apply={amount:0.##} stack={stack}", manager);
                }
            }
        }
    }
}
