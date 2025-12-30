using System.Collections.Generic;

namespace WF.Gameplay.Core.Data
{
    public class ContainerData
    {
        public string Id;
        public ContainerType Type;
        public int SlotLimit;
        public List<ItemStack> Items = new List<ItemStack>();
        public bool HasGeneratedLoot;
    }
}
