using UnityEngine;

namespace WF.Gameplay
{
    /// <summary>
    /// 玩家基础属性：生命、体力、饥饿、负重，以及体力的消耗/恢复参数。
    /// </summary>
    public class PlayerStats : MonoBehaviour
    {
        [Header("Base Stats (Max Values)")]
        [Tooltip("最大生命值")]
        public float maxHealth = 100f;
        [Tooltip("最大体力值")]
        public float maxStamina = 100f;
        [Tooltip("最大饥饿值（越低越饥饿，可根据设计反向使用）")]
        public float maxHunger = 100f;
        [Tooltip("最大可承载重量（用于背包/负重系统）")]
        public float maxCarryWeight = 50f;

        [Header("Current Stats")]
        [Tooltip("当前生命值")]
        public float health = 100f;
        [Tooltip("当前体力值")]
        public float stamina = 100f;
        [Tooltip("当前饥饿值")]
        public float hunger = 100f;
        [Tooltip("当前负重")]
        public float currentCarryWeight = 0f;

    [Header("Stamina Settings")]
    [Tooltip("奔跑时每秒体力消耗量")]
    public float sprintDrainPerSecond = 15f;
    [Tooltip("非奔跑延迟后每秒体力恢复量（延迟生效）")]
    public float staminaRegenPerSecond = 25f;
    [Tooltip("离开奔跑后开始恢复的延迟（秒）")]
    public float staminaRegenDelaySeconds = 2f;
    [Tooltip("允许开始奔跑所需的最小体力阈值")]
    public float minStaminaToSprint = 5f;
    [Tooltip("翻滚消耗的体力值")]
    public float rollStaminaCost = 20f;

        /// <summary>
        /// 体力的归一化值，范围 [0,1]
        /// </summary>
        public float StaminaNormalized => maxStamina > 0f ? Mathf.Clamp01(stamina / maxStamina) : 0f;

        /// <summary>
        /// 生命的归一化值，范围 [0,1]
        /// </summary>
        public float HealthNormalized => maxHealth > 0f ? Mathf.Clamp01(health / maxHealth) : 0f;

        /// <summary>
        /// 饥饿归一化（根据具体设计可反向使用）。
        /// </summary>
        public float HungerNormalized => maxHunger > 0f ? Mathf.Clamp01(hunger / maxHunger) : 0f;

        /// <summary>
        /// 消耗体力。
        /// </summary>
        public void ConsumeStamina(float amount)
        {
            if (amount <= 0f) return;
            stamina = Mathf.Max(0f, stamina - amount);
        }

        /// <summary>
        /// 恢复体力。
        /// </summary>
        public void RecoverStamina(float amount)
        {
            if (amount <= 0f) return;
            stamina = Mathf.Min(maxStamina, stamina + amount);
        }

        /// <summary>
        /// 直接设置体力，已做范围限制。
        /// </summary>
        public void SetStamina(float value)
        {
            stamina = Mathf.Clamp(value, 0f, maxStamina);
        }

        /// <summary>
        /// 是否有足够体力用于启动奔跑。
        /// </summary>
        public bool HasStaminaToSprint() => stamina >= minStaminaToSprint;

        public void ApplyDamage(float amount)
        {
            if (amount <= 0f) return;
            health = Mathf.Max(0f, health - amount);
        }
    }
}
