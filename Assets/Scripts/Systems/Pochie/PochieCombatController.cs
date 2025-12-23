using System;
using UnityEngine;
using WF.Gameplay.Core.Data;
using WF.Gameplay.Systems.Buffs;
using WF.Gameplay.Core.Interfaces;

/// <summary>
/// 控制Pochie的战斗行为，包括生命值、眩晕、驯服和状态转换。
/// </summary>
namespace WF.Gameplay.Systems.Pochie
{
    public class PochieCombatController : MonoBehaviour, IDamageable, WF.Gameplay.Core.Interfaces.IPoolable
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
        private const string DebugStateFormat = "[PochieCombat] {0} | {1} | 状态: {2} | 生命: {3:0.##}/{4:0.##} | 眩晕: {5:0.##}/{6:0.##}";
        // 调试受击信息格式
        private const string DebugHitFormat = "[PochieCombat] {0} 受到攻击 -> 生命 -{1:0.##} 眩晕 +{2:0.##}";
        // 调试眩晕信息
        private const string DebugStunMessage = "[PochieCombat] {0} 已被眩晕";
        // 调试捕获信息
        private const string DebugCaptureMessage = "[PochieCombat] {0} 已被驯服并成为友方";

        [Tooltip("Pochie的属性配置")]
        [SerializeField] private SOPochie pochieStats;
        [Tooltip("可以驯服Pochie的最大距离")]
        [SerializeField] private float tameRange = 2.5f;
        //TODO:写在manager里面
        [Tooltip("玩家的Transform组件")]
        [SerializeField] private Transform playerTransform;
        [Tooltip("Pochie的刚体组件")]
        [SerializeField] private Rigidbody pochieRigidbody;

        [Tooltip("是否启用调试日志")]
        [SerializeField] private bool enableDebugLogs = false;

        [Header("调试面板")]
        [Tooltip("是否在检视面板中更新调试信息")]
        [SerializeField] private bool updateInspectorDebug = false;
        [Tooltip("当前的Pochie状态（仅供调试）")]
        [SerializeField] private PochieState debugCurrentState;
        [Tooltip("与玩家的距离（仅供调试）")]
        [SerializeField] private float debugDistanceToPlayer;
        [Tooltip("玩家是否在驯服范围内（仅供调试）")]
        [SerializeField] private bool debugPlayerInTameRange;

        // Pochie当前的状态
        private PochieState _currentState = PochieState.Active;
        // 当前生命值
        private float _currentHealth;
        // 当前眩晕值
        private float _currentStun;
        // 驯服范围的平方，用于优化距离计算
        private float _cachedTameRangeSqr;
        private bool _stunDecayBlocked;
        private BuffManager _buffManager;

        // -- 事件定义 --
        /// <summary>
        /// 当生命值更新时触发。参数：当前生命值, 最大生命值
        /// </summary>
        public event Action<float, float> OnHealthChanged;

        /// <summary>
        /// 当眩晕值更新时触发。参数：当前眩晕值, 最大眩晕值
        /// </summary>
        public event Action<float, float> OnStunChanged;

        /// <summary>
        /// 当Pochie进入眩晕状态时触发。
        /// </summary>
        public event Action OnStunned;

        /// <summary>
        /// 当Pochie被驯服时触发。
        /// </summary>
        public event Action OnTamed;

        /// <summary>
        /// 当Pochie被击败时触发。
        /// </summary>
        public event Action OnDefeated;

        /// <summary>
        /// 当需要显示或更新提示信息时触发。参数：提示信息, 是否显示
        /// </summary>
        public event Action<string, bool> OnTooltipChanged;

        /// <summary>
        /// 当需要显示状态UI时触发
        /// </summary>
        public event Action OnShowStatus;

        /// <summary>
        /// 初始化时触发，用于传递初始状态。参数：最大生命值, 最大眩晕值
        /// </summary>
        public event Action<float, float> OnInitialized;


        /// <summary>
        /// 初始化组件引用。
        /// </summary>
        private void Awake()
        {
            if (pochieRigidbody == null)
            {
                pochieRigidbody = GetComponent<Rigidbody>();
            }
            if (pochieRigidbody != null)
            {
                pochieRigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
                pochieRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                pochieRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            }

            var capsule = GetComponent<CapsuleCollider>();
            if (capsule != null)
            {
                capsule.direction = 1;
            }

            if (playerTransform == null)
            {
                GameObject playerObject = GameObject.FindGameObjectWithTag(PlayerTag);
                if (playerObject != null)
                {
                    playerTransform = playerObject.transform;
                }
            }

            _buffManager = GetComponent<WF.Gameplay.Systems.Buffs.BuffManager>();
        }

