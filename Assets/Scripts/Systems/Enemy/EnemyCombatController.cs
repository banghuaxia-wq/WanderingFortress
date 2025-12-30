using UnityEngine;
using WF.Gameplay.Core.Data;
using WF.Gameplay.Core.Events;
using WF.Gameplay.Core.Utilities.Pooling;
using WF.Gameplay.Systems.Weapons.Projectile;

namespace WF.Gameplay.Systems.Enemy
{
    // 敌人战斗控制器：近战扇形检测与远程发射子弹（中文注释）
    public class EnemyCombatController : MonoBehaviour
    {
        [Header("Mode")]
        [SerializeField] private EnemyAttackMode attackMode = EnemyAttackMode.Melee; // 攻击模式（中文注释）

        [Header("Common")]
        [SerializeField] private float cooldownSeconds = 1.0f; // 攻击冷却（中文注释）
        [SerializeField] private Animator animator; // Animator引用（中文注释）
        [SerializeField] private string attackTrigger = "Attack"; // 攻击动画Trigger名（中文注释）

        [Header("Melee")]
        [SerializeField] private float meleeRange = 1.8f; // 近战攻击半径（中文注释）
        [SerializeField] private float meleeAngle = 90f; // 近战扇形角度（度）（中文注释）
        [SerializeField] private LayerMask meleeHitMask = ~0; // 近战命中层（中文注释）
        [SerializeField] private bool meleeCanBeBlockedByObstacle = false; // 近战是否会被墙挡住（中文注释）
        [SerializeField] private LayerMask meleeObstacleMask = ~0; // 近战遮挡层（中文注释）

        [Header("Ranged")]
        [SerializeField] private Transform rangedMuzzle; // 远程发射点（中文注释）
        [SerializeField] private GameObject projectilePrefab; // 子弹预制体（中文注释）
        [SerializeField] private int projectileCount = 1; // 子弹数量（中文注释）
        [SerializeField] private float projectileSpeed = 30f; // 子弹速度（中文注释）
        [SerializeField] private float projectileLifetime = 4f; // 子弹生命周期（中文注释）
        [SerializeField] private float spreadAngleWhenVisible = 1.5f; // 可见时散布角（中文注释）
        [SerializeField] private float spreadAngleWhenBlind = 6f; // 不可见时散布角（中文注释）
        [SerializeField] private float rangedAttackRange = 8f; // 远程攻击范围（中文注释）

        [Header("Damage")]
        [SerializeField] private float damage = 10f; // 伤害值（中文注释）
        [SerializeField] private float stunValue = 0f; // 硬直/眩晕值（中文注释）
        [SerializeField] private DamageType damageType = DamageType.Physical; // 伤害类型（中文注释）

        [Header("Sound")]
        [SerializeField] private bool emitAttackSound = true; // 是否发布攻击声音事件（中文注释）
        [SerializeField] private float attackNoiseRadius = 6f; // 攻击噪声半径（中文注释）
        [SerializeField] private SoundType attackNoiseType = SoundType.Combat; // 攻击噪声类型（中文注释）

        private float _nextAttackTime; // 下次允许攻击时间（中文注释）
        private EnemyActor _ownerActor;

        private void Awake()
        {
            if (animator == null) animator = GetComponent<Animator>();
            _ownerActor = GetComponentInParent<EnemyActor>();
        }

        // 由行为中心设置攻击模式（中文注释）
        public void SetAttackMode(EnemyAttackMode mode)
        {
            attackMode = mode;
        }

        // 行为树：尝试攻击（返回是否成功触发）（中文注释）
        public bool TryAttack(Vector3 aimPosition, Transform target, bool hasVision)
        {
            if (Time.time < _nextAttackTime) return false;

            bool ok = attackMode == EnemyAttackMode.Ranged
                ? TryRangedAttack(aimPosition, target, hasVision)
                : TryMeleeAttack(target);

            if (!ok) return false;

            _nextAttackTime = Time.time + Mathf.Max(0f, cooldownSeconds);

            if (animator != null && !string.IsNullOrEmpty(attackTrigger))
            {
                animator.SetTrigger(attackTrigger);
            }

            if (emitAttackSound && attackNoiseRadius > 0.001f)
            {
                EventBus.Publish(new SoundEmittedEvent(transform.position, attackNoiseRadius, attackNoiseType, gameObject));
            }

            return true;
        }

        // 行为树：近战是否在范围内（中文注释）
        public bool IsInMeleeRange(Transform target)
        {
            if (target == null) return false;
            Vector3 delta = target.position - transform.position;
            delta.y = 0f;
            return delta.sqrMagnitude <= meleeRange * meleeRange;
        }

        // 行为树：远程是否在范围内（以目标点距离判断）（中文注释）
        public bool IsInRangedRange(Vector3 aimPosition)
        {
            Vector3 delta = aimPosition - transform.position;
            delta.y = 0f;
            float r = Mathf.Max(0f, rangedAttackRange);
            return delta.sqrMagnitude <= r * r;
        }

