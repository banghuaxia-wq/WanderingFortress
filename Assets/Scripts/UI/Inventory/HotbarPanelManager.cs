using UnityEngine;
using WF.Gameplay.Systems.InventorySystem;
using WF.Gameplay.Systems.EventSystem;

namespace WF.Gameplay.UI.Inventory
{
    public class HotbarPanelManager : MonoBehaviour
    {
        [SerializeField] private Transform content;
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private UISlotPoolManager slotPool;
        private void OnEnable() { GameEvents.HotbarUpdated += Refresh; Refresh(); }
        private void OnDisable() { GameEvents.HotbarUpdated -= Refresh; }
        private void Refresh()
        {
            if (content == null) content = transform;
            if (slotPrefab == null || slotPool == null) return;
            var sys = HotbarSystem.Instance; if (sys == null) return;
            int childCount = content.childCount;
            for (int i = 0; i < childCount; i++) { var go = content.GetChild(i).gameObject; slotPool.Release(go); }
            for (int i = 0; i < sys.Count; i++)
            {
                var slot = slotPool.Get(slotPrefab, content);
                slot.Bind(sys.Get(i));
                slot.SetMeta(WF.Gameplay.Core.Data.TransferSource.Hotbar, null, i);
            }
        }
    }
}
