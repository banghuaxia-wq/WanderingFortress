using UnityEngine;
using WF.Gameplay.Core.Data;
using WF.Gameplay.Core.Interfaces;

namespace WF.Gameplay.Systems.Enemy
{
    // 敌人伤害接收器：将伤害转发到行为中心（中文注释）
    public class EnemyDamageReceiver : MonoBehaviour, IDamageable
    {
        [SerializeField] private EnemyActor actor; // 行为中心引用（中文注释）

        private void Awake()
        {
            if (actor == null) actor = GetComponent<EnemyActor>();
        }

        // 接收伤害（由投射物/近战等调用）（中文注释）
        public void TakeDamage(DamageInfo context)
        {
            if (actor == null) return;
            actor.NotifyDamaged(context);
        }
    }
}

