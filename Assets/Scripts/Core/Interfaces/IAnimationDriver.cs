using WF.Gameplay.Core.Data;

namespace WF.Gameplay.Core.Interfaces
{
    public interface IAnimationDriver
    {
        void SetAttackType(AttackType attackType);
        void TriggerAttack();
    }
}

