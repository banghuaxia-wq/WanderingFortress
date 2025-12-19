using WF.Gameplay.Core.Data;

namespace WF.Gameplay.Core.Interfaces
{
    public interface IWeaponItem : IEquippable
    {
        WeaponCategory Category { get; }
        IAttackBehavior AttackBehavior { get; }
        AttackData AttackData { get; }
        int CurrentAmmo { get; }
        int MaxAmmo { get; }
        bool CanReload();
        void TryReload(IPlayerInventory inventory);
        void ConsumeAmmo(int amount);
    }
}
