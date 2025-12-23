using UnityEngine;
using WF.Gameplay.Core.Data;

namespace WF.Gameplay.Core.Interfaces
{
    public interface ISimulationLodAgent
    {
        Transform Transform { get; } // 目标的Transform（用于距离判断）
        SimulationLodState CurrentState { get; } // 当前仿真分级状态
        void ApplyState(SimulationLodState state); // 应用新的仿真分级状态（由系统驱动）
    }
}
