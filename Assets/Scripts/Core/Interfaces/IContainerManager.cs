using WF.Gameplay.Core.Data;

namespace WF.Gameplay.Core.Interfaces
{
    public interface IContainerManager
    {
        void Register(ContainerData data);
        ContainerData Get(string id);
        void Open(string id);
        void Close(string id);
        void AddItem(string id, ItemStack item);
        void RemoveItem(string id, int index);
        ContainerData CurrentOpened { get; }
    }
}
