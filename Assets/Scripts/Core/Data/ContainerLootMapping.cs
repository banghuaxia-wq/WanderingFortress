using UnityEngine;

namespace WF.Gameplay.Core.Data
{
    [System.Serializable]
    public class ContainerLootMapping
    {
        [Tooltip("容器类型")]
        public ContainerType ContainerType = ContainerType.NormalBox;

        [Tooltip("使用的掉落表")]
        public LootTableSO LootTable;
    }
}

