using UnityEngine;

namespace WF.Gameplay
{
    public class BuffRunTimeInfo
    {
        public BuffData BuffData;
        public GameObject Creator;
        public GameObject Target;
        public float DurationTimer;
        public int CurStack;
        public float ExtraValue;
        public int ElapsedSeconds;

        public BuffRunTimeInfo(BuffData data, GameObject creator, GameObject target)
        {
            BuffData = data;
            Creator = creator;
            Target = target;
            CurStack = 1;
            DurationTimer = data.IsForever ? float.PositiveInfinity : data.Duration;
            ExtraValue = 0f;
            ElapsedSeconds = 0;
        }
    }
}
