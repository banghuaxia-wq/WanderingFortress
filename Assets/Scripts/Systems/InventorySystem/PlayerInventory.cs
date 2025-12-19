using System.Collections.Generic;
using UnityEngine;
using WF.Gameplay.Core.Data;
using WF.Gameplay.Core.Events;
using WF.Gameplay.Core.Interfaces;
using WF.Gameplay.Systems.Inventory;

namespace WF.Gameplay.Systems.InventorySystem
{
    public class PlayerInventory : MonoBehaviour, IPlayerInventory
    {
        public static PlayerInventory Instance { get; private set; }
        
        [SerializeField] private int baseCapacity = 20;
        private readonly List<ItemStack> _items = new List<ItemStack>();
        
        [SerializeField] private float overweightThreshold = 60f;
        [SerializeField] private float overloadedThreshold = 100f;
        
        private void Awake() 
        { 
            if (Instance != null && Instance != this) 
            { 
                Destroy(gameObject); 
                return; 
            } 
            Instance = this; 
            DontDestroyOnLoad(gameObject); 
        }
        
        public int Capacity => baseCapacity;
        public float TotalWeight => GetTotalWeight();
        public List<ItemStack> Items => _items;
        
        public void Add(ItemStack item) 
        { 
            if (item == null) return;
            if (item.Item == null)
            {
                // Try to resolve IItem from ItemId using ItemFactory
                var resolved = ItemFactory.CreateItemStack(item.ItemId, item.Count);
                if (resolved != null && resolved.Item != null)
                {
                    item = resolved;
                }
            }
            if (_items.Count >= Capacity) return; 
            _items.Add(item); 
            Notify(); 
        }
        
        public void Add(IItem item, int count = 1)
        {
            if (item == null) return;
            var stack = ItemStack.Create(item, count);
            Add(stack);
        }
        
        public void RemoveAt(int index) 
        { 
            if (index < 0 || index >= _items.Count) return; 
            _items.RemoveAt(index); 
            Notify(); 
        }
        
        public void RemoveItem(string itemId, int count)
        {
            for (int i = _items.Count - 1; i >= 0; i--)
            {
                if (_items[i].ItemId == itemId)
                {
                    if (_items[i].Count > count)
                    {
                        _items[i].Count -= count;
                        count = 0;
                    }
                    else
                    {
                        count -= _items[i].Count;
                        _items.RemoveAt(i);
                    }
                    if (count <= 0) break;
                }
            }
            Notify();
        }
        
        public void UseEmptySoulOrb()
        {
            // Placeholder logic
            RemoveItem("EmptySoulOrb", 1);
        }
        
        public int GetItemCount(string itemId)
        {
            int count = 0;
            foreach (var item in _items)
            {
                if (item.ItemId == itemId) count += item.Count;
            }
            return count;
        }
        
        private void Notify() 
        { 
            EventBus.Publish(new PlayerInventoryUpdatedEvent()); 
            var w = GetTotalWeight(); 
            bool ow = w >= overweightThreshold; 
            bool ol = w >= overloadedThreshold; 
            EventBus.Publish(new WeightChangedEvent(w, ow, ol));
        }
        
        public float GetTotalWeight() 
        { 
            float w = 0f; 
            for (int i = 0; i < _items.Count; i++) 
            { 
                var it = _items[i]; 
                if (it != null) w += it.TotalWeight; 
            } 
            return w; 
        }
    }
}
