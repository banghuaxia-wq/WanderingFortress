using System.Collections.Generic;
using UnityEngine;
using WF.Gameplay.Core.Data;
using WF.Gameplay.Systems.EventSystem;

namespace WF.Gameplay.Systems.InventorySystem
{
    public class PlayerInventory : MonoBehaviour
    {
        public static PlayerInventory Instance { get; private set; }
        [SerializeField] private int baseCapacity = 20;
        private readonly List<ItemStack> _items = new List<ItemStack>();
        [SerializeField] private float overweightThreshold = 60f;
        [SerializeField] private float overloadedThreshold = 100f;
        private void Awake() { if (Instance != null && Instance != this) { Destroy(gameObject); return; } Instance = this; DontDestroyOnLoad(gameObject); }
        public int Capacity => baseCapacity;
        public IReadOnlyList<ItemStack> Items => _items;
        public bool Add(ItemStack item) { if (item == null) return false; if (_items.Count >= Capacity) return false; _items.Add(item); Notify(); return true; }
        public void RemoveAt(int index) { if (index < 0 || index >= _items.Count) return; _items.RemoveAt(index); Notify(); }
        public void Set(int index, ItemStack item) { if (index < 0 || index >= Capacity) return; while (_items.Count <= index) _items.Add(null); _items[index] = item; Notify(); }
        private void Notify() { GameEvents.RaisePlayerInventoryUpdated(); var w = GetTotalWeight(); bool ow = w >= overweightThreshold; bool ol = w >= overloadedThreshold; GameEvents.RaiseWeightChanged(w, ow, ol); }
        public float GetTotalWeight() { float w = 0f; for (int i = 0; i < _items.Count; i++) { var it = _items[i]; if (it != null) w += it.TotalWeight; } return w; }
    }
}
