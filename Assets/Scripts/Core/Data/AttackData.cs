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

        // Recoil
        public float VerticalRecoil;
        public float HorizontalRecoil;
        public float RecoilDecay;
    }
}
