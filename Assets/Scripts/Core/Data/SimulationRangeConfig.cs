using UnityEngine;

namespace WF.Gameplay.Core.Data
{
    [CreateAssetMenu(fileName = "SimulationRangeConfig", menuName = "WF/World/Simulation Range Config")]
    public class SimulationRangeConfig : ScriptableObject
    {
        [Header("Spatial Hash")]
        [SerializeField] private float bucketSizeMeters = 8f; // 空间分桶尺寸（米），用于粗筛候选实体

        [Header("Update")]
        [SerializeField] private float updateIntervalSeconds = 0.25f; // 系统刷新间隔（秒），决定状态切换与分桶更新频率

        [Header("Active Range (Hysteresis)")]
        [SerializeField] private float activeEnterRadiusMeters = 30f; // 进入Active的半径（米）
        [SerializeField] private float activeExitRadiusMeters = 36f; // 退出Active的半径（米，回滞防抖）

        [Header("Passive Range (Hysteresis)")]
        [SerializeField] private float passiveEnterRadiusMeters = 65f; // 进入Passive的半径（米）
        [SerializeField] private float passiveExitRadiusMeters = 80f; // 退出Passive的半径（米，回滞防抖）

        public float BucketSizeMeters => bucketSizeMeters; // 分桶边长（米）
        public float UpdateIntervalSeconds => updateIntervalSeconds; // 刷新间隔（秒）
        public float ActiveEnterRadiusMeters => activeEnterRadiusMeters; // Active进入阈值（米）
        public float ActiveExitRadiusMeters => activeExitRadiusMeters; // Active退出阈值（米）
        public float PassiveEnterRadiusMeters => passiveEnterRadiusMeters; // Passive进入阈值（米）
        public float PassiveExitRadiusMeters => passiveExitRadiusMeters; // Passive退出阈值（米）

        public float MaxRadiusMeters => Mathf.Max(passiveEnterRadiusMeters, passiveExitRadiusMeters, activeEnterRadiusMeters, activeExitRadiusMeters); // 当前配置下可能用到的最大半径（用于候选桶遍历范围）

        // 校验并修正配置参数范围（中文注释）
        private void OnValidate()
        {
            bucketSizeMeters = Mathf.Max(1f, bucketSizeMeters);
            updateIntervalSeconds = Mathf.Max(0.02f, updateIntervalSeconds);

            activeEnterRadiusMeters = Mathf.Max(0f, activeEnterRadiusMeters);
            activeExitRadiusMeters = Mathf.Max(activeEnterRadiusMeters, activeExitRadiusMeters);

            passiveEnterRadiusMeters = Mathf.Max(activeExitRadiusMeters, passiveEnterRadiusMeters);
            passiveExitRadiusMeters = Mathf.Max(passiveEnterRadiusMeters, passiveExitRadiusMeters);
        }
    }
}
