using System;
using System.Collections.Generic;
using UnityEngine;
using WF.Gameplay.Systems.Inventory;
using WF.Gameplay.Core.Data;

namespace WF.Gameplay.Systems.ContainerSystem
{
    public static class ContainerGenerator
    {
        public static ContainerData Create(string id, ContainerType type, int slotLimit) { return new ContainerData { Id = id, Type = type, SlotLimit = slotLimit }; }
        public static ItemStack CreateItem(string itemId, ItemType type, int count, int maxStack, float weightPerUnit) 
        { 
            var stack = ItemFactory.CreateItemStack(itemId, count);
            if (stack != null) return stack;

            return new ItemStack 
            { 
                Id = itemId, 
                Count = count 
            }; 
        }

        public static void FillContainerWithLoot(ContainerData container, LootTableSO table, float luck, int seed = 0, float luckSoftCap = 0.75f)
        {
            FillContainerWithLoot(container, table, luck, seed, luckSoftCap, 1, 1f);
        }

        public static void FillContainerWithLoot(
            ContainerData container,
            LootTableSO table,
            float luck,
            int seed,
            float luckSoftCap,
            int zoneLevel,
            float zoneLuckSensitivity)
        {
            if (container == null || table == null) return;
            if (container.SlotLimit > 0 && container.Items.Count >= container.SlotLimit) return;

            var rng = seed == 0 ? new System.Random() : new System.Random(seed);
            var picked = table.AllowDuplicateItems ? null : new HashSet<string>();

            int remainingSlots = GetRemainingSlots(container);
            if (remainingSlots <= 0) return;

            var generated = GenerateLoot(table, luck, rng, picked, table.MaxNestingDepth, luckSoftCap, remainingSlots, zoneLevel, zoneLuckSensitivity);
            if (generated.Count <= 0) return;

            for (int i = 0; i < generated.Count; i++)
            {
                container.Items.Add(generated[i]);
            }
        }

        private static int GetRemainingSlots(ContainerData container)
        {
            if (container == null) return 0;
            if (container.SlotLimit <= 0) return int.MaxValue;
            return Mathf.Max(0, container.SlotLimit - container.Items.Count);
        }

        private static List<ItemStack> GenerateLoot(
            LootTableSO table,
            float luck,
            System.Random rng,
            HashSet<string> pickedItemIds,
            int maxDepth,
            float luckSoftCap,
            int maxItems,
            int zoneLevel,
            float zoneLuckSensitivity)
        {
            var results = new List<ItemStack>();
            GenerateLootInto(table, luck, rng, pickedItemIds, maxDepth, 0, luckSoftCap, maxItems, results, zoneLevel, zoneLuckSensitivity);
            return results;
        }

        private static void GenerateLootInto(
            LootTableSO table,
            float luck,
            System.Random rng,
            HashSet<string> pickedItemIds,
            int maxDepth,
            int depth,
            float luckSoftCap,
            int maxItems,
            List<ItemStack> results,
            int zoneLevel,
            float zoneLuckSensitivity)
        {
            if (table == null || rng == null || results == null) return;
            if (depth > maxDepth) return;
            if (results.Count >= maxItems) return;

            int rolls = RollInclusive(rng, table.MinRolls, table.MaxRolls);
            if (rolls <= 0) return;

            for (int i = 0; i < rolls; i++)
            {
                if (results.Count >= maxItems) return;
                var pickedTable = TryPickJackpotTable(table, rng) ?? table;

                var entry = PickEntryLayered(pickedTable, luck, rng, pickedItemIds, luckSoftCap, zoneLevel, zoneLuckSensitivity);
                if (entry == null) continue;

                if (entry.Kind == LootEntryKind.Table)
                {
                    if (entry.Table == null) continue;
                    GenerateLootInto(entry.Table, luck, rng, pickedItemIds, maxDepth, depth + 1, luckSoftCap, maxItems, results, zoneLevel, zoneLuckSensitivity);
                    continue;
                }

                if (string.IsNullOrEmpty(entry.ItemId)) continue;

                int count = RollInclusive(rng, entry.MinCount, entry.MaxCount);
                count = Mathf.Max(1, count);
                var stack = ItemFactory.CreateItemStack(entry.ItemId, count) ?? new ItemStack { Id = entry.ItemId, Count = count };
                results.Add(stack);

                pickedItemIds?.Add(entry.ItemId);
            }
        }

        private static LootTableSO TryPickJackpotTable(LootTableSO table, System.Random rng)
        {
            if (table == null || rng == null) return null;
            if (table.JackpotTable == null) return null;
            if (table.JackpotChance <= 0f) return null;
            return rng.NextDouble() < table.JackpotChance ? table.JackpotTable : null;
        }

