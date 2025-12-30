using System.Collections.Generic;
using UnityEngine;
using WF.Gameplay.Core.Data;
using WF.Gameplay.Systems.InventorySystem;
using WF.Gameplay.Core.Events;

namespace WF.Gameplay.UI.Inventory
{
    public class EquipmentPanelManager : MonoBehaviour
    {
        [SerializeField] private Transform headSlot;
        [SerializeField] private Transform armorSlot;
        [SerializeField] private Transform gloveSlot;
        [SerializeField] private Transform pantSlot;
        [SerializeField] private Transform shoeSlot;
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private UISlotPoolManager slotPool;
        private readonly Dictionary<EquipmentSlotType, PackageUISlotController> _map = new Dictionary<EquipmentSlotType, PackageUISlotController>();
        
        private void OnEnable() 
        { 
            EventBus.Subscribe<EquipmentUpdatedEvent>(OnEquipmentUpdated); 
            InitSlots(); 
            Refresh(); 
        }
        private void OnDisable() 
        { 
            EventBus.Unsubscribe<EquipmentUpdatedEvent>(OnEquipmentUpdated); 
        }
        
        private void OnEquipmentUpdated(EquipmentUpdatedEvent e) { Refresh(); }
        
        private void InitSlots()
        {
            _map.Clear();
            SetupSlot(headSlot != null ? headSlot : ResolveSlotTransform("PackageUISlot_Head"), EquipmentSlotType.Head);
            SetupSlot(armorSlot != null ? armorSlot : ResolveSlotTransform("PackageUISlot_Armor"), EquipmentSlotType.Armor);
            SetupSlot(gloveSlot != null ? gloveSlot : ResolveSlotTransform("PackageUISlot_Glove"), EquipmentSlotType.Glove);
            SetupSlot(pantSlot != null ? pantSlot : ResolveSlotTransform("PackageUISlot_Pant"), EquipmentSlotType.Pant);
            SetupSlot(shoeSlot != null ? shoeSlot : ResolveSlotTransform("PackageUISlot_Shoe"), EquipmentSlotType.Shoe);
        }

        private Transform ResolveSlotTransform(string slotObjectName)
        {
            if (string.IsNullOrWhiteSpace(slotObjectName)) return null;

            var direct = transform.Find(slotObjectName);
            if (direct != null) return direct;

            var all = GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++)
            {
                var t = all[i];
                if (t != null && t.name == slotObjectName) return t;
            }

            return null;
        }
        
        private void SetupSlot(Transform parent, EquipmentSlotType type)
        {
            if (parent == null) return;
            var slotRoot = ResolveSlotRoot(parent);
            if (slotRoot == null) return;

            var ctrl = slotRoot.GetComponent<PackageUISlotController>();
            if (ctrl == null) ctrl = slotRoot.gameObject.AddComponent<PackageUISlotController>();
            CleanupExtraClones(parent, slotRoot);
            if (ctrl == null) return;
            _map[type] = ctrl;
            ctrl.SetSlotType(UISlotType.Equipment);
            ctrl.SetMeta(TransferSource.Equipment, null, (int)type);
        }

        private Transform ResolveSlotRoot(Transform slotContainer) // 解析装备槽位的真正根节点（中文注释）
        {
            if (slotContainer == null) return null;

            if (slotContainer.Find("Icon") != null || slotContainer.Find("Count") != null)
            {
                return slotContainer;
            }

            for (int i = 0; i < slotContainer.childCount; i++)
            {
                var child = slotContainer.GetChild(i);
                if (child == null) continue;
                if (child.Find("Icon") != null || child.Find("Count") != null)
                {
                    return child;
                }
            }

            return slotContainer;
        }

        private void CleanupExtraClones(Transform slotContainer, Transform slotRoot) // 清理误生成的Slot(Clone)（中文注释）
        {
            if (!Application.isPlaying) return;
            if (slotContainer == null || slotRoot == null) return;

            var controllers = slotContainer.GetComponentsInChildren<PackageUISlotController>(true);
            for (int i = 0; i < controllers.Length; i++)
            {
                var c = controllers[i];
                if (c == null) continue;
                if (c.transform == slotRoot) continue;
                if (!c.gameObject.name.Contains("(Clone)")) continue;
                Destroy(c.gameObject);
            }
        }
        
        private void Refresh()
        {
            var sys = EquipmentSystem.Instance; if (sys == null) return;
            foreach (var kv in _map)
            {
                if (kv.Value == null) continue;
                var s = sys.Get(kv.Key);
                kv.Value.Bind(s);
            }
        }
    }
}
