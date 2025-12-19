using UnityEngine;

namespace WF.Gameplay.Core.Interfaces
{
    public interface IConsumable : IItem
    {
        int MaxStack { get; }
        float Cooldown { get; }
        bool CanConsume(GameObject user);
        void OnConsume(GameObject user);
    }
}
