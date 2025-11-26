using WF.Gameplay.Core.Data;

namespace WF.Gameplay.Core.Interfaces
{
    public interface IDamageable
    {
        void TakeDamage(DamageInfo context);
    }
}
