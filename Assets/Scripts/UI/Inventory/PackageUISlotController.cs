using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using WF.Gameplay.Core.Data;
using WF.Gameplay.Core.Interfaces;
using WF.Gameplay.Systems.Inventory.Items;
using WF.Gameplay.Systems.InventorySystem;

namespace WF.Gameplay.UI.Inventory
{
    public class PackageUISlotController : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler, IPointerClickHandler
    {
        [SerializeField] private Image icon; // 图标（中文注释）
        [SerializeField] private Image iconBackground;
        [SerializeField] private TMP_Text countText; // 数量文本（中文注释）
        [SerializeField] private TMP_Text numberText;
        [SerializeField] private GameObject numberBackground;
        [SerializeField] private GameObject selectedBackground; // 选中背景（中文注释）
        [SerializeField] private UISlotType slotType;
        [SerializeField] private TransferSource sourceType; // 槽位来源类型（中文注释）
        [SerializeField] private string containerId; // 容器ID（中文注释）
        [SerializeField] private int index; // 槽位索引（中文注释）
        [SerializeField] private Canvas dragCanvas; // 拖拽时临时父Canvas（中文注释）
        private Transform _originalParent; // 拖拽前的父节点（中文注释）
        private int _originalSibling; // 拖拽前的兄弟序号（中文注释）
        private RectTransform _rect; // 本对象RectTransform缓存（中文注释）
        private IItem _boundItem;
        private int _boundCount;

        private void Awake() // 初始化引用并默认隐藏选中态（中文注释）
        {
            if (selectedBackground == null) selectedBackground = transform.Find("SelectedBackground")?.gameObject;
            if (iconBackground == null) iconBackground = transform.Find("IconBackground")?.GetComponent<Image>();
            ResolveOptionalReferences();
            ApplySlotTypeVisuals();
            SetSelected(false);
        }

        private void OnEnable() // 对象池复用时重置选中态（中文注释）
        {
            ApplySlotTypeVisuals();
            SetSelected(false);
        }

        public void Bind(ItemStack stack)
        {
            if (icon == null) icon = transform.Find("Icon")?.GetComponent<Image>();
            if (iconBackground == null) iconBackground = transform.Find("IconBackground")?.GetComponent<Image>();
            if (countText == null) countText = transform.Find("Count")?.GetComponent<TMP_Text>();
            if (selectedBackground == null) selectedBackground = transform.Find("SelectedBackground")?.gameObject;
            ResolveOptionalReferences();
            SetSelected(false);
            
            if (stack == null || stack.Item == null)
            {
                if (icon != null) icon.enabled = false;
                if (countText != null)
                {
                    countText.text = "";
                    countText.gameObject.SetActive(false);
                }
                ApplyRarityVisuals(null);
                _boundItem = null;
                _boundCount = 0;
                return;
            }
            
            _boundItem = stack.Item;
            _boundCount = stack.Count;
            if (icon != null) 
            { 
                icon.enabled = true; 
                icon.sprite = stack.Icon; 
            }
            ApplyCountVisuals(stack.Item, stack.Count);
            ApplyRarityVisuals(stack.Item);
        }
        
        // Overload to support direct IItem binding if needed
        public void Bind(IItem item, int count)
        {
            if (item == null) 
            {
                Bind(null);
                return;
            }
            // Create temp stack for display
            Bind(ItemStack.Create(item, count));
        }

        public void SetMeta(TransferSource src, string cid, int idx)
        {
            sourceType = src; containerId = cid; index = idx;
            UpdateHotbarNumber();
        }

        public void SetSlotType(UISlotType type)
        {
            slotType = type;
            ApplySlotTypeVisuals();
            UpdateHotbarNumber();
            ApplyCountVisuals(_boundItem, _boundCount);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_boundItem == null) return;

            // 可选：禁用自身射线以便投递到目标
            var cg = GetComponent<CanvasGroup>();
            if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = false;

