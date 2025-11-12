using UnityEngine;

public class EnemyCombatController : MonoBehaviour
{
    private const string PlayerTag = "Player";
    private const KeyCode TameKey = KeyCode.E;
    private const float MinValue = 0f;
    private const float TooltipBuffer = 0.1f;
    private const string DebugStateFormat = "[EnemyCombat] {0} | {1} | 状态: {2} | 生命: {3:0.##}/{4:0.##} | 眩晕: {5:0.##}/{6:0.##}";
    private const string DebugHitFormat = "[EnemyCombat] {0} 受到攻击 -> 生命 -{1:0.##} 眩晕 +{2:0.##}";
    private const string DebugStunMessage = "[EnemyCombat] {0} 已被眩晕";
    private const string DebugCaptureMessage = "[EnemyCombat] {0} 已被驯服并成为友方";

    [SerializeField] private ScriptableEnemy enemyStats;
    [SerializeField] private EnemyStatusUI statusUI;
    [SerializeField] private float tameRange = 2.5f;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Rigidbody enemyRigidbody;
    [SerializeField] private EnemyMaterialController materialController;
    [SerializeField] private AllyFollower allyFollower;
    [SerializeField] private bool enableDebugLogs = false;

    [Header("调试面板")]
    [SerializeField] private bool updateInspectorDebug = false;
    [SerializeField] private EnemyState debugCurrentState;
    [SerializeField] private float debugDistanceToPlayer;
    [SerializeField] private bool debugPlayerInTameRange;

    private EnemyState _currentState = EnemyState.Active;
    private float _currentHealth;
    private float _currentStun;
    private float _cachedTameRangeSqr;

    private void Awake()
    {
        if (enemyRigidbody == null)
        {
            enemyRigidbody = GetComponent<Rigidbody>();
        }

        if (materialController == null)
        {
            materialController = GetComponent<EnemyMaterialController>();
        }

        if (allyFollower == null)
        {
            allyFollower = GetComponent<AllyFollower>();
        }

        if (playerTransform == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag(PlayerTag);
            if (playerObject != null)
            {
                playerTransform = playerObject.transform;
            }
        }

        if (enemyStats == null)
        {
            Debug.LogError("Enemy stats asset is not assigned.", this);
        }
    }

    private void Start()
    {
        InitializeCombatState();
    }

    private void Update()
    {
        if (_currentState == EnemyState.Active)
        {
            UpdateStunDecay();
        }

        if (_currentState == EnemyState.Stunned)
        {
            HandleTameInteraction();
        }
    }

    private void LateUpdate()
    {
        if (updateInspectorDebug)
        {
            RefreshDebugInfo();
        }
    }

    /// <summary>
    /// 初始化敌人的战斗状态。
    /// </summary>
    public void InitializeCombatState()
    {
        if (enemyStats == null)
        {
            return;
        }

        _currentHealth = enemyStats.MaxHealth;
        _currentStun = MinValue;
        _currentState = EnemyState.Active;
        float tameRadius = tameRange + TooltipBuffer;
        _cachedTameRangeSqr = tameRadius * tameRadius;

        if (statusUI != null)
        {
            statusUI.AttachToTarget(transform);
            statusUI.ConfigureBars(enemyStats.MaxHealth, enemyStats.MaxStunValue);
            statusUI.UpdateHealthBar(_currentHealth);
            statusUI.UpdateStunBar(_currentStun);
            statusUI.HideTooltip();
            statusUI.HideStatusImmediate();
        }

        if (materialController != null)
        {
            materialController.ApplyDefaultMaterial();
        }

        if (allyFollower != null)
        {
            allyFollower.DeactivateFollow();
        }

        LogStateDebug("初始化");
    }