        private static LootEntry PickEntryLayered(
            LootTableSO table,
            float luck,
            System.Random rng,
            HashSet<string> pickedItemIds,
            float luckSoftCap,
            int zoneLevel,
            float zoneLuckSensitivity)
        {
            if (table == null || rng == null) return null;
            var entries = table.Entries;
            if (entries == null || entries.Count == 0) return null;

            var normalizedLuck = ComputeNormalizedLuck(luck, luckSoftCap);

            var tierTotals = new float[GetTierCount()];
            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                if (e == null) continue;
                if (e.Kind == LootEntryKind.Item && pickedItemIds != null && !string.IsNullOrEmpty(e.ItemId) && pickedItemIds.Contains(e.ItemId)) continue;
                float baseWeight = Mathf.Max(0f, e.BaseWeight);
                if (baseWeight <= 0f) continue;
                int tierIndex = (int)e.Tier;
                if (tierIndex < 0 || tierIndex >= tierTotals.Length) tierIndex = 0;
                tierTotals[tierIndex] += baseWeight;
            }

            float tierTotal = 0f;
            for (int i = 0; i < tierTotals.Length; i++)
            {
                if (tierTotals[i] <= 0f) continue;
                tierTotals[i] *= ComputeTierMultiplier((LootTier)i, normalizedLuck);
                tierTotal += tierTotals[i];
            }

            if (tierTotal <= 0f) return null;
            int pickedTierIndex = PickIndexByWeight(rng, tierTotals, tierTotal);
            if (pickedTierIndex < 0) return null;

            float entryTotal = 0f;
            var entryWeights = new float[entries.Count];
            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                if (e == null) { entryWeights[i] = 0f; continue; }
                int tierIndex = (int)e.Tier;
                if (tierIndex != pickedTierIndex) { entryWeights[i] = 0f; continue; }
                if (e.Kind == LootEntryKind.Item && pickedItemIds != null && !string.IsNullOrEmpty(e.ItemId) && pickedItemIds.Contains(e.ItemId))
                {
                    entryWeights[i] = 0f;
                    continue;
                }

                float w = ComputeEntryWeightWithinTier(table, e, luck, luckSoftCap, zoneLevel, zoneLuckSensitivity);
                entryWeights[i] = w;
                entryTotal += w;
            }

            if (entryTotal <= 0f) return null;
            int pickedIndex = PickIndexByWeight(rng, entryWeights, entryTotal);
            return pickedIndex >= 0 ? entries[pickedIndex] : null;
        }

        private static float ComputeEntryWeightWithinTier(
            LootTableSO table,
            LootEntry entry,
            float luck,
            float luckSoftCap,
            int zoneLevel,
            float zoneLuckSensitivity)
        {
            if (entry == null) return 0f;
            float baseWeight = Mathf.Max(0f, entry.BaseWeight);
            if (baseWeight <= 0f) return 0f;

            float sensitivity = entry.LuckSensitivity * zoneLuckSensitivity;
            int safeZoneLevel = Mathf.Max(1, zoneLevel);
            float delta = luck * sensitivity * safeZoneLevel;
            float weight = baseWeight + delta;
            float minWeight = table != null ? Mathf.Max(0f, table.MinEffectiveWeight) : 0f;
            return Mathf.Max(minWeight, weight);
        }

        private static float ComputeNormalizedLuck(float luck, float luckSoftCap)
        {
            float capped = Mathf.Clamp01(luckSoftCap);
            return Tanh(luck) * capped;
        }

        private static float Tanh(float v)
        {
            return (float)Math.Tanh(v);
        }

        private static float ComputeTierMultiplier(LootTier tier, float normalizedLuck)
        {
            int maxIndex = Mathf.Max(1, GetTierCount() - 1);
            float bias01 = Mathf.Clamp01((float)(int)tier / maxIndex);
            float signed = (bias01 - 0.5f) * 2f;
            float multiplier = 1f + (normalizedLuck * signed);
            return Mathf.Max(0.05f, multiplier);
        }

        private static int GetTierCount()
        {
            return Enum.GetValues(typeof(LootTier)).Length;
        }

        private static int PickIndexByWeight(System.Random rng, float[] weights, float total)
        {
            if (rng == null || weights == null || weights.Length == 0) return -1;
            if (total <= 0f) return -1;

            float r = (float)(rng.NextDouble() * total);
            for (int i = 0; i < weights.Length; i++)
            {
                r -= Mathf.Max(0f, weights[i]);
                if (r <= 0f) return i;
            }
            return weights.Length - 1;
        }

        private static int RollInclusive(System.Random rng, int min, int max)
        {
            if (rng == null) return min;
            if (max < min) max = min;
            if (min == max) return min;
            return rng.Next(min, max + 1);
        }
    }
}
