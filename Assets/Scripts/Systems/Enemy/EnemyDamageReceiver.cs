using UnityEngine;
using WF.Gameplay.Core.Data;
using WF.Gameplay.Core.Interfaces;

namespace WF.Gameplay.Systems.Enemy
{
    public class EnemyDamageReceiver : MonoBehaviour, IDamageable
    {
        [SerializeField] private EnemyActor actor;

        public void TakeDamage(DamageInfo context)
        {
            actor.NotifyDamaged(context);
        }
    }
}
