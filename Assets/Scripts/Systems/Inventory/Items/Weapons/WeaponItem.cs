using UnityEngine;
using WF.Gameplay.Core.Data;
using WF.Gameplay.Core.Interfaces;

namespace WF.Gameplay.Systems.Inventory.Items.Weapons
{
    public abstract class WeaponItem : Items.WeaponItem, IWeaponItem
    {
        [SerializeField] private WeaponCategory category;
        [SerializeField] private AttackData attackData;
        [SerializeField] private int maxAmmo;
        [SerializeField] private ScriptableObject attackBehavior; // Reference to behavior SO

        public WeaponCategory Category => category;
        public AttackData AttackData => attackData;
        public int MaxAmmo => maxAmmo;
        public int CurrentAmmo { get; protected set; }
        
        // This will be resolved at runtime or via factory
        public IAttackBehavior AttackBehavior => attackBehavior as IAttackBehavior;

        public virtual bool CanReload() => CurrentAmmo < MaxAmmo;
        
        public virtual void TryReload(IPlayerInventory inventory)
        {
            // Base reload logic (can be overridden)
        }
        
        public virtual void ConsumeAmmo(int amount)
        {
            CurrentAmmo = Mathf.Max(0, CurrentAmmo - amount);
        }
        
        protected virtual void OnEnable()
        {
            CurrentAmmo = maxAmmo; // Reset on load for now
        }
    }
}
