using UnityEngine;
using WF.Gameplay.Core.Data;
using WF.Gameplay.Core.Events;
using WF.Gameplay.Systems.Inventory;
using WF.Gameplay.Systems.Inventory.Items;
using WF.Gameplay.Systems.InventorySystem;

namespace WF.Tests.Integration
{
    public class ItemSystemIntegrationTest : MonoBehaviour
    {
        [Header("Test Assets")]
        public ItemBase TestGunItem;
        public ItemBase TestAmmoItem;
        
        [Header("Runtime Status")]
        public bool GunEquipped;
        public int AmmoCount;
        public float InventoryWeight;

        private void Start()
        {
            Debug.Log("Starting Item System Integration Test...");
            
            // 1. Test Factory and Stack Creation
            var gunStack = ItemFactory.CreateItemStack(TestGunItem, 1);
            var ammoStack = ItemFactory.CreateItemStack(TestAmmoItem, 50);
            
            if (gunStack != null && gunStack.Item != null)
                Debug.Log($"Created Gun: {gunStack.Item.DisplayName} (ID: {gunStack.ItemId})");
            else
                Debug.LogError("Failed to create Gun ItemStack");

            if (ammoStack != null && ammoStack.Item != null)
                Debug.Log($"Created Ammo: {ammoStack.Item.DisplayName} x{ammoStack.Count}");
            else
                Debug.LogError("Failed to create Ammo ItemStack");

            // 2. Test Inventory Addition
            var inv = PlayerInventory.Instance;
            if (inv != null)
            {
                inv.Add(gunStack);
                inv.Add(ammoStack);
                InventoryWeight = inv.TotalWeight;
                Debug.Log($"Inventory Updated. Total Weight: {InventoryWeight}");
            }
            else
            {
                Debug.LogError("PlayerInventory instance not found.");
            }

            // 3. Test Equipment
            var equipSys = EquipmentSystem.Instance;
            if (equipSys != null && gunStack != null)
            {
                bool equipped = equipSys.Equip(EquipmentSlotType.Gun, gunStack);
                if (equipped)
                {
                    Debug.Log("Gun Equipped Successfully.");
                    GunEquipped = true;
                }
                else
                {
                    Debug.LogError("Failed to equip gun.");
                }
            }

            // 4. Verify Event Bus
            EventBus.Subscribe<PlayerInventoryUpdatedEvent>(e => Debug.Log("Event Received: PlayerInventoryUpdated"));
            EventBus.Subscribe<EquipmentUpdatedEvent>(e => Debug.Log("Event Received: EquipmentUpdated"));
        }

        private void Update()
        {
            // Real-time monitoring
            if (PlayerInventory.Instance != null && TestAmmoItem != null)
            {
                AmmoCount = PlayerInventory.Instance.GetItemCount(TestAmmoItem.ItemId);
            }
        }
    }
}
