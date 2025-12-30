using System.IO;
using System.Collections.Generic;
using UnityEngine;
using WF.Gameplay.Core.Data;
using WF.Gameplay.Systems.ContainerSystem;
using WF.Gameplay.Systems.InventorySystem;
using WF.Gameplay.Core.Events;
using WF.Gameplay.Systems.Buffs;
using WF.Gameplay.Systems.Player;
using WF.Gameplay.Systems.Inventory;
using WF.Gameplay.Systems.Inventory.Items;

namespace WF.Gameplay.Systems.Developer
{
    public class DeveloperManager : MonoBehaviour
    {
        [System.Serializable]
        private class StartingItemEntry
        {
            public ItemBase itemAsset;
            public int count = 1;
        }

        private static bool _startingItemsGrantedThisSession;

        [Header("Targets")]
        [SerializeField] private GameObject targetContainerObject;
        [SerializeField] private PlayerStats playerStats;
        [SerializeField] private BuffData buffAsset;

        [Header("Hatch Spawn Settings")]
        [Tooltip("用于测试生成的Hatch数据（SOHatch）")]
        [SerializeField] private SOHatch devHatchData;
        [Tooltip("生成位置（为空则使用场景原点）")]
        [SerializeField] private Transform devSpawnPoint;

        [Header("Inventory Item Settings")]
        [SerializeField] private ItemBase invItemAsset;
        [SerializeField] private string invItemId = "DevItem"; // Fallback ID
        [SerializeField] private int invItemCount = 1;

        [Header("Starting Inventory")]
        [SerializeField] private bool grantStartingItemsOnStart = true;
        [SerializeField] private bool grantStartingItemsOnlyOnce = true;
        [SerializeField] private List<StartingItemEntry> startingItems = new List<StartingItemEntry>();

        [Header("Container Item Settings")]
        [SerializeField] private ItemBase boxItemAsset;
        [SerializeField] private string boxItemId = "DevItem"; // Fallback ID
        [SerializeField] private int boxItemCount = 1;

        [Header("Save Settings")]
        [SerializeField] private string saveFileName = "save.json";

        private void Start()
        {
            if (grantStartingItemsOnStart)
            {
                GrantStartingItems();
            }
        }

        [ContextMenu("Grant Starting Items")]
        public void GrantStartingItems()
        {
            if (grantStartingItemsOnlyOnce && _startingItemsGrantedThisSession) return;

            var inv = PlayerInventory.Instance;
            if (inv == null)
            {
                Debug.LogWarning("PlayerInventory not found.");
                return;
            }

            if (startingItems != null)
            {
                for (int i = 0; i < startingItems.Count; i++)
                {
                    var entry = startingItems[i];
                    if (entry == null || entry.itemAsset == null) continue;
                    int count = Mathf.Max(1, entry.count);
                    var item = ItemFactory.CreateItemStack(entry.itemAsset, count);
                    if (item == null) continue;
                    inv.Add(item);
                }
            }

            _startingItemsGrantedThisSession = true;
        }

        [ContextMenu("Add Item To Container")]
        public void AddItemToContainer()
        {
            var mgr = ContainerManager.Instance; if (mgr == null) { Debug.LogWarning("ContainerManager not found."); return; }
            
            ItemStack item = null;
            if (boxItemAsset != null)
            {
                item = ItemFactory.CreateItemStack(boxItemAsset, boxItemCount);
            }
            else
            {
                item = ItemFactory.CreateItemStack(boxItemId, boxItemCount);
            }
            
            if (item == null) { Debug.LogWarning("Failed to create item."); return; }

            var actor = targetContainerObject != null ? targetContainerObject.GetComponent<WF.Gameplay.Systems.ContainerSystem.ContainerActor>() : null;
            var id = actor != null ? actor.ContainerId : (targetContainerObject != null ? targetContainerObject.name : null);
            if (string.IsNullOrEmpty(id)) { Debug.LogWarning("Target container object not set."); return; }
            mgr.AddItem(id, item);
        }

        [ContextMenu("Add Item To Inventory")]
        public void AddItemToInventory()
        {
            var inv = PlayerInventory.Instance; if (inv == null) { Debug.LogWarning("PlayerInventory not found."); return; }
            
            ItemStack item = null;
            if (invItemAsset != null)
            {
                item = ItemFactory.CreateItemStack(invItemAsset, invItemCount);
            }
            else
            {
                item = ItemFactory.CreateItemStack(invItemId, invItemCount);
            }
            
            if (item == null) { Debug.LogWarning("Failed to create item."); return; }
            
            inv.Add(item);
        }

