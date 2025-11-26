using UnityEngine;
using WF.Gameplay.Core.Data;
using WF.Gameplay.Systems.EventSystem;

namespace WF.Gameplay.Systems.InventorySystem
{
    public class StorageSystem : MonoBehaviour
    {
        public static StorageSystem Instance { get; private set; }
        [SerializeField] private string warehouseId = "Warehouse";
        private void Awake() { if (Instance != null && Instance != this) { Destroy(gameObject); return; } Instance = this; DontDestroyOnLoad(gameObject); }
        public void EnsureWarehouse(int slotLimit) { var mgr = ContainerSystem.ContainerManager.Instance; if (mgr == null) return; var d = mgr.Get(warehouseId); if (d == null) { d = new ContainerData { Id = warehouseId, Type = ContainerType.Warehouse, SlotLimit = slotLimit }; mgr.Register(d); GameEvents.RaiseContainerUpdated(d); } }
    }
}