        private bool TryMeleeAttack(Transform explicitTarget)
        {
            Vector3 origin = transform.position;
            Vector3 forward = transform.forward;
            forward.y = 0f;
            forward.Normalize();

            float radius = Mathf.Max(0f, meleeRange);
            if (radius <= 0.001f) return false;

            bool canSwingAtTarget = false;
            if (explicitTarget != null)
            {
                Vector3 toTarget = explicitTarget.position - origin;
                toTarget.y = 0f;

                if (toTarget.sqrMagnitude > 0.0001f && toTarget.sqrMagnitude <= radius * radius)
                {
                    float halfAngle = meleeAngle * 0.5f;
                    float ang = Vector3.Angle(forward, toTarget.normalized);
                    if (ang <= halfAngle)
                    {
                        if (meleeCanBeBlockedByObstacle)
                        {
                            float dist = Mathf.Sqrt(toTarget.sqrMagnitude);
                            Vector3 rayDir = (explicitTarget.position - origin).normalized;
                            if (Physics.Raycast(origin, rayDir, out var blockHit, dist, meleeObstacleMask, QueryTriggerInteraction.Ignore))
                            {
                                if (blockHit.transform == explicitTarget || blockHit.transform.IsChildOf(explicitTarget))
                                {
                                    canSwingAtTarget = true;
                                }
                            }
                            else
                            {
                                canSwingAtTarget = true;
                            }
                        }
                        else
                        {
                            canSwingAtTarget = true;
                        }
                    }
                }
            }

            var hits = Physics.OverlapSphere(origin, radius, meleeHitMask, QueryTriggerInteraction.Ignore);
            if (hits == null || hits.Length == 0) return canSwingAtTarget;

            bool hitAny = false;
            float half = meleeAngle * 0.5f;

            for (int i = 0; i < hits.Length; i++)
            {
                var col = hits[i];
                if (col == null) continue;
                if (col.attachedRigidbody != null && col.attachedRigidbody.gameObject == gameObject) continue;
                if (col.gameObject == gameObject) continue;
                if (_ownerActor != null && col.GetComponentInParent<EnemyActor>() != null) continue;

                var dmg = col.GetComponent<WF.Gameplay.Core.Interfaces.IDamageable>() ?? col.GetComponentInParent<WF.Gameplay.Core.Interfaces.IDamageable>();
                if (dmg == null) continue;

                if (explicitTarget != null)
                {
                    if (col.transform != explicitTarget && !col.transform.IsChildOf(explicitTarget)) continue;
                }

                Vector3 to = col.transform.position - origin;
                to.y = 0f;
                if (to.sqrMagnitude <= 0.0001f) continue;

                float ang = Vector3.Angle(forward, to.normalized);
                if (ang > half) continue;

                if (meleeCanBeBlockedByObstacle)
                {
                    float dist = Mathf.Sqrt(to.sqrMagnitude);
                    Vector3 rayDir = (col.transform.position - origin).normalized;
                    if (Physics.Raycast(origin, rayDir, out var blockHit, dist, meleeObstacleMask, QueryTriggerInteraction.Ignore))
                    {
                        if (blockHit.transform != col.transform && !blockHit.transform.IsChildOf(col.transform))
                        {
                            continue;
                        }
                    }
                }

                var payload = new DamageInfo
                {
                    Source = gameObject,
                    Damage = damage,
                    InstantStun = stunValue,
                    Type = damageType,
                    AppliedBuffs = null
                };
                dmg.TakeDamage(payload);
                hitAny = true;
            }

            return hitAny || canSwingAtTarget;
        }

        private bool TryRangedAttack(Vector3 aimPosition, Transform target, bool hasVision)
        {
            if (projectilePrefab == null) return false;

            Vector3 spawnPos = rangedMuzzle != null ? rangedMuzzle.position : transform.position;
            Vector3 aim = target != null && hasVision ? target.position : aimPosition;
            Vector3 baseDir = aim - spawnPos;
            baseDir.y = 0f;
            if (baseDir.sqrMagnitude < 0.0001f) baseDir = transform.forward;
            baseDir.Normalize();

            float spread = hasVision ? spreadAngleWhenVisible : spreadAngleWhenBlind;
            int count = Mathf.Max(1, projectileCount);

            for (int i = 0; i < count; i++)
            {
                Vector3 shotDir = ApplySpread(baseDir, spread);
                SpawnProjectile(spawnPos, shotDir);
            }

            return true;
        }

        private void SpawnProjectile(Vector3 spawnPos, Vector3 direction)
        {
            GameObject inst = null;
            Quaternion rot = Quaternion.LookRotation(direction);

            if (PoolManager.Instance != null)
            {
                inst = PoolManager.Instance.Get(projectilePrefab);
                if (inst != null)
                {
                    inst.transform.SetPositionAndRotation(spawnPos, rot);
                }
            }

            if (inst == null)
            {
                inst = Instantiate(projectilePrefab, spawnPos, rot);
                var po = inst.GetComponent<PooledObject>();
                if (po == null) po = inst.AddComponent<PooledObject>();
                po.SourcePrefab = projectilePrefab;
            }

            if (!inst.TryGetComponent(out Projectile proj))
            {
                proj = inst.AddComponent<Projectile>();
            }

            var payload = new DamageInfo
            {
                Source = gameObject,
                Damage = damage,
                InstantStun = stunValue,
                Type = damageType,
                AppliedBuffs = null
            };

            proj.Initialize(projectileSpeed, projectileLifetime, direction, payload);
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