        /// <summary>
        /// 在游戏开始时调用，初始化战斗状态。
        /// </summary>
        private void Start()
        {
            if (pochieStats == null)
            {
                enabled = false;
                Debug.LogError("Pochie stats asset is not assigned.", this);
                return;
            }
            InitializeCombatState();
        }

        /// <summary>
        /// 每帧更新，处理眩晕衰减和驯服交互。
        /// </summary>
        private void Update()
        {
            if (_currentState == PochieState.Active)
            {
                UpdateStunDecay();
            }

            if (_currentState == PochieState.Stunned)
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
        /// 初始化Pochie的战斗状态。
        /// </summary>
        public void InitializeCombatState()
        {
            if (pochieStats == null)
            {
                return;
            }

            _currentHealth = pochieStats.MaxHealth;
            _currentStun = MinValue;
            _currentState = PochieState.Active;
            float tameRadius = tameRange + TooltipBuffer;
            _cachedTameRangeSqr = tameRadius * tameRadius;

            OnInitialized?.Invoke(pochieStats.MaxHealth, pochieStats.MaxStunValue);
            OnHealthChanged?.Invoke(_currentHealth, pochieStats.MaxHealth);
            OnStunChanged?.Invoke(_currentStun, pochieStats.MaxStunValue);


            LogStateDebug("初始化");
        }

        // 设置Pochie的配置数据（用于工厂创建后初始化）
        public void SetStats(WF.Gameplay.Core.Data.SOPochie data)
        {
            pochieStats = data;
        }

        /// <summary>
        /// 处理来自子弹的伤害与眩晕叠加。
        /// </summary>
        /// <param name="damageAmount">伤害值</param>
        /// <param name="stunGain">增加的眩晕值</param>
        public void ReceiveProjectileHit(float damageAmount, float stunGain)
        {
            if (_currentState == PochieState.Ally || pochieStats == null)
            {
                return;
            }

            if (enableDebugLogs)
            {
                Debug.Log(string.Format(DebugHitFormat, name, damageAmount, stunGain), this);
            }

            _currentHealth = Mathf.Max(MinValue, _currentHealth - damageAmount);
            _currentStun = Mathf.Min(pochieStats.MaxStunValue, _currentStun + stunGain);

            OnShowStatus?.Invoke();
            OnHealthChanged?.Invoke(_currentHealth, pochieStats.MaxHealth);
            OnStunChanged?.Invoke(_currentStun, pochieStats.MaxStunValue);

            if (_currentStun >= pochieStats.MaxStunValue && _currentState != PochieState.Stunned)
            {
                EnterStunnedState();
            }

            if (_currentHealth <= MinValue && _currentState != PochieState.Ally)
            {
                HandleDefeat();
            }

            LogStateDebug("受击后");
        }

        public void TakeDamage(DamageInfo info)
        {
            if (_currentState == PochieState.Ally || pochieStats == null) return;
            float damageAmount = info != null ? Mathf.Max(MinValue, info.Damage) : MinValue;
            float stunGain = info != null ? Mathf.Max(MinValue, info.InstantStun) : MinValue;
            _currentHealth = Mathf.Max(MinValue, _currentHealth - damageAmount);
            _currentStun = Mathf.Min(pochieStats.MaxStunValue, _currentStun + stunGain);
            if (info != null && info.AppliedBuffs != null)
            {
                var bm = _buffManager;
                if (bm == null) bm = GetComponent<WF.Gameplay.Systems.Buffs.BuffManager>();
                if (bm != null)
                {
                    for (int i = 0; i < info.AppliedBuffs.Count; i++)
                    {
                        var ab = info.AppliedBuffs[i];
                        if (ab.Buff != null && ab.ExtraValue > 0f)
                        {
                            bm.AddBuff(ab.Buff, info.Source != null ? info.Source : gameObject, ab.ExtraValue);
                        }
                    }
                }
            }
            OnShowStatus?.Invoke();
            OnHealthChanged?.Invoke(_currentHealth, pochieStats.MaxHealth);
            OnStunChanged?.Invoke(_currentStun, pochieStats.MaxStunValue);
            if (_currentStun >= pochieStats.MaxStunValue && _currentState != PochieState.Stunned)
            {
                EnterStunnedState();
            }
            if (_currentHealth <= MinValue && _currentState != PochieState.Ally)
            {
                HandleDefeat();
            }
            LogStateDebug("接口受击");
        }

        public void OnRecycle()
        {
            StopMovementImmediate();
            if (pochieStats != null)
            {
                _currentHealth = pochieStats.MaxHealth;
                _currentStun = MinValue;
            }
            _currentState = PochieState.Active;
            _stunDecayBlocked = false;
            var bm = _buffManager != null ? _buffManager : GetComponent<WF.Gameplay.Systems.Buffs.BuffManager>();
            if (bm != null)
            {
                for (int i = bm.buffs.Count - 1; i >= 0; i--)
                {
                    var data = bm.buffs[i].BuffData;
                    if (data != null && !string.IsNullOrEmpty(data.Id)) bm.RemoveBuff(data.Id);
                }
            }
        }

        public void ReceiveBuffDamage(float damageAmount)
        {
            if (_currentState == PochieState.Ally || pochieStats == null)
            {
                return;
            }
            _currentHealth = Mathf.Max(MinValue, _currentHealth - damageAmount);
            OnShowStatus?.Invoke();
            OnHealthChanged?.Invoke(_currentHealth, pochieStats.MaxHealth);
            if (_currentHealth <= MinValue && _currentState != PochieState.Ally)
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
            if (pochieStats == null) return;

            float dt = Time.deltaTime;
            if (pochieStats.stunAutoIncreaseRate > 0f)
            {
                _currentStun = Mathf.Min(pochieStats.MaxStunValue, _currentStun + pochieStats.stunAutoIncreaseRate * dt);
            }

            if (!_stunDecayBlocked && _currentStun > MinValue && pochieStats.stunDecayRate > 0f)
            {
                _currentStun = Mathf.Max(MinValue, _currentStun - pochieStats.stunDecayRate * dt);
            }

            OnStunChanged?.Invoke(_currentStun, pochieStats.MaxStunValue);
        }

        /// <summary>
        /// 进入眩晕状态。
        /// </summary>
        private void EnterStunnedState()
        {
            _currentState = PochieState.Stunned;
            _currentStun = pochieStats != null ? pochieStats.MaxStunValue : _currentStun;
            StopMovementImmediate();

            OnStunned?.Invoke();
            OnStunChanged?.Invoke(_currentStun, pochieStats.MaxStunValue);
            OnTooltipChanged?.Invoke("按 E 驯服", true);

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

            OnTooltipChanged?.Invoke("按 E 驯服", isInRange);

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
        /// 将Pochie转换为盟友。
        /// </summary>
        private void BecomeAlly()
        {
            _currentState = PochieState.Ally;
            if (pochieStats != null)
            {
                _currentHealth = pochieStats.MaxHealth;
            }
            _currentStun = MinValue;

            OnTamed?.Invoke();
            OnHealthChanged?.Invoke(_currentHealth, pochieStats.MaxHealth);
            OnStunChanged?.Invoke(_currentStun, pochieStats.MaxStunValue);


            if (enableDebugLogs)
            {
                Debug.Log(string.Format(DebugCaptureMessage, name), this);
            }

            LogStateDebug("驯服成功");
        }

        /// <summary>
        /// 处理Pochie被击败的逻辑。
        /// </summary>
        private void HandleDefeat()
        {
            _currentState = PochieState.Stunned;
            StopMovementImmediate();

            OnDefeated?.Invoke();
            OnTooltipChanged?.Invoke("Pochie倒下", true);

            LogStateDebug("生命耗尽");
        }

        public void SetStunDecayBlocked(bool blocked)
        {
            _stunDecayBlocked = blocked;
        }

        /// <summary>
        /// 立即停止Pochie的移动。
        /// </summary>
        private void StopMovementImmediate()
        {
            if (pochieRigidbody != null)
            {
                pochieRigidbody.velocity = Vector3.zero;
                pochieRigidbody.angularVelocity = Vector3.zero;
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
            if (!enableDebugLogs || pochieStats == null)
            {
                return;
            }

            Debug.Log(string.Format(
                DebugStateFormat,
                name,
                label,
                _currentState,
                _currentHealth,
                pochieStats.MaxHealth,
                _currentStun,
                pochieStats.MaxStunValue), this);
        }
    }
}
