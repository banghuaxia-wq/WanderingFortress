using UnityEngine;
using WF.Gameplay.Core.Data;
using WF.Gameplay.Systems.ContainerSystem;

namespace WF.Gameplay.Systems.InventorySystem
{
    public class InventoryTransferSystem : MonoBehaviour
    {
        public static InventoryTransferSystem Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public bool TryTransfer(TransferRequest request)
        {
            if (request == null) return false;

            if (request.From == TransferSource.Box && request.To == TransferSource.Package) return TryMoveBoxToPackage(request);
            if (request.From == TransferSource.Package && request.To == TransferSource.Box) return TryMovePackageToBox(request);
            if (request.From == TransferSource.Package && request.To == TransferSource.Hotbar) return TryAssignPackageToHotbar(request);
            if (request.From == TransferSource.Hotbar && request.To == TransferSource.Package) return TryUnassignHotbar(request);
            if (request.From == TransferSource.Package && request.To == TransferSource.Equipment) return TryEquipFromPackage(request);
            if (request.From == TransferSource.Equipment && request.To == TransferSource.Package) return TryUnequipToPackage(request);

            return false;
        }

        private bool TryMoveBoxToPackage(TransferRequest request)
        {
            var inventory = PlayerInventory.Instance;
            var containerManager = ContainerManager.Instance;
            if (inventory == null || containerManager == null) return false;

            var container = containerManager.Get(request.FromContainerId);
            if (container == null) return false;

            if (request.FromIndex < 0 || request.FromIndex >= container.Items.Count) return false;
            if (inventory.Items.Count >= inventory.Capacity) return false;

            var item = container.Items[request.FromIndex];
            if (item == null) return false;

            inventory.Add(item);
            containerManager.RemoveItem(request.FromContainerId, request.FromIndex);
            return true;
        }

        private bool TryMovePackageToBox(TransferRequest request)
        {
            var inventory = PlayerInventory.Instance;
            var containerManager = ContainerManager.Instance;
            if (inventory == null || containerManager == null) return false;

            if (request.FromIndex < 0 || request.FromIndex >= inventory.Items.Count) return false;
            if (string.IsNullOrEmpty(request.ToContainerId)) return false;

            var container = containerManager.Get(request.ToContainerId);
            if (container == null) return false;
            if (container.SlotLimit > 0 && container.Items.Count >= container.SlotLimit) return false;

            var item = inventory.Items[request.FromIndex];
            if (item == null) return false;

            containerManager.AddItem(request.ToContainerId, item);
            inventory.RemoveAt(request.FromIndex);
            return true;
        }

        private bool TryAssignPackageToHotbar(TransferRequest request)
        {
            var inventory = PlayerInventory.Instance;
            var hotbar = HotbarSystem.Instance;
            if (inventory == null || hotbar == null) return false;

            if (request.FromIndex < 0 || request.FromIndex >= inventory.Items.Count) return false;
            var item = inventory.Items[request.FromIndex];
            if (item == null) return false;

            return hotbar.Set(request.ToIndex, item);
        }

        private bool TryUnassignHotbar(TransferRequest request)
        {
            var hotbar = HotbarSystem.Instance;
            if (hotbar == null) return false;

            return hotbar.Set(request.FromIndex, null);
        }

        private bool TryEquipFromPackage(TransferRequest request)
        {
            var inventory = PlayerInventory.Instance;
            var equipment = EquipmentSystem.Instance;
            if (inventory == null || equipment == null) return false;

            if (request.FromIndex < 0 || request.FromIndex >= inventory.Items.Count) return false;

            var slot = (EquipmentSlotType)request.ToIndex;
            var previous = equipment.Get(slot);
            if (previous != null)
            {
                if (inventory.Items.Count >= inventory.Capacity) return false;
                equipment.Unequip(slot);
                inventory.Add(previous);
            }

            var item = inventory.Items[request.FromIndex];
            if (item == null) return false;

            if (!equipment.Equip(slot, item)) return false;

            inventory.RemoveAt(request.FromIndex);
            return true;
        }

        private bool TryUnequipToPackage(TransferRequest request)
        {
            var inventory = PlayerInventory.Instance;
            var equipment = EquipmentSystem.Instance;
            if (inventory == null || equipment == null) return false;

            if (inventory.Items.Count >= inventory.Capacity) return false;

            var slot = (EquipmentSlotType)request.FromIndex;
            var item = equipment.Get(slot);
            if (item == null) return false;

            inventory.Add(item);
            equipment.Unequip(slot);
            return true;
        }
    }
}
