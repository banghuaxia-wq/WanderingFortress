using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using WF.Gameplay.Systems.Camera;
using WF.Gameplay.Core.Events;
using UCamera = UnityEngine.Camera;

namespace WF.Gameplay.Systems.Player
{
    /// <summary>
    /// 玩家状态管理器（单例）。
    /// - 体力消耗/恢复的统一控制（基于 PlayerStats 参数）。
    /// - 为移动脚本提供体力门槛判断（CanSprint）。
    /// - 发布 StaminaChangedEvent 供 UI 监听。
    /// </summary>
    public class PlayerStateManager : MonoBehaviour
    {
        public static PlayerStateManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private PlayerStats playerStats;
        [SerializeField] private PlayerMove playerMove;

        [Header("Auto Bind")]
        [Tooltip("启动时尝试自动绑定玩家引用")]
        [SerializeField] private bool autoBindOnStart = true;

        [Header("Event Settings")]
        [Tooltip("体力事件的最小间隔（秒），用于限流；设置较小可提高UI更新频率")]
        [SerializeField] private float staminaEventInterval = 0.016f; // 默认约60FPS（中文注释）
        private float _lastStaminaEventTime;

        // 记录非奔跑状态持续时间，用于体力恢复延迟
        private float _timeNotSprinting;
        private float _invincibleTimer;
        private Dictionary<string, float> _moveSpeedMultipliers = new Dictionary<string, float>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (autoBindOnStart)
            {
                TryBindPlayer();
            }
        }

        private void Update()
        {
            if (playerStats == null)
            {
                TryBindPlayer();
            }

            if (playerMove == null)
            {
                TryBindPlayerMove();
            }

            if (playerStats == null || playerMove == null)
            {
                return;
            }

            // 体力消耗/恢复
            float dt = Time.deltaTime;
            bool changed = false;

            if (playerMove.IsSprinting)
            {
                // 奔跑：消耗体力并重置“非奔跑计时”
                if (playerStats.stamina > 0) changed = true;
                playerStats.ConsumeStamina(playerStats.sprintDrainPerSecond * dt);
                _timeNotSprinting = 0f;

                if (playerStats.stamina <= 0f)
                {
                    playerMove.ForceStopSprint();
                }
            }
            else
            {
                // 非奔跑：先累计非奔跑时间，超过延迟后再恢复体力（加速）
                _timeNotSprinting += dt;
                if (_timeNotSprinting >= playerStats.staminaRegenDelaySeconds)
                {
                    if (playerStats.stamina < playerStats.maxStamina) changed = true;
                    playerStats.RecoverStamina(playerStats.staminaRegenPerSecond * dt);
                }
            }

            if (changed)
            {
                // 每帧变更都发布，提高UI刷新频率（中文注释）
                EventBus.Publish(new StaminaChangedEvent(playerStats.stamina, playerStats.maxStamina, playerStats.stamina <= 0));
                _lastStaminaEventTime = Time.time;
            }

            if (_invincibleTimer > 0f)
            {
                _invincibleTimer -= Time.deltaTime;
                if (_invincibleTimer < 0f) _invincibleTimer = 0f;
            }
        }

        public bool CanSprint => playerStats != null && playerStats.HasStaminaToSprint();
        public bool CanRoll => playerStats != null && playerStats.stamina >= playerStats.rollStaminaCost;
        public bool IsInvincible => _invincibleTimer > 0f;
        public float MovementSpeedMultiplier
        {
            get
            {
                if (_moveSpeedMultipliers == null || _moveSpeedMultipliers.Count == 0) return 1f;
                float m = 1f;
                foreach (var kv in _moveSpeedMultipliers) m *= kv.Value;
                return m;
            }
        }

        public void SetInvincible(float duration)
        {
            _invincibleTimer = Mathf.Max(_invincibleTimer, duration);
        }

        public void ConsumeStaminaForRoll()
        {
            if (playerStats != null)
            {
                playerStats.ConsumeStamina(playerStats.rollStaminaCost);
                _timeNotSprinting = 0f; // Reset regen delay
                
                // Roll is an instant action, so we always publish immediately
                EventBus.Publish(new StaminaChangedEvent(playerStats.stamina, playerStats.maxStamina, playerStats.stamina <= 0));
                _lastStaminaEventTime = Time.time;
            }
        }

        public void SetMovementSpeedMultiplier(string key, float multiplier)
        {
            _moveSpeedMultipliers[key] = multiplier;
        }

        public void RemoveMovementSpeedMultiplier(string key)
        {
            if (_moveSpeedMultipliers.ContainsKey(key))
            {
                _moveSpeedMultipliers.Remove(key);
            }
        }

        public Dictionary<string, float> GetMovementSpeedMultipliersSnapshot()
        {
            return new Dictionary<string, float>(_moveSpeedMultipliers);
        }

        private void TryBindPlayer()
        {
            if (playerStats == null)
            {
                playerStats = FindObjectOfType<PlayerStats>();
            }
            TryBindPlayerMove();
        }

        private void TryBindPlayerMove()
        {
            if (playerMove == null)
            {
                playerMove = FindObjectOfType<PlayerMove>();
            }
        }
    }
}
