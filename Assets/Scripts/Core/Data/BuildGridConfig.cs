using UnityEngine;

namespace WF.Gameplay.Core.Data
{
    [CreateAssetMenu(fileName = "BuildGridConfig", menuName = "WF/Building/Build Grid Config")]
    public class BuildGridConfig : ScriptableObject
    {
        [SerializeField] private float cellSizeMeters = 1f; // 网格单元尺寸（米）
        [SerializeField] private Vector3 originWorld = Vector3.zero; // 网格原点在世界坐标中的位置（cell(0,0)的起点）
        [SerializeField] private float surfaceRayStartHeightMeters = 50f;
        [SerializeField] private float surfaceRayDistanceMeters = 200f;
        [SerializeField] private float maxSurfaceSlopeDegrees = 25f;
        [SerializeField] private float maxFootprintHeightDeltaMeters = 0.5f;

        public float CellSizeMeters => cellSizeMeters; // 单元尺寸（米）
        public Vector3 OriginWorld => originWorld; // 世界原点（中文注释）
        public float SurfaceRayStartHeightMeters => surfaceRayStartHeightMeters;
        public float SurfaceRayDistanceMeters => surfaceRayDistanceMeters;
        public float MaxSurfaceSlopeDegrees => maxSurfaceSlopeDegrees;
        public float MaxFootprintHeightDeltaMeters => maxFootprintHeightDeltaMeters;

        // 校验并修正配置参数范围（中文注释）
        private void OnValidate()
        {
            cellSizeMeters = Mathf.Max(0.1f, cellSizeMeters);
            surfaceRayStartHeightMeters = Mathf.Clamp(surfaceRayStartHeightMeters, 1f, 500f);
            surfaceRayDistanceMeters = Mathf.Clamp(surfaceRayDistanceMeters, 1f, 2000f);
            maxSurfaceSlopeDegrees = Mathf.Clamp(maxSurfaceSlopeDegrees, 0f, 89f);
            maxFootprintHeightDeltaMeters = Mathf.Max(0f, maxFootprintHeightDeltaMeters);
        }
    }
}
