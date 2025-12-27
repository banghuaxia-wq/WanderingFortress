using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WF.Gameplay.Core.Events;
using WF.Gameplay.Systems.InventorySystem;

namespace WF.Gameplay.UI.HUD
{
    /// <summary>
    /// 快捷栏选择控制：监听数字键 1-9，切换各槽位的 SelectedBackground 显示。
    /// 逻辑状态委托给 HotbarSystem。
    /// </summary>
    public class HotbarController : MonoBehaviour
    {
        [Header("Hierarchy")]
        [Tooltip("HotbarPanel 根对象；若为空将自动查找 HUDCanvas/HotbarPanel")] 
        [SerializeField] private Transform hotbarPanel;

        [Tooltip("是否使用 CanvasGroup 控制可见性（若组件存在）")]
        [SerializeField] private bool useCanvasGroupIfPresent = false;

        private readonly List<HotbarSlot> _slots = new List<HotbarSlot>();
        // No local selectedIndex, use HotbarSystem's
        private bool _initialized; // 是否已执行默认选中（中文注释）

        [Tooltip("选中高亮的单一指示器（将被移动到选中槽位）")]
        [SerializeField] private RectTransform selectedIndicator; // 单一选中指示器（中文注释）

        [Tooltip("指示器相对槽位的偏移量（像素)")]
        [SerializeField] private Vector2 selectedIndicatorOffset = new Vector2(0f, 20f); // 指示器偏移（中文注释）

        [Tooltip("指示器移动动画时长（秒）")]
        [SerializeField] private float selectedIndicatorMoveSeconds = 0.15f; // 指示器移动时长（中文注释）

        private Coroutine _indicatorMoveCo; // 指示器移动协程引用（中文注释）
        private Coroutine _rebuildAfterUpdateCo;
        private Coroutine _refreshAfterLayoutCo; // 等待布局稳定后刷新选中指示器（中文注释）

        private void Awake()
        {
            ResolveHotbarPanel();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<HotbarUpdatedEvent>(OnHotbarUpdated);
            EventBus.Subscribe<HotbarSelectionChangedEvent>(OnHotbarSelectionChanged);
            // Delay build to ensure HotbarPanelManager has initialized
            // Or just try to build now
            ResolveHotbarPanel();
            BuildSlotCache();
            EnsureIndicatorInitialized();
            ScheduleRefreshAfterLayout();

            // 默认选中第一格，并重置其他格子的SelectedBackground为隐藏（仅首次）（中文注释）
            if (!_initialized && HotbarSystem.Instance != null)
            {
                HotbarSystem.Instance.SelectSlot(0);
                ScheduleRefreshAfterLayout();
                _initialized = true;
            }
        }

        private void Start()
        {
            // 再次保障默认选中在场景启动后生效（例如初始化顺序不同）（中文注释）
            if (!_initialized)
            {
                StartCoroutine(EnsureDefaultSelectionNextFrame());
            }
        }

        private System.Collections.IEnumerator EnsureDefaultSelectionNextFrame()
        {
            yield return null; // 等待一帧，确保所有UI已生成
            ResolveHotbarPanel();
            BuildSlotCache();
            if (HotbarSystem.Instance != null)
            {
                HotbarSystem.Instance.SelectSlot(0);
            }
            ScheduleRefreshAfterLayout();
            _initialized = true;
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<HotbarUpdatedEvent>(OnHotbarUpdated);
            EventBus.Unsubscribe<HotbarSelectionChangedEvent>(OnHotbarSelectionChanged);
            if (_rebuildAfterUpdateCo != null)
            {
                StopCoroutine(_rebuildAfterUpdateCo);
                _rebuildAfterUpdateCo = null;
            }
            if (_indicatorMoveCo != null)
            {
                StopCoroutine(_indicatorMoveCo);
                _indicatorMoveCo = null;
            }
            if (_refreshAfterLayoutCo != null)
            {
                StopCoroutine(_refreshAfterLayoutCo);
                _refreshAfterLayoutCo = null;
            }
        }

        private void OnDestroy()
        {
            if (_rebuildAfterUpdateCo != null)
            {
                StopCoroutine(_rebuildAfterUpdateCo);
                _rebuildAfterUpdateCo = null;
            }
            if (_indicatorMoveCo != null)
            {
                StopCoroutine(_indicatorMoveCo);
                _indicatorMoveCo = null;
            }
            if (_refreshAfterLayoutCo != null)
            {
                StopCoroutine(_refreshAfterLayoutCo);
                _refreshAfterLayoutCo = null;
            }
        }

        private void OnHotbarUpdated(HotbarUpdatedEvent e)
        {
            if (!isActiveAndEnabled) return;
            if (_rebuildAfterUpdateCo != null) StopCoroutine(_rebuildAfterUpdateCo);
            _rebuildAfterUpdateCo = StartCoroutine(RebuildAfterHotbarUpdate());
        }

