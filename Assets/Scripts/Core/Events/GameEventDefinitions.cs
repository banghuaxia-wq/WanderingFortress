using WF.Gameplay.Core.Data;
using WF.Gameplay.Core.Interfaces;

namespace WF.Gameplay.Core.Events
{
    // 容器相关事件
    public struct ContainerOpenedEvent 
    { 
        public ContainerData Data; 
        public ContainerOpenedEvent(ContainerData data) { Data = data; } 
    }
    
    public struct ContainerUpdatedEvent 
    { 
        public ContainerData Data; 
        public ContainerUpdatedEvent(ContainerData data) { Data = data; } 
    }
    
    public struct ContainerClosedEvent 
    { 
        public ContainerData Data; 
        public ContainerClosedEvent(ContainerData data) { Data = data; } 
    }
    
    // 库存/装备/快捷栏相关事件
    public struct PlayerInventoryUpdatedEvent { }
    public struct EquipmentUpdatedEvent { }
    public struct HotbarUpdatedEvent { }
    public struct HotbarSelectionChangedEvent 
    { 
        public int SelectedIndex; 
        public HotbarSelectionChangedEvent(int index) { SelectedIndex = index; } 
    }
    
    // 重量相关事件
    public struct WeightChangedEvent 
    { 
        public float Weight; 
        public bool IsOverweight; 
        public bool IsOverloaded; 
        public WeightChangedEvent(float w, bool o1, bool o2) { Weight = w; IsOverweight = o1; IsOverloaded = o2; }
    }

    // 物品相关事件
    public struct ItemUsedEvent
    {
        public IItem Item;
        public UnityEngine.GameObject User;
        public ItemUsedEvent(IItem item, UnityEngine.GameObject user) { Item = item; User = user; }
    }

    public struct ItemEquippedEvent
    {
        public IEquippable Item;
        public UnityEngine.GameObject User;
        public EquipmentSlotType Slot;
        public ItemEquippedEvent(IEquippable item, UnityEngine.GameObject user, EquipmentSlotType slot) { Item = item; User = user; Slot = slot; }
    }

    public struct ItemUnequippedEvent
    {
        public IEquippable Item;
        public UnityEngine.GameObject User;
        public EquipmentSlotType Slot;
        public ItemUnequippedEvent(IEquippable item, UnityEngine.GameObject user, EquipmentSlotType slot) { Item = item; User = user; Slot = slot; }
    }

    public struct ItemConsumedEvent
    {
        public IConsumable Item;
        public UnityEngine.GameObject User;
        public int Amount;
        public ItemConsumedEvent(IConsumable item, UnityEngine.GameObject user, int amount) { Item = item; User = user; Amount = amount; }
    }

    // Player Stats Events
    public struct StaminaChangedEvent
    {
        public float Current;
        public float Max;
        public bool IsExhausted;
        public StaminaChangedEvent(float current, float max, bool isExhausted) { Current = current; Max = max; IsExhausted = isExhausted; }
    }

    public struct CrosshairOffsetEvent
    {
        public UnityEngine.Vector2 Offset;
        public CrosshairOffsetEvent(UnityEngine.Vector2 offset) { Offset = offset; }
    }

    public struct PochieSpawnedEvent
    {
        public UnityEngine.GameObject Instance;
        public PochieSpawnedEvent(UnityEngine.GameObject instance) { Instance = instance; }
    }

    public struct InputStateChangedEvent
    {
        public bool InputEnabled;
        public InputStateChangedEvent(bool enabled) { InputEnabled = enabled; }
    }
}
