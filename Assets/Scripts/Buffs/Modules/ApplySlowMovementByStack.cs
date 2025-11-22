using UnityEngine;

namespace WF.Gameplay
{
    [CreateAssetMenu(menuName="WF/Buffs/Modules/ApplySlowMovementByStack")]
    public class ApplySlowMovementByStack : BuffModule
    {
        public float slowPerStack = 0.2f;
        public float minMultiplier = 0.2f;
        public override void Execute(BuffRunTimeInfo info, BuffManager manager)
        {
            var sm = PlayerStateManager.Instance;
            if (sm == null) return;
            string key = (info.BuffData != null && !string.IsNullOrEmpty(info.BuffData.Id))
                ? info.BuffData.Id
                : (info.BuffData != null ? info.BuffData.name : name);
            if (callback == BuffCallback.OnRemove)
            {
                sm.RemoveMovementSpeedMultiplier(key);
                return;
            }
            int stack = Mathf.Max(1, info.CurStack);
            float mul = Mathf.Clamp(1f - slowPerStack * stack, minMultiplier, 1f);
            sm.SetMovementSpeedMultiplier(key, mul);
        }
    }
}
