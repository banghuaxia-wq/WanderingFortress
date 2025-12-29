using UnityEngine;

namespace WF.Gameplay.Core.Data
{
    [System.Serializable]
    public class AttackData
    {
        public AttackType Type;
        public float Damage;
        public float AttackSpeed; // Seconds per attack
        public float Range;
        public float Cooldown;
        public ResourceCost Cost;
        public float StunValue;
        
        // Ranged specific
        public GameObject ProjectilePrefab;
        public int ProjectileCount = 1;
        public float SpreadAngle = 0f;
        public float ProjectileSpeed = 50f;
        public float ProjectileLifetime = 5f;

        // Sound
        public float NoiseRadius = 0f; // 攻击噪声半径（用于吸引敌人）（中文注释）
        public SoundType NoiseType = SoundType.Combat; // 噪声类型（中文注释）

        // Recoil
        public float VerticalRecoil;
        public float HorizontalRecoil;
        public float RecoilDecay;
    }
}
