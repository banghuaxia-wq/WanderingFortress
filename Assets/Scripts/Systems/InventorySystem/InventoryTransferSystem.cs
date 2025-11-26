using UnityEngine;
using WF.Gameplay.Core.Data;
using WF.Gameplay.Systems.EventSystem;
using WF.Gameplay.Systems.ContainerSystem;

namespace WF.Gameplay.Systems.InventorySystem
{
    public class InventoryTransferSystem : MonoBehaviour
    {
        private void OnEnable() { GameEvents.TransferRequested += OnTransferRequested; }
        private void OnDisable() { GameEvents.TransferRequested -= OnTransferRequested; }
        private void OnTransferRequested(TransferRequest req)
        {
            if (req == null) return;
            // Box → Package: 将容器的物品移入背包（中文注释）
            if (req.From == TransferSource.Box && req.To == TransferSource.Package)
            {
                var inv = PlayerInventory.Instance; var cm = ContainerManager.Instance;
                if (inv == null || cm == null) return;
                var c = cm.Get(req.FromContainerId); if (c == null) return;
                if (req.FromIndex < 0 || req.FromIndex >= c.Items.Count) return;
                if (inv.Items.Count >= inv.Capacity) return;
                var item = c.Items[req.FromIndex];
                if (item == null) return;
                inv.Add(item);
                cm.RemoveItem(req.FromContainerId, req.FromIndex);
            }
        }
    }
}