        private void OnHotbarSelectionChanged(HotbarSelectionChangedEvent e)
        {
            RefreshSelectionVisuals();
        }

        private System.Collections.IEnumerator RebuildAfterHotbarUpdate()
        {
            yield return null;
            BuildSlotCacheAndRefresh();
            _rebuildAfterUpdateCo = null;
        }

        private void BuildSlotCacheAndRefresh()
        {
            BuildSlotCache();
            ScheduleRefreshAfterLayout();
        }

        private void ScheduleRefreshAfterLayout() // 延迟到布局稳定后刷新选中态，避免首次位置偏移（中文注释）
        {
            if (!isActiveAndEnabled) return;
            if (_refreshAfterLayoutCo != null) StopCoroutine(_refreshAfterLayoutCo);
            _refreshAfterLayoutCo = StartCoroutine(RefreshAfterLayoutStable());
        }

        private System.Collections.IEnumerator RefreshAfterLayoutStable() // 等待一帧并强制布局重建后刷新（中文注释）
        {
            yield return null;
            ForceRebuildHotbarLayout();
            RefreshSelectionVisuals();
            _refreshAfterLayoutCo = null;
        }

        private void ForceRebuildHotbarLayout() // 强制HotbarPanel布局计算，确保anchoredPosition已更新（中文注释）
        {
            if (hotbarPanel == null) return;
            var rt = hotbarPanel as RectTransform;
            if (rt == null) return;
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
            Canvas.ForceUpdateCanvases();
        }

        private void Update()
        {
            int keyIndex = ReadNumberKeyIndex();
            if (keyIndex >= 0)
            {
                if (HotbarSystem.Instance != null)
                {
                    HotbarSystem.Instance.SelectSlot(keyIndex);
                }
            }
        }

