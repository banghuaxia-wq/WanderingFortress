using UnityEngine;
using WF.Gameplay.Core.Data;
using WF.Gameplay.Systems.InventorySystem;
using WF.Gameplay.Core.Events;

namespace WF.Gameplay.UI.Inventory
{
    public class HotbarPanelManager : MonoBehaviour
    {
        [SerializeField] private Transform content;
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private UISlotPoolManager slotPool;
        [SerializeField] private int initialSlotCount = 10; // 开局快捷栏槽位数量（中文注释）
        private const string SelectedIndicatorObjectName = "SelectedIndicator"; // 选中指示器对象名（中文注释）
        
        private void OnEnable() 
        { 
            EventBus.Subscribe<HotbarUpdatedEvent>(OnHotbarUpdated); 
            Refresh(); 
        }
        private void OnDisable() 
        { 
            EventBus.Unsubscribe<HotbarUpdatedEvent>(OnHotbarUpdated); 
        }
        
        private void OnHotbarUpdated(HotbarUpdatedEvent e) { Refresh(); }
        
        private void Refresh()
        {
            if (content == null) content = transform;
            if (slotPrefab == null || slotPool == null) return;
            var sys = HotbarSystem.Instance;
            int desiredCount = sys != null ? sys.Count : initialSlotCount;
            if (desiredCount < 0) desiredCount = 0;

            var slots = GetExistingSlots();
            EnsureSlotCount(slots, desiredCount);
            BindSlots(slots, sys);
        }

        private System.Collections.Generic.List<PackageUISlotController> GetExistingSlots() // 获取当前已有快捷栏槽位列表（中文注释）
        {
            var list = new System.Collections.Generic.List<PackageUISlotController>();
            if (content == null) return list;

            for (int i = 0; i < content.childCount; i++)
            {
                var child = content.GetChild(i);
                if (child == null) continue;
                if (child.name == SelectedIndicatorObjectName) continue;
                var slot = child.GetComponent<PackageUISlotController>();
                if (slot == null) continue;
                list.Add(slot);
            }

            return list;
        }

        private void EnsureSlotCount(System.Collections.Generic.List<PackageUISlotController> slots, int desiredCount) // 确保槽位数量达到目标值（中文注释）
        {
            if (slots == null) return;
            if (slotPrefab == null || slotPool == null || content == null) return;

            for (int i = slots.Count - 1; i >= desiredCount; i--)
            {
                var slot = slots[i];
                if (slot == null) { slots.RemoveAt(i); continue; }
                slotPool.Release(slot.gameObject);
                slots.RemoveAt(i);
            }

            for (int i = slots.Count; i < desiredCount; i++)
            {
                var slot = slotPool.Get(slotPrefab, content);
                if (slot == null) continue;
                slots.Add(slot);
            }
        }

        private void BindSlots(System.Collections.Generic.List<PackageUISlotController> slots, HotbarSystem sys) // 绑定快捷栏数据到槽位（中文注释）
        {
            if (slots == null) return;

            for (int i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                if (slot == null) continue;
                slot.SetSlotType(UISlotType.Hotbar);
                slot.SetMeta(TransferSource.Hotbar, null, i);
                slot.Bind(sys != null ? sys.Get(i) : null);
            }
        }
    }
}
