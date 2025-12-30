using System.Collections.Generic;
using UnityEngine;

namespace WF.Gameplay.Core.Data
{
    [CreateAssetMenu(fileName = "ContainerLootConfig", menuName = "WF/Loot/Container Loot Config")]
    public class ContainerLootConfigSO : ScriptableObject
    {
        [Tooltip("按容器类型映射到掉落表")]
        [SerializeField] private List<ContainerLootMapping> mappings = new List<ContainerLootMapping>();

        public LootTableSO GetLootTable(ContainerType containerType)
        {
            for (int i = 0; i < mappings.Count; i++)
            {
                var m = mappings[i];
                if (m != null && m.ContainerType == containerType) return m.LootTable;
            }

            return null;
        }
    }
}

