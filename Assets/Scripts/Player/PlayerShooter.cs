using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    private const string FireInputName = "Fire1";
    private const float MinimumFireInterval = 0.01f;
    private const float MinAimDirectionSqr = 0.0001f;

    [SerializeField] private SOGun equippedGun;
    [SerializeField] private Transform shootOrigin;
    [SerializeField] private LayerMask aimLayerMask;

    private float _nextFireTime;
    private Camera _gameplayCamera;

    private void Awake()
    {
        if (shootOrigin == null)
        {
            shootOrigin = transform;
        }

        AcquireGameplayCamera();
    }

    private void Update()
    {
        if (equippedGun == null || equippedGun.AmmoType == null)
        {
            return;
        }

        if (!Input.GetButton(FireInputName))
        {
            return;
        }

        if (Time.time < _nextFireTime)
        {
            return;
        }

        FireGun();
        float fireRate = Mathf.Max(MinimumFireInterval, equippedGun.FireRate);
        _nextFireTime = Time.time + fireRate;
    }

    private void FireGun()
    {
        if (_gameplayCamera == null)
        {
            AcquireGameplayCamera();
        }

        Vector3 aimDirection = GetAimDirection();
        if (aimDirection.sqrMagnitude < MinAimDirectionSqr)
        {
            return;
        }

        for (int i = 0; i < equippedGun.BulletsPerShot; i++)
        {
            Vector3 shotDirection = ApplySpread(aimDirection);
            SpawnBullet(shotDirection);
        }
    }

    private void SpawnBullet(Vector3 direction)
    {
        SOBullet ammoDefinition = equippedGun.AmmoType;
        if (ammoDefinition.Prefab == null)
        {
            Debug.LogWarning("Ammo prefab is not assigned.", this);
            return;
        }

        GameObject bulletInstance = Instantiate(ammoDefinition.Prefab, shootOrigin.position, Quaternion.LookRotation(direction));
        if (!bulletInstance.TryGetComponent(out BulletProjectile projectile))
        {
            projectile = bulletInstance.AddComponent<BulletProjectile>();
        }

        float damage = equippedGun.UseGunDamageOverride ? equippedGun.DamageOverride : ammoDefinition.DamageAmount;
        float stun = equippedGun.UseGunDamageOverride ? equippedGun.StunOverride : ammoDefinition.StunGainAmount;

        projectile.Initialize(ammoDefinition.Speed, ammoDefinition.Lifetime, damage, stun, direction);
    }

    private Vector3 ApplySpread(Vector3 baseDirection)
    {
        if (equippedGun.SpreadAngle <= 0f || equippedGun.BulletsPerShot == 1)
        {
            return baseDirection;
        }

        float spreadAngle = Random.Range(-equippedGun.SpreadAngle, equippedGun.SpreadAngle);
        Quaternion spreadRotation = Quaternion.AngleAxis(spreadAngle, Vector3.up);
        Vector3 spreadDirection = spreadRotation * baseDirection;
        spreadDirection.Normalize();
        return spreadDirection;
    }

    private Vector3 GetAimDirection()
    {
        if (_gameplayCamera == null)
        {
            return Vector3.zero;
        }

        Ray aimRay = _gameplayCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(aimRay, out RaycastHit hitInfo, Mathf.Infinity, aimLayerMask))
        {
            Vector3 directionToHit = hitInfo.point - shootOrigin.position;
            directionToHit.y = 0f;
            directionToHit.Normalize();
            return directionToHit;
        }

        Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, shootOrigin.position.y, 0f));
        if (groundPlane.Raycast(aimRay, out float distance))
        {
            Vector3 planePoint = aimRay.GetPoint(distance);
            Vector3 direction = planePoint - shootOrigin.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > MinAimDirectionSqr)
            {
                direction.Normalize();
                return direction;
            }
        }

        return Vector3.zero;
    }

    private void AcquireGameplayCamera()
    {
        if (GameplayCameraProvider.TryGetGameplayCamera(out Camera camera))
        {
            _gameplayCamera = camera;
        }
        else
        {
            _gameplayCamera = Camera.main;
        }
    }
}