        [ContextMenu("Adjust Player Values (Stamina=Full)")]
        public void AdjustPlayerValues()
        {
            if (playerStats == null) playerStats = FindObjectOfType<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.stamina = playerStats.maxStamina;
            }
        }

        [ContextMenu("Apply Buff To Player")]
        public void ApplyBuffToPlayer()
        {
            if (buffAsset == null) { Debug.LogWarning("BuffData not set."); return; }
            var player = FindObjectOfType<PlayerStateManager>(); if (player == null) { Debug.LogWarning("PlayerStateManager not found."); return; }
            var mgr = player.GetComponent<BuffManager>(); if (mgr == null) mgr = player.gameObject.AddComponent<BuffManager>();
            mgr.AddBuff(buffAsset, gameObject);
        }

        [System.Serializable]
        private class SaveData
        {
            public List<ItemStack> InventoryItems = new List<ItemStack>();
            public List<ContainerData> Containers = new List<ContainerData>();
        }

        [ContextMenu("Save Game")]
        public void SaveGame()
        {
            var data = new SaveData();
            var inv = PlayerInventory.Instance; if (inv != null) data.InventoryItems.AddRange(inv.Items);
            var mgr = ContainerManager.Instance; if (mgr != null)
            {
                var actor = targetContainerObject != null ? targetContainerObject.GetComponent<WF.Gameplay.Systems.ContainerSystem.ContainerActor>() : null;
                var id = actor != null ? actor.ContainerId : (targetContainerObject != null ? targetContainerObject.name : null);
                if (!string.IsNullOrEmpty(id)) { var c = mgr.Get(id); if (c != null) data.Containers.Add(c); }
            }
            var json = JsonUtility.ToJson(data, true);
            var path = Path.Combine(Application.persistentDataPath, saveFileName);
            File.WriteAllText(path, json);
            Debug.Log($"Saved to {path}");
        }

        [ContextMenu("Load Game")]
        public void LoadGame()
        {
            var path = Path.Combine(Application.persistentDataPath, saveFileName);
            if (!File.Exists(path)) { Debug.LogWarning("Save file not found."); return; }
            var json = File.ReadAllText(path);
            var data = JsonUtility.FromJson<SaveData>(json);
            var inv = PlayerInventory.Instance; if (inv != null)
            {
                // 清空并重建（简化）
                for (int i = inv.Items.Count - 1; i >= 0; i--) inv.RemoveAt(i);
                if (data.InventoryItems != null) 
                {
                    foreach (var it in data.InventoryItems) 
                    {
                        // Re-link IItem reference after deserialization
                        if (it.Item == null && !string.IsNullOrEmpty(it.ItemId))
                        {
                            var resolved = ItemFactory.CreateItemStack(it.ItemId, it.Count);
                            if (resolved != null) it.Item = resolved.Item;
                        }
                        inv.Add(it);
                    }
                }
            }
            var mgr = ContainerManager.Instance; if (mgr != null && data.Containers != null)
            {
                foreach (var c in data.Containers)
                {
                    // Re-link IItem references
                    foreach (var it in c.Items)
                    {
                        if (it.Item == null && !string.IsNullOrEmpty(it.ItemId))
                        {
                            var resolved = ItemFactory.CreateItemStack(it.ItemId, it.Count);
                            if (resolved != null) it.Item = resolved.Item;
                        }
                    }
                    mgr.Register(c);
                    EventBus.Publish(new ContainerUpdatedEvent(c));
                }
            }
            Debug.Log("Loaded save.");
        }

        [ContextMenu("Spawn Hatch For Test")]
        public void SpawnHatchForTest()
        {
            var pos = devSpawnPoint != null ? devSpawnPoint.position : Vector3.zero;
            var rot = devSpawnPoint != null ? devSpawnPoint.rotation : Quaternion.identity;
            var go = WF.Gameplay.Systems.Hatch.HatchService.EnsureInstance().SpawnFromData(devHatchData, pos, rot);
            if (go == null)
            {
                Debug.LogWarning("SpawnHatchForTest 失败：未设置 Hatch 数据 或 服务不可用。");
            }
        }
    }
}

