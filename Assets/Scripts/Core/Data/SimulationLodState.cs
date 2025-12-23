namespace WF.Gameplay.Core.Data
{
    public enum SimulationLodState
    {
        Dormant, // 休眠：不进行仿真更新，仅保留最小状态
        Passive, // 半活跃：低频/简化仿真（例如降频AI、关闭昂贵组件）
        Active // 活跃：完整仿真（AI/动画/物理等全量运行）
    }
}
