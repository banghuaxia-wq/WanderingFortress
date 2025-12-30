using System.Collections.Generic;
using UnityEngine;

namespace WF.Gameplay.Core.Data
{
    [CreateAssetMenu(fileName = "LootTable", menuName = "WF/Loot/Loot Table")]
    public class LootTableSO : ScriptableObject
    {
        [Tooltip("掉落表唯一标识（用于调试/查找）")]
        [SerializeField] private string tableId;

        [Tooltip("一次生成的抽取次数下限")]
        [SerializeField] private int minRolls = 1;

        [Tooltip("一次生成的抽取次数上限")]
        [SerializeField] private int maxRolls = 3;

        [Tooltip("是否允许同一物品ID在同一个容器内重复抽到")]
        [SerializeField] private bool allowDuplicateItems = true;

        [Tooltip("权重下限，避免所有条目权重被压到0导致无法抽取")]
        [SerializeField] private float minEffectiveWeight = 0.01f;

        [Tooltip("嵌套最大深度（防止配表环/过深嵌套）")]
        [SerializeField] private int maxNestingDepth = 8;

        [Tooltip("Jackpot 概率（0~1）。命中后改从 JackpotTable 抽取一次")]
        [Range(0f, 1f)]
        [SerializeField] private float jackpotChance = 0f;

        [Tooltip("Jackpot 使用的掉落表")]
        [SerializeField] private LootTableSO jackpotTable;

        [Tooltip("掉落表条目")]
        [SerializeField] private List<LootEntry> entries = new List<LootEntry>();

        public string TableId => tableId;
        public int MinRolls => minRolls;
        public int MaxRolls => maxRolls;
        public bool AllowDuplicateItems => allowDuplicateItems;
        public float MinEffectiveWeight => minEffectiveWeight;
        public int MaxNestingDepth => maxNestingDepth;
        public float JackpotChance => jackpotChance;
        public LootTableSO JackpotTable => jackpotTable;
        public IReadOnlyList<LootEntry> Entries => entries;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(tableId)) tableId = name;
            if (minRolls < 0) minRolls = 0;
            if (maxRolls < minRolls) maxRolls = minRolls;
            if (minEffectiveWeight < 0f) minEffectiveWeight = 0f;
            if (maxNestingDepth < 1) maxNestingDepth = 1;
        }
    }
}