        private int ReadNumberKeyIndex()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) return 0;
            if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) return 1;
            if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) return 2;
            if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4)) return 3;
            if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5)) return 4;
            if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6)) return 5;
            if (Input.GetKeyDown(KeyCode.Alpha7) || Input.GetKeyDown(KeyCode.Keypad7)) return 6;
            if (Input.GetKeyDown(KeyCode.Alpha8) || Input.GetKeyDown(KeyCode.Keypad8)) return 7;
            if (Input.GetKeyDown(KeyCode.Alpha9) || Input.GetKeyDown(KeyCode.Keypad9)) return 8;
            if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0)) return 9;
            return -1;
        }

        private void RefreshSelectionVisuals()
        {
            if (HotbarSystem.Instance == null) return;
            EnsureIndicatorInitialized();
            HideAllSlotSelectedBackgrounds();
            int selectedIndex = HotbarSystem.Instance.SelectedIndex;
            if (selectedIndex >= 0 && selectedIndex < _slots.Count)
            {
                MoveIndicatorToSlot(_slots[selectedIndex]);
            }
        }

        private void SetSelectedBackgroundVisible(HotbarSlot slot, bool visible)
        {
            if (slot == null) return;
            if (slot.selectedBackground == null) return;

            if (useCanvasGroupIfPresent && slot.canvasGroup != null)
            {
                slot.canvasGroup.alpha = visible ? 1f : 0f;
                slot.canvasGroup.blocksRaycasts = visible;
                slot.canvasGroup.interactable = visible;
            }
            else
            {
                slot.selectedBackground.SetActive(visible);
            }
        }

        private void BuildSlotCache()
        {
            _slots.Clear();
            if (hotbarPanel == null)
            {
                ResolveHotbarPanel();
                if (hotbarPanel == null) return;
            }
            
            // Re-find children
            int count = hotbarPanel.childCount;
            for (int i = 0; i < count; i++)
            {
                var child = hotbarPanel.GetChild(i);
                var slot = child.GetComponent<HotbarSlot>();
                if (slot == null)
                {
                    // 自动绑定：若缺少HotbarSlot组件，则在运行时添加并查找SelectedBackground（新方法，中文注释）
                    var selected = child.Find("SelectedBackground");
                    if (selected != null)
                    {
                        slot = child.gameObject.AddComponent<HotbarSlot>();
                        slot.selectedBackground = selected.gameObject;
                        var cg = selected.GetComponent<CanvasGroup>();
                        if (cg != null) slot.canvasGroup = cg;
                    }
                }
                if (slot != null) { _slots.Add(slot); }
            }
        }

        private void EnsureIndicatorInitialized()
        {
            if (selectedIndicator != null)
            {
                EnsureIndicatorIgnoresLayout(selectedIndicator);
                HideAllSlotSelectedBackgrounds();
                return;
            }
            if (_slots.Count == 0) BuildSlotCache();
            // 优先使用HotbarPanel下已存在的名为"SelectedIndicator"的对象
            if (hotbarPanel != null)
            {
                var found = hotbarPanel.Find("SelectedIndicator");
                if (found != null)
                {
                    selectedIndicator = found.GetComponent<RectTransform>();
                    EnsureIndicatorIgnoresLayout(selectedIndicator);
                    HideAllSlotSelectedBackgrounds();
                    return;
                }
            }
            // 使用第一个槽位的 SelectedBackground 作为指示器并移到 HotbarPanel 根下（中文注释）
            for (int i = 0; i < _slots.Count; i++)
            {
                var sb = _slots[i]?.selectedBackground;
                if (sb != null)
                {
                    selectedIndicator = sb.GetComponent<RectTransform>();
                    if (selectedIndicator != null)
                    {
                        selectedIndicator.SetParent(hotbarPanel, false);
                        selectedIndicator.name = "SelectedIndicator";
                        selectedIndicator.anchoredPosition = Vector2.zero;
                        selectedIndicator.localScale = Vector3.one;
                        selectedIndicator.localRotation = Quaternion.identity;
                        EnsureIndicatorIgnoresLayout(selectedIndicator);
                    }
                    break;
                }
            }
            HideAllSlotSelectedBackgrounds();
        }

        private void EnsureIndicatorIgnoresLayout(RectTransform indicator) // 确保指示器不参与布局计算（中文注释）
        {
            if (indicator == null) return;
            var layoutElement = indicator.GetComponent<LayoutElement>();
            if (layoutElement == null) layoutElement = indicator.gameObject.AddComponent<LayoutElement>();
            layoutElement.ignoreLayout = true;
        }

        private void HideAllSlotSelectedBackgrounds()
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                var sb = _slots[i]?.selectedBackground;
                if (sb == null) continue;
                if (selectedIndicator != null && sb == selectedIndicator.gameObject) continue; // 不隐藏作为指示器的对象
                sb.SetActive(false);
            }
        }

        private void MoveIndicatorToSlot(HotbarSlot slot)
        {
            if (slot == null) return;
            if (selectedIndicator == null)
            {
                EnsureIndicatorInitialized();
                if (selectedIndicator == null) return;
            }
            var rt = slot.transform as RectTransform;
            if (rt == null) return;
            selectedIndicator.gameObject.SetActive(true);
            // 对齐锚点与枢轴，避免不同布局导致位置偏差（中文注释）
            selectedIndicator.anchorMin = rt.anchorMin;
            selectedIndicator.anchorMax = rt.anchorMax;
            selectedIndicator.pivot = rt.pivot;

            Vector2 target = rt.anchoredPosition + selectedIndicatorOffset;
            if (_indicatorMoveCo != null) StopCoroutine(_indicatorMoveCo);
            _indicatorMoveCo = StartCoroutine(AnimateIndicatorTo(target));
            selectedIndicator.SetAsLastSibling();
        }

        private System.Collections.IEnumerator AnimateIndicatorTo(Vector2 target)
        {
            var indicator = selectedIndicator;
            if (indicator == null)
            {
                _indicatorMoveCo = null;
                yield break;
            }

            Vector2 start = indicator.anchoredPosition;
            float dur = Mathf.Max(0.0001f, selectedIndicatorMoveSeconds);
            float t = 0f;
            while (t < 1f)
            {
                if (indicator == null)
                {
                    _indicatorMoveCo = null;
                    yield break;
                }
                t += Time.deltaTime / dur;
                indicator.anchoredPosition = Vector2.Lerp(start, target, t);
                yield return null;
            }
            if (indicator != null)
            {
                indicator.anchoredPosition = target;
            }
            _indicatorMoveCo = null;
        }

        private void ResolveHotbarPanel()
        {
            if (hotbarPanel != null) return;
            // 优先使用当前对象自身
            if (transform != null && string.Equals(transform.name, "HotbarPanel"))
            {
                hotbarPanel = transform;
                return;
            }
            // 尝试在父层级查找
            var t1 = transform.Find("HotbarPanel");
            if (t1 != null) { hotbarPanel = t1; return; }
            // 尝试从 HUDCanvas 下查找
            var hud = GameObject.Find("HUDCanvas");
            if (hud != null)
            {
                var t2 = hud.transform.Find("HotbarPanel");
                if (t2 != null) { hotbarPanel = t2; return; }
            }
            // 全场景查找同名对象
            foreach (var tr in GameObject.FindObjectsOfType<Transform>())
            {
                if (string.Equals(tr.name, "HotbarPanel")) { hotbarPanel = tr; break; }
            }
        }

        /// <summary>
        /// 运行时自动绑定槽位的SelectedBackground（中文注释）
        /// </summary>
        private void AutoBindMissingSlots() { BuildSlotCache(); }
    }
}
