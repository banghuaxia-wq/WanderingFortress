using UnityEngine;
using WF.Gameplay.Core.Data;

namespace WF.Gameplay.Core.Interfaces
{
    public interface IItem
    {
        string ItemId { get; }
        string DisplayName { get; }
        ItemType Type { get; }
        ItemTag Tags { get; }
        Sprite Icon { get; }
        float Weight { get; }
        string Description { get; }
        bool CanUse(UnityEngine.GameObject user);
        void Use(UnityEngine.GameObject user);
    }
}
