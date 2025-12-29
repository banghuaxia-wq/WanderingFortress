using UnityEngine;
using WF.Gameplay.Core.Data;
using WF.Gameplay.Core.Interfaces;

namespace WF.Gameplay.Systems.Animation
{
    [RequireComponent(typeof(Animator))]
    public class AnimatorAnimationDriver : MonoBehaviour, IAnimationDriver
    {
        private static readonly int AttackTypeHash = Animator.StringToHash("AttackType");
        private static readonly int AttackTriggerHash = Animator.StringToHash("Attack");

        [SerializeField] private Animator _animator;

        private void Awake()
        {
            if (_animator == null)
            {
                _animator = GetComponent<Animator>();
            }
        }

        public void SetAttackType(AttackType attackType)
        {
            if (_animator == null) return;
            _animator.SetInteger(AttackTypeHash, (int)attackType);
        }

        public void TriggerAttack()
        {
            if (_animator == null) return;
            _animator.SetTrigger(AttackTriggerHash);
        }
    }
}

