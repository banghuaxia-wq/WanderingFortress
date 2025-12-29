using System.Collections.Generic;
using UnityEngine;
using WF.Gameplay.Core.Data;
using WF.Gameplay.Core.Events;
using WF.Gameplay.Core.Interfaces;

namespace WF.Gameplay.Systems.InventorySystem
{
    public class HotbarSystem : MonoBehaviour
    {
        public static HotbarSystem Instance { get; private set; }
        [SerializeField] private int slotCount = 10;
        private readonly List<ItemStack> _slots = new List<ItemStack>();
        
        public int SelectedIndex { get; private set; } = 0;

        private void Awake() 
        { 
            if (Instance != null && Instance != this) { Destroy(gameObject); return; } 
            Instance = this; 
            DontDestroyOnLoad(gameObject); 
            for (int i = 0; i < slotCount; i++) _slots.Add(null); 
        }
        
        public int Count => _slots.Count;
        
        public ItemStack Get(int i) { if (i < 0 || i >= _slots.Count) return null; return _slots[i]; }
        
        public void SelectSlot(int index)
        {
            if (index < 0 || index >= _slots.Count) return;
            if (SelectedIndex != index)
            {
                SelectedIndex = index;
                EventBus.Publish(new HotbarSelectionChangedEvent(SelectedIndex));
            }
        }

        public bool Set(int i, ItemStack item)  
        { 
            if (i < 0 || i >= _slots.Count) return false; 
            
            // Validate using IItem interface if available
            if (item != null)
            {
                if (item.Item != null)
                {
                    var tags = item.Item.Tags;
                    bool allowedByType =
                        item.Type == ItemType.Weapon ||
                        item.Type == ItemType.Tool ||
                        item.Type == ItemType.Consumable ||
                        item.Type == ItemType.Throwable ||
                        item.Type == ItemType.Food ||
                        item.Type == ItemType.Building ||
                        item.Type == ItemType.Facility;
                    bool allowedByTag = (tags & (ItemTag.Weapon | ItemTag.Tool | ItemTag.Consumable | ItemTag.Throwable | ItemTag.Food | ItemTag.Building | ItemTag.Facility)) != 0;
                    bool allowedByInterface = item.Item is IConsumable;

                    if (!allowedByType && !allowedByTag && !allowedByInterface)
                    {
                        return false;
                    }
                }
                else
                {
                    // Legacy fallback
                    if (!item.IsUsable &&
                        item.Type != ItemType.Weapon &&
                        item.Type != ItemType.Tool &&
                        item.Type != ItemType.Consumable &&
                        item.Type != ItemType.Throwable &&
                        item.Type != ItemType.Food &&
                        item.Type != ItemType.Building &&
                        item.Type != ItemType.Facility) return false;
                }
            }
            
            _slots[i] = item; 
            EventBus.Publish(new HotbarUpdatedEvent()); 
            return true; 
        }
        
        public void UseItem(int index, GameObject user)
        {
            var stack = Get(index);
            if (stack != null && stack.Item != null && stack.Item.CanUse(user))
            {
                stack.Item.Use(user);
                
                // Handle consumption if applicable
                if (stack.Item is IConsumable consumable)
                {
                    consumable.OnConsume(user);
                    // Reduce count logic would go here or be handled by inventory update
                }
            }
        }
        
        public IReadOnlyList<ItemStack> Slots => _slots;
    }
}
