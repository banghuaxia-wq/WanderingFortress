using UnityEngine;

namespace WF.Gameplay.Core.Data
{
    [CreateAssetMenu(fileName = "BuildingDefinition", menuName = "WF/Building/Building Definition")]
    public class BuildingDefinition : ScriptableObject
    {
        [SerializeField] private string id; // 建筑定义ID（建议唯一，用于存档/查找）（中文注释）
        [SerializeField] private GameObject prefab; // 放置时生成的预制体（可为空，仅占格）（中文注释）
        [SerializeField] private bool allowRotation = true; // 是否允许旋转（中文注释）
        [SerializeField] private Vector2Int[] footprintOffsets = { new Vector2Int(0, 0) }; // 占格偏移（相对锚点格）（中文注释）

        public string Id => id; // 建筑ID（中文注释）
        public GameObject Prefab => prefab; // 建筑预制体（中文注释）
        public bool AllowRotation => allowRotation; // 是否允许旋转（中文注释）
        public Vector2Int[] FootprintOffsets => footprintOffsets; // 占格偏移列表（中文注释）

        // 校验并补全默认值（中文注释）
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(id)) id = name;
            if (footprintOffsets == null || footprintOffsets.Length == 0)
            {
                footprintOffsets = new[] { new Vector2Int(0, 0) };
            }
        }
    }
}
