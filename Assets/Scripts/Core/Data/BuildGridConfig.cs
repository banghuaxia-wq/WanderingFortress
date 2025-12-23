using UnityEngine;

namespace WF.Gameplay.Core.Data
{
    [CreateAssetMenu(fileName = "BuildGridConfig", menuName = "WF/Building/Build Grid Config")]
    public class BuildGridConfig : ScriptableObject
    {
        [SerializeField] private float cellSizeMeters = 1f; // 网格单元尺寸（米）
        [SerializeField] private Vector3 originWorld = Vector3.zero; // 网格原点在世界坐标中的位置（cell(0,0)的起点）

        public float CellSizeMeters => cellSizeMeters; // 单元尺寸（米）
        public Vector3 OriginWorld => originWorld; // 世界原点（中文注释）

        // 校验并修正配置参数范围（中文注释）
        private void OnValidate()
        {
            cellSizeMeters = Mathf.Max(0.1f, cellSizeMeters);
        }
    }
}
