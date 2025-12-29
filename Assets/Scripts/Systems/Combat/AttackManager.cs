using UnityEngine;
using WF.Gameplay.Core.Events;
using WF.Gameplay.Core.Interfaces;

namespace WF.Gameplay.Systems.Combat
{
    public class AttackManager : MonoBehaviour
    {
        public static AttackManager Instance { get; private set; }
        
        private float _nextAttackTime;
        
        private void Awake() 
        { 
            if (Instance != null && Instance != this) { Destroy(gameObject); return; } 
            Instance = this; 
        }
        
        public bool TryAttack(GameObject owner, IWeaponItem weapon, Vector3 aimDirection, Vector3? origin = null)
        {
            if (weapon == null || weapon.AttackBehavior == null || weapon.AttackData == null) return false;
            
            if (Time.time < _nextAttackTime) return false;
            
            if (!weapon.AttackBehavior.CanExecute(owner, weapon, weapon.AttackData)) return false;
            
            weapon.AttackBehavior.Execute(owner, weapon, weapon.AttackData, aimDirection, origin);

            PublishAttackSound(owner, weapon.AttackData, origin);
            
            _nextAttackTime = Time.time + weapon.AttackData.Cooldown;
            return true;
        }

        private void PublishAttackSound(GameObject owner, WF.Gameplay.Core.Data.AttackData data, Vector3? origin)
        {
            if (data == null) return;
            if (data.NoiseRadius <= 0.001f) return;

            Vector3 pos = origin.HasValue ? origin.Value : (owner != null ? owner.transform.position : Vector3.zero);
            EventBus.Publish(new SoundEmittedEvent(pos, data.NoiseRadius, data.NoiseType, owner));
        }
    }
}
