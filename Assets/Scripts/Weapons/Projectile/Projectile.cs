using UnityEngine;
using WF.Gameplay;

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
    /// <param name="damage">伤害值</param>
    /// <param name="stun">眩晕值</param>
    /// <param name="direction">飞行方向</param>
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
        if (!IsLayerHittable(other.gameObject.layer))
        {
            return;
        }

        switch (behavior)
        {
            case ProjectileBehaviorType.Damage:
                var dmg = other.GetComponent<IDamageable>() ?? other.GetComponentInParent<IDamageable>();
                if (dmg != null)
                {
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
        var po = GetComponent<WF.Gameplay.PooledObject>();
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

