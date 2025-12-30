using UnityEngine;
using WF.Gameplay.Core.Data;
using WF.Gameplay.Core.Interfaces;
using WF.Gameplay.Core.Utilities.Pooling;
using WF.Gameplay.Systems.Inventory;
using WF.Gameplay.Systems.Inventory.Items;
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
                
                TryTriggerAttackAnimation(owner, data.Type);
            }
        }

        private void SpawnProjectile(GameObject owner, AttackData data, Vector3 direction, Vector3 spawnPos)
        {
            GameObject projectilePrefab = data.ProjectilePrefab;
            if (projectilePrefab == null)
            {
                projectilePrefab = TryResolveProjectilePrefabFromAmmo(data);
            }

            if (projectilePrefab == null) return;
            
            GameObject bulletInstance = null;
            if (PoolManager.Instance != null)
            {
                bulletInstance = PoolManager.Instance.Get(projectilePrefab);
                if (bulletInstance != null)
                {
                    bulletInstance.transform.SetPositionAndRotation(spawnPos, Quaternion.LookRotation(direction));
                }
            }
            
            if (bulletInstance == null)
            {
                bulletInstance = Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(direction));
                var po = bulletInstance.GetComponent<PooledObject>();
                if (po == null) po = bulletInstance.AddComponent<PooledObject>();
                po.SourcePrefab = projectilePrefab;
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

        private static GameObject TryResolveProjectilePrefabFromAmmo(AttackData data)
        {
            if (data == null) return null;
            string ammoItemId = data.Cost.AmmoItemId;
            if (string.IsNullOrWhiteSpace(ammoItemId)) return null;

            var stack = ItemFactory.CreateItemStack(ammoItemId, 1);
            if (stack == null) return null;
            if (stack.Item is not AmmoItem ammo) return null;
            return ammo.ProjectilePrefab;
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
