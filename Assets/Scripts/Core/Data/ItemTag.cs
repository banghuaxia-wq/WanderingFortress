using System;

namespace WF.Gameplay.Core.Data
{
    [Flags]
    public enum ItemTag
    {
        None = 0,
        Consumable = 1 << 0,
        Tool = 1 << 1,
        Weapon = 1 << 2,
        Armor = 1 << 3,
        Material = 1 << 4,
        Quest = 1 << 5,
        Ammo = 1 << 6,
        Throwable = 1 << 7,
        Fuel = 1 << 8
    }
}

