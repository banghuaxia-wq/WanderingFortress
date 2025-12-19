using WF.Gameplay.Core.Interfaces;

namespace WF.Gameplay.Core.Interfaces
{
    public interface IStackable
    {
        int CurrentStack { get; set; }
        int MaxStack { get; }
        bool CanStackWith(IItem other);
    }
}
