using UnityEngine;
using WF.Gameplay.Core.Data;
using WF.Gameplay.Core.Interfaces;

namespace WF.Gameplay.Systems.WorldSimulation
{
    public class SimulationLodAgent : MonoBehaviour, ISimulationLodAgent
    {
        [SerializeField] private Behaviour[] activeOnlyBehaviours; // 仅在Active时启用的组件列表（例如AI、寻路等）
        [SerializeField] private Behaviour[] passiveAndActiveBehaviours; // 在Passive与Active时启用的组件列表（例如低成本动画/特效等）
        [SerializeField] private GameObject[] activeOnlyObjects; // 仅在Active时显示/启用的对象（例如高成本VFX）
        [SerializeField] private GameObject[] passiveAndActiveObjects; // 在Passive与Active时显示/启用的对象

        public Transform Transform => transform; // 当前对象Transform（中文注释）

        public SimulationLodState CurrentState { get; private set; } = SimulationLodState.Active; // 当前仿真分级状态

        // 启用时自动注册到距离分级系统（中文注释）
        private void OnEnable()
        {
            var system = SimulationRangeSystem.Instance != null
                ? SimulationRangeSystem.Instance
                : FindObjectOfType<SimulationRangeSystem>();

            if (system != null)
            {
                system.Register(this);
            }
        }

        // 禁用时自动从系统注销（中文注释）
        private void OnDisable()
        {
            if (SimulationRangeSystem.Instance != null)
            {
                SimulationRangeSystem.Instance.Unregister(this);
            }
        }

        // 根据状态切换批量开关组件/对象（中文注释）
        public void ApplyState(SimulationLodState state)
        {
            if (CurrentState == state) return;
            CurrentState = state;

            bool isActive = state == SimulationLodState.Active;
            bool isPassiveOrActive = state == SimulationLodState.Active || state == SimulationLodState.Passive;

            SetEnabled(activeOnlyBehaviours, isActive);
            SetEnabled(passiveAndActiveBehaviours, isPassiveOrActive);
            SetActive(activeOnlyObjects, isActive);
            SetActive(passiveAndActiveObjects, isPassiveOrActive);
        }

        // 批量启用/禁用Behaviour组件（中文注释）
        private static void SetEnabled(Behaviour[] behaviours, bool enabled)
        {
            if (behaviours == null) return;
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] == null) continue;
                behaviours[i].enabled = enabled;
            }
        }

        // 批量显示/隐藏GameObject（中文注释）
        private static void SetActive(GameObject[] objects, bool active)
        {
            if (objects == null) return;
            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i] == null) continue;
                objects[i].SetActive(active);
            }
        }
    }
}
