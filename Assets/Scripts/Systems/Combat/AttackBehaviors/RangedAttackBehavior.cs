using UnityEngine;
using WF.Gameplay.Core.Data;
using WF.Gameplay.Core.Interfaces;
using WF.Gameplay.Core.Utilities.Pooling;
using WF.Gameplay.Systems.Inventory.Items.Weapons;
using WF.Gameplay.Systems.Weapons.Projectile;

namespace WF.Gameplay.Systems.Combat.AttackBehaviors
{
    [CreateAssetMenu(fileName = "NewRangedAttack", menuName = "Combat/Attack Behaviors/Ranged")]
    public class RangedAttackBehavior : AttackBehaviorBase
    {
        public override void Execute(GameObject owner, IWeaponItem weapon, AttackData data, Vector3 aimDirection, Vector3? origin = null)
        {
            if (weapon is RangedWeaponItem rangedWeapon)
            {
                // Consume Ammo
                rangedWeapon.ConsumeAmmo(data.Cost.AmmoCount);

                Vector3 spawnPos = origin.HasValue ? origin.Value : owner.transform.position;

                // Spawn Projectiles
                for (int i = 0; i < data.ProjectileCount; i++)
                {
                    Vector3 shotDirection = ApplySpread(aimDirection, data.SpreadAngle);
                    SpawnProjectile(owner, data, shotDirection, spawnPos);
                }
                
                // Trigger Animation
                var animator = owner.GetComponent<Animator>();
                if (animator != null)
                {
                    animator.SetTrigger("Attack");
                }
            }
        }

        private void SpawnProjectile(GameObject owner, AttackData data, Vector3 direction, Vector3 spawnPos)
        {
            if (data.ProjectilePrefab == null) return;
            
            GameObject bulletInstance = null;
            if (PoolManager.Instance != null)
            {
                bulletInstance = PoolManager.Instance.Get(data.ProjectilePrefab);
                if (bulletInstance != null)
                {
                    bulletInstance.transform.SetPositionAndRotation(spawnPos, Quaternion.LookRotation(direction));
                }
            }
            
            if (bulletInstance == null)
            {
                bulletInstance = Instantiate(data.ProjectilePrefab, spawnPos, Quaternion.LookRotation(direction));
                var po = bulletInstance.GetComponent<PooledObject>();
                if (po == null) po = bulletInstance.AddComponent<PooledObject>();
                po.SourcePrefab = data.ProjectilePrefab;
            }

            if (!bulletInstance.TryGetComponent(out Projectile projectile))
            {
                projectile = bulletInstance.AddComponent<Projectile>();
            }

            var payload = new DamageInfo
            {
                Source = owner,
                Damage = data.Damage,
                InstantStun = data.StunValue,
                Type = DamageType.Physical,
                AppliedBuffs = null 
            };

            projectile.Initialize(data.ProjectileSpeed, data.ProjectileLifetime, direction, payload);
        }

        private Vector3 ApplySpread(Vector3 baseDirection, float spreadAngle)
        {
            if (spreadAngle <= 0f) return baseDirection.normalized;

            baseDirection.Normalize();
            float angleRad = spreadAngle * Mathf.Deg2Rad;
            float u = Random.value;
            float cosTheta = Mathf.Lerp(Mathf.Cos(angleRad), 1f, u);
            float sinTheta = Mathf.Sqrt(1f - cosTheta * cosTheta);
            float phi = Random.value * Mathf.PI * 2f;
            Vector3 n = baseDirection;
            Vector3 arbitrary = Mathf.Abs(n.y) < 0.99f ? Vector3.up : Vector3.right;
            Vector3 t = Vector3.Normalize(Vector3.Cross(arbitrary, n));
            Vector3 b = Vector3.Cross(n, t);
            Vector3 dir = t * (sinTheta * Mathf.Cos(phi)) + b * (sinTheta * Mathf.Sin(phi)) + n * cosTheta;
            return dir.normalized;
        }
    }
}
