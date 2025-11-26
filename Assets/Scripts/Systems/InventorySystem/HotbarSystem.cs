using System.Collections.Generic;
using UnityEngine;
using WF.Gameplay.Core.Data;
using WF.Gameplay.Systems.EventSystem;

namespace WF.Gameplay.Systems.InventorySystem
{
    public class HotbarSystem : MonoBehaviour
    {
        public static HotbarSystem Instance { get; private set; }
        [SerializeField] private int slotCount = 10;
        private readonly List<ItemStack> _slots = new List<ItemStack>();
        private void Awake() { if (Instance != null && Instance != this) { Destroy(gameObject); return; } Instance = this; DontDestroyOnLoad(gameObject); for (int i = 0; i < slotCount; i++) _slots.Add(null); }
        public int Count => _slots.Count;
        public ItemStack Get(int i) { if (i < 0 || i >= _slots.Count) return null; return _slots[i]; }
        public bool Set(int i, ItemStack item) { if (i < 0 || i >= _slots.Count) return false; if (item != null && !item.IsUsable && item.Type != ItemType.Weapon && item.Type != ItemType.Consumable) return false; _slots[i] = item; GameEvents.RaiseHotbarUpdated(); return true; }
        public IReadOnlyList<ItemStack> Slots => _slots;
    }
}
