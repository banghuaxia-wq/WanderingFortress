using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using WF.Gameplay.Core.Data;
using WF.Gameplay.Core.Interfaces;
using WF.Gameplay.Systems.InventorySystem;

namespace WF.Gameplay.UI.Inventory
{
    public class PackageUISlotController : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler
    {
        [SerializeField] private Image icon; // 图标（中文注释）
        [SerializeField] private Text countText; // 数量文本（中文注释）
        [SerializeField] private TransferSource sourceType; // 槽位来源类型（中文注释）
        [SerializeField] private string containerId; // 容器ID（中文注释）
        [SerializeField] private int index; // 槽位索引（中文注释）
        [SerializeField] private Canvas dragCanvas; // 拖拽时临时父Canvas（中文注释）
        private Transform _originalParent; // 拖拽前的父节点（中文注释）
        private int _originalSibling; // 拖拽前的兄弟序号（中文注释）
        private RectTransform _rect; // 本对象RectTransform缓存（中文注释）
        private IItem _boundItem;

        public void Bind(ItemStack stack)
        {
            if (icon == null) icon = transform.Find("Icon")?.GetComponent<Image>();
            if (countText == null) countText = transform.Find("Count")?.GetComponent<Text>();
            
            if (stack == null || stack.Item == null)
            {
                if (icon != null) icon.enabled = false;
                if (countText != null) countText.text = "";
                _boundItem = null;
                return;
            }
            
            _boundItem = stack.Item;
            if (icon != null) 
            { 
                icon.enabled = true; 
                icon.sprite = stack.Icon; 
            }
            if (countText != null) 
            {
                countText.text = stack.Count > 1 ? stack.Count.ToString() : "";
            }
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
