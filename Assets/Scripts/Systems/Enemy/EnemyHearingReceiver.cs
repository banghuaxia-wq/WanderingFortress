using UnityEngine;
using WF.Gameplay.Core.Data;
using WF.Gameplay.Core.Events;

namespace WF.Gameplay.Systems.Enemy
{
    // 敌人听觉接收器：订阅声音事件并写入目标记忆（中文注释）
    public class EnemyHearingReceiver : MonoBehaviour
    {
        [Header("Hearing")]
        [SerializeField] private float hearingSensitivity = 1f; // 听觉灵敏度倍率（中文注释）

        [Header("Filter")]
        [SerializeField] private bool hearFootstep = true; // 是否响应脚步声（中文注释）
        [SerializeField] private bool hearGunshot = true; // 是否响应枪声（中文注释）
        [SerializeField] private bool hearCombat = true; // 是否响应战斗声（中文注释）
        [SerializeField] private bool hearImpact = true; // 是否响应撞击声（中文注释）
        [SerializeField] private bool hearSkill = true; // 是否响应技能声（中文注释）
        [SerializeField] private bool hearOther = true; // 是否响应其他声音（中文注释）

        private EnemyTargetMemory _memory; // 目标记忆引用（中文注释）

        // 绑定目标记忆（由行为中心在Awake调用）（中文注释）
        public void BindMemory(EnemyTargetMemory memory)
        {
            _memory = memory;
        }

        private void OnEnable()
        {
            EventBus.Subscribe<SoundEmittedEvent>(OnSoundEmitted);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<SoundEmittedEvent>(OnSoundEmitted);
        }

        private void OnSoundEmitted(SoundEmittedEvent e)
        {
            if (_memory == null) return;
            if (e.Source != null && e.Source == gameObject) return;
            if (e.Source != null && e.Source.GetComponentInParent<EnemyActor>() != null) return;
            if (!AcceptType(e.Type)) return;

            float radius = Mathf.Max(0f, e.Radius) * Mathf.Max(0f, hearingSensitivity);
            if (radius <= 0.001f) return;

            Vector3 delta = e.Position - transform.position;
            delta.y = 0f;
            float sqr = delta.sqrMagnitude;
            if (sqr > radius * radius) return;

            _memory.RecordHeard(e.Position, e.Source != null ? e.Source.transform : null);
        }

        private bool AcceptType(SoundType type)
        {
            switch (type)
            {
                case SoundType.Footstep: return hearFootstep;
                case SoundType.Gunshot: return hearGunshot;
                case SoundType.Combat: return hearCombat;
                case SoundType.Impact: return hearImpact;
                case SoundType.Skill: return hearSkill;
                default: return hearOther;
            }
        }
    }
}
