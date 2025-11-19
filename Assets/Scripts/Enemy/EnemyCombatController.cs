using UnityEngine;

/// <summary>
/// 控制敌人的战斗行为，包括生命值、眩晕、驯服和状态转换。
/// </summary>
public class EnemyCombatController : MonoBehaviour
{
    // 玩家对象的标签
    private const string PlayerTag = "Player";
    // 驯服按键
    private const KeyCode TameKey = KeyCode.E;
    // 最小值
    private const float MinValue = 0f;
    // 提示信息的缓冲范围
    private const float TooltipBuffer = 0.1f;
    // 调试状态格式
    private const string DebugStateFormat = "[EnemyCombat] {0} | {1} | 状态: {2} | 生命: {3:0.##}/{4:0.##} | 眩晕: {5:0.##}/{6:0.##}";
    // 调试受击信息格式
    private const string DebugHitFormat = "[EnemyCombat] {0} 受到攻击 -> 生命 -{1:0.##} 眩晕 +{2:0.##}";
    // 调试眩晕信息
    private const string DebugStunMessage = "[EnemyCombat] {0} 已被眩晕";
    // 调试捕获信息
    private const string DebugCaptureMessage = "[EnemyCombat] {0} 已被驯服并成为友方";

    [Tooltip("敌人的属性配置")]
    [SerializeField] private ScriptableEnemy enemyStats;
    [Tooltip("用于显示敌人状态的UI界面")]
    [SerializeField] private EnemyStatusUI statusUI;
    [Tooltip("可以驯服敌人的最大距离")]
    [SerializeField] private float tameRange = 2.5f;
    [Tooltip("玩家的Transform组件")]
    [SerializeField] private Transform playerTransform;
    [Tooltip("敌人的刚体组件")]
    [SerializeField] private Rigidbody enemyRigidbody;
    [Tooltip("敌人的材质控制器")]
    [SerializeField] private EnemyMaterialController materialController;
    [Tooltip("盟友跟随控制器")]
    [SerializeField] private AllyFollower allyFollower;
    [Tooltip("是否启用调试日志")]
    [SerializeField] private bool enableDebugLogs = false;

    [Header("调试面板")]
    [Tooltip("是否在检视面板中更新调试信息")]
    [SerializeField] private bool updateInspectorDebug = false;
    [Tooltip("当前的敌人状态（仅供调试）")]
    [SerializeField] private EnemyState debugCurrentState;
    [Tooltip("与玩家的距离（仅供调试）")]
    [SerializeField] private float debugDistanceToPlayer;
    [Tooltip("玩家是否在驯服范围内（仅供调试）")]
    [SerializeField] private bool debugPlayerInTameRange;

    // 敌人当前的状态
    private EnemyState _currentState = EnemyState.Active;
    // 当前生命值
    private float _currentHealth;
    // 当前眩晕值
    private float _currentStun;
    // 驯服范围的平方，用于优化距离计算
    private float _cachedTameRangeSqr;

    /// <summary>
    /// 初始化组件引用。
    /// </summary>
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

    /// <summary>
    /// 在游戏开始时调用，初始化战斗状态。
    /// </summary>
    private void Start()
    {
        InitializeCombatState();
    }

    /// <summary>
    /// 每帧更新，处理眩晕衰减和驯服交互。
    /// </summary>
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

    /// <summary>
    /// 在所有Update函数调用后执行，用于刷新调试信息。
    /// </summary>
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
    /// <param name="damageAmount">伤害值</param>
    /// <param name="stunGain">增加的眩晕值</param>
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

    /// <summary>
    /// 更新眩晕值的衰减。
    /// </summary>
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

    /// <summary>
    /// 进入眩晕状态。
    /// </summary>
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

    /// <summary>
    /// 处理驯服交互。
    /// </summary>
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

    /// <summary>
    /// 将敌人转换为盟友。
    /// </summary>
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

    /// <summary>
    /// 处理敌人被击败的逻辑。
    /// </summary>
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

    /// <summary>
    /// 立即停止敌人的移动。
    /// </summary>
    private void StopMovementImmediate()
    {
        if (enemyRigidbody != null)
        {
            enemyRigidbody.velocity = Vector3.zero;
            enemyRigidbody.angularVelocity = Vector3.zero;
        }
    }

    /// <summary>
    /// 刷新检视面板中的调试信息。
    /// </summary>
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

    /// <summary>
    /// 记录状态调试信息。
    /// </summary>
    /// <param name="label">调试信息的标签</param>
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

