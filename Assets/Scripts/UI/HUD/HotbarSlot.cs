using UnityEngine;

namespace WF.Gameplay.UI.HUD
{
    /// <summary>
    /// 快捷栏槽位组件，用于引用选中态背景等UI元素。
    /// </summary>
    public class HotbarSlot : MonoBehaviour
    {
        [Tooltip("选中时的背景高亮对象")]
        public GameObject selectedBackground;
        
        [Tooltip("用于控制透明度或交互的CanvasGroup（可选）")]
        public CanvasGroup canvasGroup;
    }
}
