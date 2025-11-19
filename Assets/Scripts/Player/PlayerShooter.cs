using UnityEngine;

/// <summary>
/// 控制玩家的射击行为，包括瞄准、开火和子弹生成。
/// </summary>
public class PlayerShooter : MonoBehaviour
{
    // 开火输入的名称
    private const string FireInputName = "Fire1";
    // 最小开火间隔
    private const float MinimumFireInterval = 0.01f;
    // 瞄准方向向量的最小平方长度，用于避免零向量问题
    private const float MinAimDirectionSqr = 0.0001f;

    [Tooltip("当前装备的枪械配置")]
    [SerializeField] private SOGun equippedGun;
    [Tooltip("子弹生成的起始位置")]
    [SerializeField] private Transform shootOrigin;
    [Tooltip("用于射线检测以确定瞄准点的层")]
    [SerializeField] private LayerMask aimLayerMask;

    // 下一次可以开火的时间
    private float _nextFireTime;
    // 游戏主摄像机
    private Camera _gameplayCamera;

    /// <summary>
    /// 初始化射击点和摄像机引用。
    /// </summary>
    private void Awake()
    {
        if (shootOrigin == null)
        {
            shootOrigin = transform;
        }

        AcquireGameplayCamera();
    }

    /// <summary>
    /// 每帧更新，检测开火输入并控制射速。
    /// </summary>
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

    /// <summary>
    /// 执行开火逻辑，计算瞄准方向并发射子弹。
    /// </summary>
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

    /// <summary>
    /// 生成并发射一颗子弹。
    /// </summary>
    /// <param name="direction">子弹的发射方向</param>
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

    /// <summary>
    /// 为子弹方向应用随机散射。
    /// </summary>
    /// <param name="baseDirection">基础射击方向</param>
    /// <returns>应用散射后的射击方向</returns>
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

    /// <summary>
    /// 获取玩家当前的瞄准方向。
    /// </summary>
    /// <returns>归一化的瞄准方向向量</returns>
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

    /// <summary>
    /// 获取游戏主摄像机。
    /// </summary>
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

