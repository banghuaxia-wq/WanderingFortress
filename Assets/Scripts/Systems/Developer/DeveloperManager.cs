using System.IO;
using System.Collections.Generic;
using UnityEngine;
using WF.Gameplay.Core.Data;
using WF.Gameplay.Systems.ContainerSystem;
using WF.Gameplay.Systems.InventorySystem;
using WF.Gameplay.Systems.EventSystem;
using WF.Gameplay.Systems.Buffs;
using WF.Gameplay.Systems.Player;

namespace WF.Gameplay.Systems.Developer
{
    public class DeveloperManager : MonoBehaviour
    {
        [Header("Targets")]
        [SerializeField] private GameObject targetContainerObject;
        [SerializeField] private PlayerStats playerStats;
        [SerializeField] private BuffData buffAsset;

        [Header("Inventory Item Settings")]
        [SerializeField] private string invItemId = "DevItem"; // 添加到背包的物品ID
        [SerializeField] private ItemType invItemType = ItemType.Tool; // 添加到背包的物品类型
        [SerializeField] private int invItemCount = 1; // 添加到背包的数量
        [SerializeField] private int invItemMaxStack = 10; // 背包物品最大叠堆
        [SerializeField] private float invItemWeightPerUnit = 0.2f; // 背包物品单位重量
        [SerializeField] private bool invItemUsable = false; // 背包物品是否可使用

        [Header("Container Item Settings")]
        [SerializeField] private string boxItemId = "DevItem"; // 添加到容器的物品ID
        [SerializeField] private ItemType boxItemType = ItemType.Tool; // 添加到容器的物品类型
        [SerializeField] private int boxItemCount = 1; // 添加到容器的数量
        [SerializeField] private int boxItemMaxStack = 10; // 容器物品最大叠堆
        [SerializeField] private float boxItemWeightPerUnit = 0.2f; // 容器物品单位重量

        [Header("Save Settings")]
        [SerializeField] private string saveFileName = "save.json";

        [ContextMenu("Add Item To Container")]
        public void AddItemToContainer()
        {
            var mgr = ContainerManager.Instance; if (mgr == null) { Debug.LogWarning("ContainerManager not found."); return; }
            var item = ContainerGenerator.CreateItem(boxItemId, boxItemType, boxItemCount, boxItemMaxStack, boxItemWeightPerUnit);
            var actor = targetContainerObject != null ? targetContainerObject.GetComponent<WF.Gameplay.Systems.ContainerSystem.ContainerActor>() : null;
            var id = actor != null ? actor.ContainerId : (targetContainerObject != null ? targetContainerObject.name : null);
            if (string.IsNullOrEmpty(id)) { Debug.LogWarning("Target container object not set."); return; }
            mgr.AddItem(id, item);
        }

        [ContextMenu("Add Item To Inventory")]
        public void AddItemToInventory()
        {
            var inv = PlayerInventory.Instance; if (inv == null) { Debug.LogWarning("PlayerInventory not found."); return; }
            var item = ContainerGenerator.CreateItem(invItemId, invItemType, invItemCount, invItemMaxStack, invItemWeightPerUnit);
            item.IsUsable = invItemUsable;
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
                if (data.InventoryItems != null) foreach (var it in data.InventoryItems) inv.Add(it);
            }
            var mgr = ContainerManager.Instance; if (mgr != null && data.Containers != null)
            {
                foreach (var c in data.Containers)
                {
                    mgr.Register(c);
                    GameEvents.RaiseContainerUpdated(c);
                }
            }
            Debug.Log("Loaded save.");
        }
    }
}
