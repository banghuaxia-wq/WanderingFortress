using System;
using WF.Gameplay.Core.Data;

namespace WF.Gameplay.Systems.EventSystem
{
    public static class GameEvents
    {
        public static event Action<ContainerData> ContainerOpened;
        public static event Action<ContainerData> ContainerUpdated;
        public static event Action<ContainerData> ContainerClosed;
        public static event Action PlayerInventoryUpdated;
        public static event Action EquipmentUpdated;
        public static event Action HotbarUpdated;
        public static event Action<float, bool, bool> WeightChanged;
        public static event Action<TransferRequest> TransferRequested;
        public static void RaiseContainerOpened(ContainerData d) { ContainerOpened?.Invoke(d); }
        public static void RaiseContainerUpdated(ContainerData d) { ContainerUpdated?.Invoke(d); }
        public static void RaiseContainerClosed(ContainerData d) { ContainerClosed?.Invoke(d); }
        public static void RaisePlayerInventoryUpdated() { PlayerInventoryUpdated?.Invoke(); }
        public static void RaiseEquipmentUpdated() { EquipmentUpdated?.Invoke(); }
        public static void RaiseHotbarUpdated() { HotbarUpdated?.Invoke(); }
        public static void RaiseWeightChanged(float w, bool overweight, bool overloaded) { WeightChanged?.Invoke(w, overweight, overloaded); }
        public static void RaiseTransferRequested(TransferRequest req) { TransferRequested?.Invoke(req); }
    }
}
