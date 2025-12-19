using WF.Gameplay.Core.Data;

namespace WF.Gameplay.Core.Interfaces
{
    public interface IPlayerInventory
    {
        int Capacity { get; }
        float TotalWeight { get; }
        System.Collections.Generic.List<ItemStack> Items { get; }
        void Add(ItemStack item);
        void RemoveAt(int index);
        void RemoveItem(string itemId, int count);
        void UseEmptySoulOrb();
        int GetItemCount(string itemId);
    }
}
