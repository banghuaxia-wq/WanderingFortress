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
    [SerializeField] private CrosshairController crosshair;

    // 下一次可以开火的时间
    private float _nextFireTime;
    // 游戏主摄像机
    private Camera _gameplayCamera;
    private float _verticalRecoil;
    private float _horizontalRecoil;
    private Vector2 _axisVertical;
    private Vector2 _axisHorizontal;

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
        if (crosshair == null) crosshair = FindObjectOfType<CrosshairController>();
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

        ApplyRecoilDecay();
        UpdateCrosshairOffset();

        if (!Input.GetButton(FireInputName))
        {
            return;
        }

        if (Time.time < _nextFireTime)
        {
            return;
        }

        FireGun();
        ApplyRecoilKick();
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

        GameObject bulletInstance = null;
        if (WF.Gameplay.PoolManager.Instance != null)
        {
            bulletInstance = WF.Gameplay.PoolManager.Instance.Get(ammoDefinition.Prefab);
            if (bulletInstance != null)
            {
                bulletInstance.transform.SetPositionAndRotation(shootOrigin.position, Quaternion.LookRotation(direction));
            }
        }
        if (bulletInstance == null)
        {
            bulletInstance = Instantiate(ammoDefinition.Prefab, shootOrigin.position, Quaternion.LookRotation(direction));
            var po = bulletInstance.GetComponent<WF.Gameplay.PooledObject>();
            if (po == null) po = bulletInstance.AddComponent<WF.Gameplay.PooledObject>();
            po.SourcePrefab = ammoDefinition.Prefab;
        }
        if (!bulletInstance.TryGetComponent(out Projectile projectile))
        {
            projectile = bulletInstance.AddComponent<Projectile>();
        }

        float damage = equippedGun.UseGunDamageOverride ? equippedGun.DamageOverride : ammoDefinition.DamageAmount;
        float instantStun = Mathf.Max(0f, damage * Mathf.Max(0f, equippedGun.WeaponStunCoefficient));
        float delayedStun = instantStun * 0.5f;
        var payload = new WF.Gameplay.DamageInfo
        {
            Source = gameObject,
            Damage = damage,
            InstantStun = instantStun,
            Type = WF.Gameplay.DamageType.Physical,
            AppliedBuffs = null
        };
        if (ammoDefinition.SedativeBuffData != null && delayedStun > 0f)
        {
            payload.AppliedBuffs = new System.Collections.Generic.List<WF.Gameplay.AppliedBuff>
            {
                new WF.Gameplay.AppliedBuff { Buff = ammoDefinition.SedativeBuffData, ExtraValue = delayedStun }
            };
        }
        projectile.Initialize(ammoDefinition.Speed, ammoDefinition.Lifetime, direction, payload);
    }

    /// <summary>
    /// 为子弹方向应用随机散射。
    /// </summary>
    /// <param name="baseDirection">基础射击方向</param>
    /// <returns>应用散射后的射击方向</returns>
    private Vector3 ApplySpread(Vector3 baseDirection)
    {
        float angle = Mathf.Max(0f, equippedGun.SpreadAngle);
        if (angle <= 0f)
        {
            return baseDirection.normalized;
        }
        baseDirection.Normalize();
        float angleRad = angle * Mathf.Deg2Rad;
        float u = Random.value;
        float cosTheta = Mathf.Lerp(Mathf.Cos(angleRad), 1f, u);
        float sinTheta = Mathf.Sqrt(1f - cosTheta * cosTheta);
        float phi = Random.value * Mathf.PI * 2f;
        Vector3 n = baseDirection;
        Vector3 arbitrary = Mathf.Abs(n.y) < 0.99f ? Vector3.up : Vector3.right;
        Vector3 t = Vector3.Normalize(Vector3.Cross(arbitrary, n));
        Vector3 b = Vector3.Cross(n, t);
        Vector3 dir = t * (sinTheta * Mathf.Cos(phi)) + b * (sinTheta * Mathf.Sin(phi)) + n * cosTheta;
        dir.Normalize();
        return dir;
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

        Vector3 playerScreen = _gameplayCamera.WorldToScreenPoint(shootOrigin.position);
        Vector2 toMouse = new Vector2(Input.mousePosition.x - playerScreen.x, Input.mousePosition.y - playerScreen.y);
        if (toMouse.sqrMagnitude < MinAimDirectionSqr) return Vector3.zero;
        toMouse.Normalize();
        Vector2 perp = new Vector2(-toMouse.y, toMouse.x);
        _axisVertical = toMouse;
        _axisHorizontal = perp;
        Vector2 offset = _axisVertical * _verticalRecoil + _axisHorizontal * _horizontalRecoil;
        Vector3 mouse = new Vector3(Input.mousePosition.x + offset.x, Input.mousePosition.y + offset.y, 0f);
        Ray aimRay = _gameplayCamera.ScreenPointToRay(mouse);

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

    private void ApplyRecoilKick()
    {
        if (equippedGun == null) return;
        Vector3 playerScreen = _gameplayCamera != null ? _gameplayCamera.WorldToScreenPoint(shootOrigin.position) : new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
        Vector2 toMouse = new Vector2(Input.mousePosition.x - playerScreen.x, Input.mousePosition.y - playerScreen.y);
        if (toMouse.sqrMagnitude < MinAimDirectionSqr) return;
        toMouse.Normalize();
        Vector2 perp = new Vector2(-toMouse.y, toMouse.x);
        _axisVertical = toMouse;
        _axisHorizontal = perp;
        float v = equippedGun.VerticalRecoilPerShot;
        float h = equippedGun.HorizontalRecoilPerShot * (equippedGun.RandomizeHorizontalDirection ? (Random.value < 0.5f ? -1f : 1f) : 1f);
        _verticalRecoil = Mathf.Clamp(_verticalRecoil + v, -equippedGun.VerticalRecoilMax, equippedGun.VerticalRecoilMax);
        _horizontalRecoil = Mathf.Clamp(_horizontalRecoil + h, -equippedGun.HorizontalRecoilMax, equippedGun.HorizontalRecoilMax);
    }

    private void ApplyRecoilDecay()
    {
        if (equippedGun == null) return;
        float decay = Mathf.Max(0f, equippedGun.RecoilDecayPerSecond) * Time.deltaTime;
        _verticalRecoil = Mathf.MoveTowards(_verticalRecoil, 0f, decay);
        _horizontalRecoil = Mathf.MoveTowards(_horizontalRecoil, 0f, decay);
    }

    private void UpdateCrosshairOffset()
    {
        if (crosshair == null || _gameplayCamera == null) return;
        Vector3 playerScreen = _gameplayCamera.WorldToScreenPoint(shootOrigin.position);
        Vector2 toMouse = new Vector2(Input.mousePosition.x - playerScreen.x, Input.mousePosition.y - playerScreen.y);
        if (toMouse.sqrMagnitude < MinAimDirectionSqr)
        {
            crosshair.SetExternalOffset(Vector2.zero);
            return;
        }
        toMouse.Normalize();
        Vector2 perp = new Vector2(-toMouse.y, toMouse.x);
        Vector2 offset = toMouse * _verticalRecoil + perp * _horizontalRecoil;
        crosshair.SetExternalOffset(offset);
    }
}

