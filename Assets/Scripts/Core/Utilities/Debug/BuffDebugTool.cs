using UnityEngine;

using WF.Gameplay.Core.Data;
using WF.Gameplay.Systems.Buffs;

namespace WF.Gameplay.Core.Utilities.Debug
{
    public class BuffDebugTool : MonoBehaviour
    {
        [SerializeField] private BuffData buffToApply;
        [SerializeField] private int applyTimes = 1;
        [SerializeField] private GameObject explicitTarget;
        [SerializeField] private bool autoAddManager = true;

        [ContextMenu("Apply To Explicit Target")]
        public void ApplyBuffToExplicit()
        {
            if (explicitTarget == null || buffToApply == null) return;
            ApplyBuffTo(explicitTarget);
        }

        [ContextMenu("Remove Buff From Explicit Target")]
        public void RemoveBuffFromExplicit()
        {
            if (explicitTarget == null || buffToApply == null) return;
            RemoveBuffFrom(explicitTarget);
        }

        private void ApplyBuffTo(GameObject target)
        {
            BuffManager mgr = target.GetComponent<BuffManager>();
            if (mgr == null && autoAddManager) mgr = target.AddComponent<BuffManager>();
            if (mgr == null) return;
            int times = Mathf.Max(1, applyTimes);
            for (int i = 0; i < times; i++) mgr.AddBuff(buffToApply, gameObject);
        }

        private void RemoveBuffFrom(GameObject target)
        {
            BuffManager mgr = target.GetComponent<BuffManager>();
            if (mgr == null) return;
            mgr.RemoveBuff(buffToApply.Id);
        }
    }
}
