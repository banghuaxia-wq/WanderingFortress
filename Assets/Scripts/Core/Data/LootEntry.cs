using UnityEngine;

namespace WF.Gameplay.Core.Data
{
    [System.Serializable]
    public class LootEntry
    {
        [Tooltip("分层：用于“先选层级，再层内抽取”的权重修正")]
        public LootTier Tier = LootTier.Common;

        [Tooltip("条目类型：物品 或 子掉落表")]
        public LootEntryKind Kind = LootEntryKind.Item;

        [Tooltip("物品ID（当 Kind=Item 时使用）")]
        public string ItemId;

        [Tooltip("子掉落表（当 Kind=Table 时使用）")]
        public LootTableSO Table;

        [Tooltip("基础权重")]
        [Min(0f)]
        public float BaseWeight = 1f;

        [Tooltip("幸运修正系数（正：更容易出；负：更不容易出）")]
        public float LuckSensitivity = 0f;

        [Tooltip("最小数量")]
        [Min(1)]
        public int MinCount = 1;

        [Tooltip("最大数量")]
        [Min(1)]
        public int MaxCount = 1;
    }
}
