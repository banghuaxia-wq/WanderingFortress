using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WF.UI
{
    /// <summary>
    /// 快捷栏选择控制：监听数字键 1-9，切换各槽位的 SelectedBackground 显示。
    /// 不要求 CanvasGroup，直接启用/禁用对象即可；若存在 CanvasGroup，则可选用渐隐方式。
    /// </summary>
    public class HotbarController : MonoBehaviour
    {
    [Header("Hierarchy")]
    [Tooltip("HotbarPanel 根对象；若为空将自动查找 HUDCanvas/HotbarPanel")] 
    [SerializeField] private Transform hotbarPanel;

    [Tooltip("是否使用 CanvasGroup 控制可见性（若组件存在）")]
    [SerializeField] private bool useCanvasGroupIfPresent = false;

    private readonly List<HotbarSlot> _slots = new List<HotbarSlot>();
    private int _selectedIndex = -1;

    private void Awake()
    {
        if (hotbarPanel == null)
        {
            var hud = GameObject.Find("HUDCanvas");
            if (hud != null)
            {
                var t = hud.transform.Find("HotbarPanel");
                if (t != null) hotbarPanel = t;
            }
        }
    }

    private void Start()
    {
        BuildSlotCache();
        // 默认选中第一个槽（若存在）
        if (_slots.Count > 0)
        {
            SelectSlot(0);
        }
    }

    private void Update()
    {
        int keyIndex = ReadNumberKeyIndex();
        if (keyIndex >= 0 && keyIndex < _slots.Count)
        {
            SelectSlot(keyIndex);
        }
    }

    private int ReadNumberKeyIndex()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) return 0;
        if (Input.GetKeyDown(KeyCode.Alpha2)) return 1;
        if (Input.GetKeyDown(KeyCode.Alpha3)) return 2;
        if (Input.GetKeyDown(KeyCode.Alpha4)) return 3;
        if (Input.GetKeyDown(KeyCode.Alpha5)) return 4;
        if (Input.GetKeyDown(KeyCode.Alpha6)) return 5;
        if (Input.GetKeyDown(KeyCode.Alpha7)) return 6;
        if (Input.GetKeyDown(KeyCode.Alpha8)) return 7;
        if (Input.GetKeyDown(KeyCode.Alpha9)) return 8;
        return -1;
    }

    private void SelectSlot(int index)
    {
        _selectedIndex = index;
        for (int i = 0; i < _slots.Count; i++)
        {
            bool isSelected = (i == _selectedIndex);
            SetSelectedBackgroundVisible(_slots[i], isSelected);
        }
    }

    private void SetSelectedBackgroundVisible(HotbarSlot slot, bool visible)
    {
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
        if (hotbarPanel == null) return;

        // 搜集所有名称以 "HotbarUISlot" 开头的子物体作为槽
        foreach (Transform child in hotbarPanel)
        {
            if (!child.name.StartsWith("HotbarUISlot")) continue;

            var selected = child.Find("SelectedBackground");
            var slot = new HotbarSlot
            {
                root = child,
                selectedBackground = selected ? selected.gameObject : null,
                canvasGroup = selected ? selected.GetComponent<CanvasGroup>() : null
            };
            _slots.Add(slot);
        }
    }

    private class HotbarSlot
    {
        public Transform root;
        public GameObject selectedBackground;
        public CanvasGroup canvasGroup;
    }
}
}
