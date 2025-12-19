using System.Collections.Generic;
using UnityEngine;
using WF.Gameplay.Core.Data;
using WF.Gameplay.Core.Events;
using WF.Gameplay.Core.Interfaces;
using WF.Gameplay.Systems.Inventory.Items;
using WF.Gameplay.Systems.Inventory.Items.Weapons;
using WF.Gameplay.Systems.InventorySystem;

namespace WF.Gameplay.Systems.Combat
{
    public class WeaponManager : MonoBehaviour
    {
        public static WeaponManager Instance { get; private set; }
        
        [SerializeField] private EquipmentSystem equipmentSystem;
        
        // Cache current weapons for quick access
        private IWeaponItem _currentWeapon;
        
        // Default unarmed weapon (fallback)
        [SerializeField] private UnarmedWeaponItem defaultUnarmedWeapon;
        
        private void Awake()
        { 
            if (Instance != null && Instance != this) { Destroy(gameObject); return; } 
            Instance = this; 
            DontDestroyOnLoad(gameObject);
        }
        
        private void OnEnable()
        {
            EventBus.Subscribe<EquipmentUpdatedEvent>(OnEquipmentUpdated);
            EventBus.Subscribe<HotbarUpdatedEvent>(OnHotbarUpdated);
            EventBus.Subscribe<HotbarSelectionChangedEvent>(OnHotbarSelectionChanged);
        }
        
        private void OnDisable()
        {
            EventBus.Unsubscribe<EquipmentUpdatedEvent>(OnEquipmentUpdated);
            EventBus.Unsubscribe<HotbarUpdatedEvent>(OnHotbarUpdated);
            EventBus.Unsubscribe<HotbarSelectionChangedEvent>(OnHotbarSelectionChanged);
        }
        
        private void Start()
        {
            if (equipmentSystem == null) equipmentSystem = EquipmentSystem.Instance;
            RefreshWeapons();
        }
        
        private void OnEquipmentUpdated(EquipmentUpdatedEvent e)
        {
            RefreshWeapons();
        }
        
        private void OnHotbarUpdated(HotbarUpdatedEvent e)
        {
            // If the item in the selected slot changed, we need to refresh
            RefreshWeapons();
        }

        private void OnHotbarSelectionChanged(HotbarSelectionChangedEvent e)
        {
            RefreshWeapons();
        }
        
        private void RefreshWeapons()
        {
            IWeaponItem candidate = null;

            // 1. Check Hotbar Selection
            if (HotbarSystem.Instance != null)
            {
                var stack = HotbarSystem.Instance.Get(HotbarSystem.Instance.SelectedIndex);
                if (stack != null && stack.Item is IWeaponItem w)
                {
                    candidate = w;
                }
            }

            // 2. Check Equipment System (if no hotbar weapon selected)
            if (candidate == null && equipmentSystem != null)
            {
                // Check MainHand first
                var main = equipmentSystem.Get(EquipmentSlotType.MainHand);
                if (main != null && main.Item is IWeaponItem w1) candidate = w1;

                // Then Gun
                if (candidate == null)
                {
                    var gun = equipmentSystem.Get(EquipmentSlotType.Gun);
                    if (gun != null && gun.Item is IWeaponItem w2) candidate = w2;
                }

                // Then Melee
                if (candidate == null)
                {
                    var melee = equipmentSystem.Get(EquipmentSlotType.Melee);
                    if (melee != null && melee.Item is IWeaponItem w3) candidate = w3;
                }
            }

            // 3. Fallback
            _currentWeapon = candidate ?? defaultUnarmedWeapon;
        }
        
        public IWeaponItem GetCurrentWeapon()
        {
            return _currentWeapon;
        }
    }
}
