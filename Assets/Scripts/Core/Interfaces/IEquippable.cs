using UnityEngine;
using WF.Gameplay.Core.Data;

namespace WF.Gameplay.Core.Interfaces
{
    public interface IEquippable : IItem
    {
        EquipmentSlotType SlotType { get; }
        bool CanEquip(GameObject user);
        void OnEquip(GameObject user);
        void OnUnequip(GameObject user);
    }
}
