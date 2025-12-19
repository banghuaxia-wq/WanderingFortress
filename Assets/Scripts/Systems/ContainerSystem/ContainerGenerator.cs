using WF.Gameplay.Systems.Inventory;
using WF.Gameplay.Core.Data;

namespace WF.Gameplay.Systems.ContainerSystem
{
    public static class ContainerGenerator
    {
        public static ContainerData Create(string id, ContainerType type, int slotLimit) { return new ContainerData { Id = id, Type = type, SlotLimit = slotLimit }; }
        public static ItemStack CreateItem(string itemId, ItemType type, int count, int maxStack, float weightPerUnit) 
        { 
            // Attempt to create using factory first
            var stack = ItemFactory.CreateItemStack(itemId, count);
            if (stack != null) return stack;

            // Fallback: Return a raw stack with ID (properties will be null/default until Item is assigned)
            return new ItemStack 
            { 
                Id = itemId, 
                Count = count 
            }; 
        }
    }
}
