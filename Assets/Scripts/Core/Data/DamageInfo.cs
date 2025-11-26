using System.Collections.Generic;
using UnityEngine;

namespace WF.Gameplay.Core.Data
{
    public class DamageInfo
    {
        public GameObject Source;
        public float Damage;
        public float InstantStun;
        public DamageType Type;
        public List<AppliedBuff> AppliedBuffs;
    }

    public struct AppliedBuff
    {
        public BuffData Buff;
        public float ExtraValue;
    }
}
