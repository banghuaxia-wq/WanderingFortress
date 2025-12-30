using UnityEngine;
using WF.Gameplay.Core.Data;
using WF.Gameplay.Core.Interfaces;
using WF.Gameplay.Systems.Inventory.Items;

namespace WF.Gameplay.Systems.Weapons.Projectile
{
    /// <summary>
    /// 控制子弹的飞行、碰撞和销毁逻辑。
    /// </summary>
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class Projectile : MonoBehaviour
    {
        // 移动方向向量的最小平方长度，用于避免零向量问题
        private const float MinDirectionSqrMagnitude = 0.0001f;

        [Tooltip("可以被子弹击中的层")]
        [SerializeField] private LayerMask hitMask;
        [SerializeField] private bool autoConfigurePhysics = true;
        [Tooltip("子弹击中环境后是否销毁")]
        [SerializeField] private bool destroyOnEnvironmentHit = true;
        [SerializeField] private float armDelaySeconds = 0.02f;
        [SerializeField] private bool useContinuousCast = true;

        // 子弹的飞行速度
        private float _speed;
        // 子弹的生命周期（秒）
        private float _lifetime;
        // 子弹的伤害值
        private DamageInfo _payload;
        // 子弹的飞行方向
        private Vector3 _direction;
        // 子弹的生命周期计时器
        private float _lifeTimer;
        // 子弹的刚体组件
        private Rigidbody _rigidbody;
        // 子弹的碰撞体组件
        private Collider _collider;
        private float _castRadius;
        public enum ProjectileBehaviorType { Damage, Capture, Recall }
        [SerializeField] private ProjectileBehaviorType behavior = ProjectileBehaviorType.Damage;
        [SerializeField] private float captureLevel = 1f;

        /// <summary>
        /// 初始化组件引用并根据设置配置物理属性。
        /// </summary>
        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
            _castRadius = ResolveCastRadius(_collider);

            if (autoConfigurePhysics)
            {
                if (_rigidbody != null)
                {
                    _rigidbody.useGravity = false;
                    _rigidbody.isKinematic = true;
                }

                if (_collider != null)
                {
                    _collider.isTrigger = true;
                }
            }
        }

        /// <summary>
        /// 初始化子弹的飞行参数。
        /// </summary>
        /// <param name="speed">飞行速度</param>
        /// <param name="lifetime">生命周期</param>
        /// <param name="direction">飞行方向</param>
        /// <param name="payload">伤害信息</param>
        public void Initialize(float speed, float lifetime, Vector3 direction, DamageInfo payload)
        {
            _speed = speed;
            _lifetime = lifetime;
            _payload = payload;
            _direction = direction.normalized;
            _lifeTimer = 0f;
            transform.forward = _direction;
        }

        /// <summary>
        /// 每帧更新子弹的位置和生命周期。
        /// </summary>
        private void Update()
        {
            if (_direction.sqrMagnitude < MinDirectionSqrMagnitude)
            {
                ReturnToPool();
                return;
            }

            float frameDistance = _speed * Time.deltaTime;
            if (useContinuousCast && _lifeTimer >= Mathf.Max(0f, armDelaySeconds))
            {
                if (TryCastHit(frameDistance, out var hit))
                {
                    HandleHit(hit.collider);
                    return;
                }
            }

            transform.position += _direction * frameDistance;
            _lifeTimer += Time.deltaTime;

            if (_lifeTimer >= _lifetime)
            {
                ReturnToPool();
            }
        }

        /// <summary>
        /// 处理触发器碰撞事件。
        /// </summary>
        /// <param name="other">与之碰撞的另一个碰撞体</param>
        private void OnTriggerEnter(Collider other)
        {
            if (_lifeTimer < Mathf.Max(0f, armDelaySeconds)) return;
            HandleHit(other);
        }

        private void HandleHit(Collider other)
        {
            if (other == null) return;

            if (!IsLayerHittable(other.gameObject.layer))
            {
                return;
            }

            if (_payload != null && _payload.Source != null)
            {
                var sourceTransform = _payload.Source.transform;
                if (other.transform == sourceTransform || other.transform.IsChildOf(sourceTransform)) return;
            }

            switch (behavior)
            {
                case ProjectileBehaviorType.Damage:
                    var dmg = other.GetComponent<IDamageable>() ?? other.GetComponentInParent<IDamageable>();
                    if (dmg != null)
                    {
                        if (_payload != null && _payload.Source != null)
                        {
                            var sourceTransform = _payload.Source.transform;
                            var sourceRoot = sourceTransform.root != null ? sourceTransform.root.gameObject : _payload.Source;
                            var otherRoot = other.transform.root != null ? other.transform.root.gameObject : other.gameObject;
                            if (sourceRoot != null && otherRoot != null && sourceRoot == otherRoot) return;
                        }

                        dmg.TakeDamage(_payload);
                        ReturnToPool();
                        return;
                    }
                    break;
                case ProjectileBehaviorType.Capture:
                    var cap = other.GetComponent<ICapturable>() ?? other.GetComponentInParent<ICapturable>();
                    if (cap != null)
                    {
                        cap.TryCapture(captureLevel);
                        ReturnToPool();
                        return;
                    }
                    break;
                case ProjectileBehaviorType.Recall:
                    var rec = other.GetComponent<IRecallable>() ?? other.GetComponentInParent<IRecallable>();
                    if (rec != null)
                    {
                        GameObject caller = _payload != null ? _payload.Source : gameObject;
                        rec.Recall(caller);
                        ReturnToPool();
                        return;
                    }
                    break;
            }

            if (destroyOnEnvironmentHit)
            {
                ReturnToPool();
            }
        }

        private bool TryCastHit(float frameDistance, out RaycastHit hit)
        {
            hit = default;
            if (_castRadius <= 0f) return false;

            Vector3 origin = transform.position;
            if (frameDistance <= 0f) return false;

            int mask = hitMask == 0 ? Physics.DefaultRaycastLayers : hitMask.value;
            return Physics.SphereCast(origin, _castRadius, _direction, out hit, frameDistance, mask, QueryTriggerInteraction.Collide);
        }

        private static float ResolveCastRadius(Collider col)
        {
            if (col == null) return 0.05f;
            if (col is SphereCollider sphere)
            {
                float maxScale = Mathf.Max(col.transform.lossyScale.x, col.transform.lossyScale.y, col.transform.lossyScale.z);
                return Mathf.Max(0.001f, sphere.radius * maxScale);
            }

            var bounds = col.bounds;
            float r = Mathf.Min(bounds.extents.x, bounds.extents.y, bounds.extents.z);
            return Mathf.Max(0.001f, r);
        }

        /// <summary>
        /// 检查给定的层是否在可命中层遮罩内。
        /// </summary>
        /// <param name="objectLayer">要检查的对象的层</param>
        /// <returns>如果层是可命中的，则为true；否则为false。</returns>
        private bool IsLayerHittable(int objectLayer)
        {
            if (hitMask == 0)
            {
                return true;
            }

            return (hitMask & (1 << objectLayer)) != 0;
        }

        private void ReturnToPool()
        {
            var po = GetComponent<WF.Gameplay.Core.Utilities.Pooling.PooledObject>();
            if (po != null)
            {
                po.ReturnToPool();
            }
            else
            {
                gameObject.SetActive(false);
                Destroy(gameObject);
            }
        }
    }
}