    /// <summary>
    /// 处理来自子弹的伤害与眩晕叠加。
    /// </summary>
    public void ReceiveProjectileHit(float damageAmount, float stunGain)
    {
        if (_currentState == EnemyState.Ally || enemyStats == null)
        {
            return;
        }

        if (enableDebugLogs)
        {
            Debug.Log(string.Format(DebugHitFormat, name, damageAmount, stunGain), this);
        }

        _currentHealth = Mathf.Max(MinValue, _currentHealth - damageAmount);
        _currentStun = Mathf.Min(enemyStats.MaxStunValue, _currentStun + stunGain);

        if (statusUI != null)
        {
            statusUI.UpdateHealthBar(_currentHealth);
            statusUI.UpdateStunBar(_currentStun);
            statusUI.ShowStatus();
        }

        if (_currentStun >= enemyStats.MaxStunValue && _currentState != EnemyState.Stunned)
        {
            EnterStunnedState();
        }

        if (_currentHealth <= MinValue && _currentState != EnemyState.Ally)
        {
            HandleDefeat();
        }

        LogStateDebug("受击后");
    }

    private void UpdateStunDecay()
    {
        if (_currentStun <= MinValue || enemyStats == null)
        {
            return;
        }

        _currentStun = Mathf.Max(MinValue, _currentStun - enemyStats.StunDecayRate * Time.deltaTime);
        if (statusUI != null)
        {
            statusUI.UpdateStunBar(_currentStun);
        }
    }

    private void EnterStunnedState()
    {
        _currentState = EnemyState.Stunned;
        _currentStun = enemyStats != null ? enemyStats.MaxStunValue : _currentStun;
        StopMovementImmediate();

        if (statusUI != null)
        {
            statusUI.UpdateStunBar(_currentStun);
            statusUI.ShowStatus();
            statusUI.ShowTooltip("按 E 驯服");
        }

        if (enableDebugLogs)
        {
            Debug.Log(string.Format(DebugStunMessage, name), this);
        }

        LogStateDebug("眩晕");
    }

    private void HandleTameInteraction()
    {
        if (playerTransform == null)
        {
            return;
        }

        Vector3 offsetToPlayer = playerTransform.position - transform.position;
        float distanceSqr = offsetToPlayer.sqrMagnitude;
        bool isInRange = distanceSqr <= _cachedTameRangeSqr;

        if (statusUI != null)
        {
            if (isInRange)
            {
                statusUI.ShowTooltip("按 E 驯服");
            }
            else
            {
                statusUI.HideTooltip();
            }
        }

        if (!isInRange)
        {
            return;
        }

        if (Input.GetKeyDown(TameKey))
        {
            BecomeAlly();
        }
    }

    private void BecomeAlly()
    {
        _currentState = EnemyState.Ally;
        if (enemyStats != null)
        {
            _currentHealth = enemyStats.MaxHealth;
        }
        _currentStun = MinValue;

        if (statusUI != null)
        {
            statusUI.HideTooltip();
            statusUI.ShowStatus();
            statusUI.UpdateStunBar(_currentStun);
            statusUI.UpdateHealthBar(_currentHealth);
        }

        if (materialController != null)
        {
            materialController.ApplyAllyMaterial();
        }

        if (allyFollower != null && playerTransform != null)
        {
            allyFollower.ActivateFollow(playerTransform);
        }

        if (enableDebugLogs)
        {
            Debug.Log(string.Format(DebugCaptureMessage, name), this);
        }

        LogStateDebug("驯服成功");
    }

    private void HandleDefeat()
    {
        _currentState = EnemyState.Stunned;
        StopMovementImmediate();

        if (statusUI != null)
        {
            statusUI.ShowTooltip("敌人倒下");
        }

        LogStateDebug("生命耗尽");
    }

    private void StopMovementImmediate()
    {
        if (enemyRigidbody != null)
        {
            enemyRigidbody.velocity = Vector3.zero;
            enemyRigidbody.angularVelocity = Vector3.zero;
        }
    }

    private void RefreshDebugInfo()
    {
        debugCurrentState = _currentState;
        if (playerTransform != null)
        {
            debugDistanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            debugPlayerInTameRange = debugDistanceToPlayer <= (tameRange + TooltipBuffer);
        }
        else
        {
            debugDistanceToPlayer = 0f;
            debugPlayerInTameRange = false;
        }
    }

    private void LogStateDebug(string label)
    {
        if (!enableDebugLogs || enemyStats == null)
        {
            return;
        }

        Debug.Log(string.Format(
            DebugStateFormat,
            name,
            label,
            _currentState,
            _currentHealth,
            enemyStats.MaxHealth,
            _currentStun,
            enemyStats.MaxStunValue), this);
    }
}

