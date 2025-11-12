using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class BulletProjectile : MonoBehaviour
{
    private const float MinDirectionSqrMagnitude = 0.0001f;

    [SerializeField] private LayerMask hitMask;
    [SerializeField] private bool autoConfigurePhysics = true;
    [SerializeField] private bool destroyOnEnvironmentHit = true;

    private float _speed;
    private float _lifetime;
    private float _damage;
    private float _stun;
    private Vector3 _direction;
    private float _lifeTimer;
    private Rigidbody _rigidbody;
    private Collider _collider;

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
    public void Initialize(float speed, float lifetime, float damage, float stun, Vector3 direction)
    {
        _speed = speed;
        _lifetime = lifetime;
        _damage = damage;
        _stun = stun;
        _direction = direction.normalized;
        _lifeTimer = 0f;
        transform.forward = _direction;
    }

    private void Update()
    {
        if (_direction.sqrMagnitude < MinDirectionSqrMagnitude)
        {
            Destroy(gameObject);
            return;
        }

        float frameDistance = _speed * Time.deltaTime;
        transform.position += _direction * frameDistance;
        _lifeTimer += Time.deltaTime;

        if (_lifeTimer >= _lifetime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsLayerHittable(other.gameObject.layer))
        {
            return;
        }

        if (other.TryGetComponent(out EnemyCombatController enemyCombat))
        {
            enemyCombat.ReceiveProjectileHit(_damage, _stun);
            Destroy(gameObject);
            return;
        }

        EnemyCombatController enemyFromParent = other.GetComponentInParent<EnemyCombatController>();
        if (enemyFromParent != null)
        {
            enemyFromParent.ReceiveProjectileHit(_damage, _stun);
            Destroy(gameObject);
            return;
        }

        if (destroyOnEnvironmentHit)
        {
            Destroy(gameObject);
        }
    }

    private bool IsLayerHittable(int objectLayer)
    {
        if (hitMask == 0)
        {
            return true;
        }

        return (hitMask & (1 << objectLayer)) != 0;
    }
}

