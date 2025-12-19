using UnityEngine;
using WF.Gameplay.Core.Data;
using WF.Gameplay.Core.Interfaces;
using WF.Gameplay.Systems.Inventory.Items;
using WF.Gameplay.Systems.Inventory.Items.Weapons;
using WeaponItemSO = WF.Gameplay.Systems.Inventory.Items.Weapons.WeaponItem;

namespace WF.Gameplay.Systems.Inventory
{
    public class ItemFactory : MonoBehaviour
    {
        public static ItemFactory Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        /// <summary>
        /// 使用物品资产创建 ItemStack（中文注释）
        /// </summary>
        public static ItemStack CreateItemStack(ItemBase item, int count)
        {
            if (item == null) return null;
            return ItemStack.Create(item, count);
        }

        /// <summary>
        /// 通过物品ID从 Resources 加载并创建 ItemStack（中文注释）
        /// </summary>
        public static ItemStack CreateItemStack(string itemId, int count)
        {
            if (string.IsNullOrEmpty(itemId)) return null;

            // Try to load from Resources using common paths
            // 1. Items root
            var item = Resources.Load<ItemBase>($"Items/{itemId}");
            
            // 2. Items/Weapons
            if (item == null) item = Resources.Load<ItemBase>($"Items/Weapons/{itemId}");
            if (item == null) item = Resources.Load<ItemBase>($"Items/Weapons/Ranged/{itemId}");
            if (item == null) item = Resources.Load<ItemBase>($"Items/Weapons/Melee/{itemId}");
            if (item == null) item = Resources.Load<ItemBase>($"Items/Weapons/Unarmed/{itemId}");
            
            // 3. Items/Consumables
            if (item == null) item = Resources.Load<ItemBase>($"Items/Consumables/{itemId}");
            
            // 4. Items/Equipment
            if (item == null) item = Resources.Load<ItemBase>($"Items/Equipment/{itemId}");

            if (item != null)
            {
                return ItemStack.Create(item, count);
            }

            return null;
        }

        public IWeaponItem CreateWeapon(WeaponItemSO template)
        {
            if (template == null) return null;
            
            // Instantiate the ScriptableObject to create a runtime copy
            // This ensures ammo and other state is unique to this instance
            var instance = Instantiate(template);
            instance.name = template.name;
            
            return instance;
        }

        public IWeaponItem CreateWeapon(string resourcePath)
        {
            var template = Resources.Load<WeaponItemSO>(resourcePath);
            if (template == null)
            {
                Debug.LogError($"Weapon template not found at {resourcePath}");
                return null;
            }
            return CreateWeapon(template);
        }
    }
}