            if (_rect == null) _rect = GetComponent<RectTransform>();
            if (dragCanvas == null) dragCanvas = GetComponentInParent<Canvas>();
            _originalParent = _rect.parent;
            _originalSibling = _rect.GetSiblingIndex();
            if (dragCanvas != null)
            {
                _rect.SetParent(dragCanvas.transform, true);
                _rect.SetAsLastSibling();
            }
            UpdateDragPosition(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            var cg = GetComponent<CanvasGroup>();
            if (cg != null) cg.blocksRaycasts = true;
            if (_rect != null && _originalParent != null)
            {
                _rect.SetParent(_originalParent, true);
                _rect.SetSiblingIndex(_originalSibling);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            UpdateDragPosition(eventData);
        }

        public void OnPointerClick(PointerEventData eventData) // 点击槽位时显示选中背景（中文注释）
        {
            DeselectSiblings();
            SetSelected(true);
        }

        private void DeselectSiblings() // 同一父节点下只保留一个选中态（中文注释）
        {
            var parent = transform.parent;
            if (parent == null) return;

            int childCount = parent.childCount;
            for (int i = 0; i < childCount; i++)
            {
                var slot = parent.GetChild(i).GetComponent<PackageUISlotController>();
                if (slot == null || slot == this) continue;
                slot.SetSelected(false);
            }
        }

        private void SetSelected(bool selected) // 控制选中背景显隐（中文注释）
        {
            if (selectedBackground == null) return;
            selectedBackground.SetActive(selected);
        }

        private void ResolveOptionalReferences()
        {
            if (numberText == null) numberText = transform.Find("Number")?.GetComponent<TMP_Text>();
            if (numberBackground == null) numberBackground = transform.Find("NumberBackground")?.gameObject;
        }

        private void ApplySlotTypeVisuals()
        {
            bool showHotbarNumber = slotType == UISlotType.Hotbar;

            if (numberBackground != null) numberBackground.SetActive(showHotbarNumber);
            if (numberText != null) numberText.gameObject.SetActive(showHotbarNumber);
        }

        private void UpdateHotbarNumber()
        {
            if (slotType != UISlotType.Hotbar) return;
            if (numberText == null) numberText = transform.Find("Number")?.GetComponent<TMP_Text>();
            if (numberText == null) return;

            int displayNumber = (index + 1) % 10;
            numberText.text = displayNumber.ToString();
        }

        private void ApplyCountVisuals(IItem item, int count)
        {
            if (countText == null) countText = transform.Find("Count")?.GetComponent<TMP_Text>();
            if (countText == null) return;

            bool canStack = item is IStackable s ? s.MaxStack > 1 : (item is IConsumable c ? c.MaxStack > 1 : false);
            bool showCount = canStack && count > 1;

            countText.text = showCount ? count.ToString() : "";
            countText.gameObject.SetActive(showCount);
        }

        private void ApplyRarityVisuals(IItem item)
        {
            if (iconBackground == null) iconBackground = transform.Find("IconBackground")?.GetComponent<Image>();
            if (iconBackground == null) return;

            var rarity = ItemRarity.Common;
            if (item is ItemBase itemBase) rarity = itemBase.Rarity;

            iconBackground.color = GetRarityColor(rarity);
        }

        private static Color GetRarityColor(ItemRarity rarity)
        {
            switch (rarity)
            {
                case ItemRarity.Uncommon: return new Color(0.30f, 0.85f, 0.35f, 1f);
                case ItemRarity.Rare: return new Color(0.25f, 0.55f, 0.95f, 1f);
                case ItemRarity.Epic: return new Color(0.75f, 0.35f, 0.95f, 1f);
                case ItemRarity.Legendary: return new Color(0.98f, 0.62f, 0.12f, 1f);
                default: return new Color(0.65f, 0.65f, 0.65f, 1f);
            }
        }

        private void UpdateDragPosition(PointerEventData eventData)
        {
            if (_rect == null) return;
            if (dragCanvas == null || dragCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                _rect.position = eventData.position;
            }
            else
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(dragCanvas.transform as RectTransform, eventData.position, dragCanvas.worldCamera, out var localPoint);
                _rect.localPosition = localPoint;
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            var dragObj = eventData.pointerDrag;
            if (dragObj == null) return;
            var srcSlot = dragObj.GetComponent<PackageUISlotController>();
            if (srcSlot == null) return;

            var req = new TransferRequest
            {
                From = srcSlot.sourceType,
                To = sourceType,
                FromIndex = srcSlot.index,
                ToIndex = index,
                FromContainerId = srcSlot.containerId,
                ToContainerId = containerId
            };
            var transferSystem = InventoryTransferSystem.Instance;
            if (transferSystem == null) return;
            transferSystem.TryTransfer(req);
        }
    }
}
