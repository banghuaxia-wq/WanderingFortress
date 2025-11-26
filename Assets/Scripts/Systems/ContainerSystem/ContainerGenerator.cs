using UnityEngine;
using WF.Gameplay.Core.Data;

namespace WF.Gameplay.Systems.ContainerSystem
{
    public static class ContainerGenerator
    {
        public static ContainerData Create(string id, ContainerType type, int slotLimit) { return new ContainerData { Id = id, Type = type, SlotLimit = slotLimit }; }
        public static ItemStack CreateItem(string itemId, ItemType type, int count, int maxStack, float weightPerUnit) { return new ItemStack { ItemId = itemId, Type = type, Count = count, MaxStack = maxStack, WeightPerUnit = weightPerUnit }; }
    }
}
