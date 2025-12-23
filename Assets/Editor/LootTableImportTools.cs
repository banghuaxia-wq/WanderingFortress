using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using WF.Gameplay.Core.Data;

public static class LootTableImportTools
{
    [Serializable]
    private class LootTableJson
    {
        // 掉落表ID（空则回退使用JSON文件名）
        public string tableId;
        public int minRolls = 1;
        public int maxRolls = 3;
        public bool allowDuplicateItems = true;
        public float minEffectiveWeight = 0.01f;
        public int maxNestingDepth = 8;
        public float jackpotChance = 0f;
        // JackpotTable 的资产路径（可选）
        public string jackpotTableAssetPath;
        public LootEntryJson[] entries;
    }

    [Serializable]
    private class LootEntryJson
    {
        // 分层（Common/Uncommon/Rare/Epic/Legendary）
        public string tier;
        // 条目类型（Item/Table）
        public string kind;
        // 物品ID（仅 Kind=Item）
        public string itemId;
        // 子掉落表资产路径（仅 Kind=Table）
        public string tableAssetPath;
        public float baseWeight = 1f;
        public float luckSensitivity = 0f;
        public int minCount = 1;
        public int maxCount = 1;
    }

    [MenuItem("Tools/Loot/Import LootTable From Selected JSON", priority = 20)]
    // 从选中的 JSON TextAsset 导入/更新 LootTableSO
    private static void ImportSelectedJson()
    {
        var text = Selection.activeObject as TextAsset;
        if (text == null)
        {
            Debug.LogWarning("[Loot] Select a JSON TextAsset to import.");
            return;
        }

        LootTableJson parsed;
        try
        {
            parsed = JsonUtility.FromJson<LootTableJson>(text.text);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[Loot] JSON parse failed: {ex.Message}");
            return;
        }

        if (parsed == null)
        {
            Debug.LogWarning("[Loot] JSON parse returned null.");
            return;
        }

        var jsonPath = AssetDatabase.GetAssetPath(text);
        var dir = Path.GetDirectoryName(jsonPath)?.Replace('\\', '/') ?? "Assets";
        var assetName = string.IsNullOrEmpty(parsed.tableId) ? text.name : parsed.tableId;
        var targetPath = $"{dir}/{assetName}.asset";

        var table = AssetDatabase.LoadAssetAtPath<LootTableSO>(targetPath);
        if (table == null)
        {
            table = ScriptableObject.CreateInstance<LootTableSO>();
            AssetDatabase.CreateAsset(table, AssetDatabase.GenerateUniqueAssetPath(targetPath));
        }

        ApplyLootTable(table, parsed);
        EditorUtility.SetDirty(table);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[Loot] Imported LootTable: {AssetDatabase.GetAssetPath(table)}");
    }

    // 写入 ScriptableObject（通过 SerializedObject 设置私有序列化字段）
    private static void ApplyLootTable(LootTableSO table, LootTableJson parsed)
    {
        var so = new SerializedObject(table);
        so.Update();

        SetIfExists(so, "tableId", parsed.tableId);
        SetIfExists(so, "minRolls", parsed.minRolls);
        SetIfExists(so, "maxRolls", parsed.maxRolls);
        SetIfExists(so, "allowDuplicateItems", parsed.allowDuplicateItems);
        SetIfExists(so, "minEffectiveWeight", parsed.minEffectiveWeight);
        SetIfExists(so, "maxNestingDepth", parsed.maxNestingDepth);
        SetIfExists(so, "jackpotChance", parsed.jackpotChance);

        var jackpotProp = so.FindProperty("jackpotTable");
        if (jackpotProp != null)
        {
            LootTableSO jackpot = null;
            if (!string.IsNullOrEmpty(parsed.jackpotTableAssetPath))
            {
                jackpot = AssetDatabase.LoadAssetAtPath<LootTableSO>(parsed.jackpotTableAssetPath);
            }
            jackpotProp.objectReferenceValue = jackpot;
        }

        var entriesProp = so.FindProperty("entries");
        if (entriesProp != null)
        {
            var src = parsed.entries ?? Array.Empty<LootEntryJson>();
            entriesProp.arraySize = src.Length;
            for (int i = 0; i < src.Length; i++)
            {
                var e = src[i];
                var p = entriesProp.GetArrayElementAtIndex(i);
                if (p == null) continue;

                SetEnum(p, "Tier", ParseEnum(e.tier, LootTier.Common));
                SetEnum(p, "Kind", ParseEnum(e.kind, LootEntryKind.Item));
                SetString(p, "ItemId", e.itemId);
                SetFloat(p, "BaseWeight", e.baseWeight);
                SetFloat(p, "LuckSensitivity", e.luckSensitivity);
                SetInt(p, "MinCount", Mathf.Max(1, e.minCount));
                SetInt(p, "MaxCount", Mathf.Max(1, e.maxCount));

                var tableProp = p.FindPropertyRelative("Table");
                if (tableProp != null)
                {
                    LootTableSO sub = null;
                    if (!string.IsNullOrEmpty(e.tableAssetPath))
                    {
                        sub = AssetDatabase.LoadAssetAtPath<LootTableSO>(e.tableAssetPath);
                    }
                    tableProp.objectReferenceValue = sub;
                }
            }
        }

        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // 解析枚举（忽略大小写，失败回退默认值）
    private static T ParseEnum<T>(string s, T fallback) where T : struct
    {
        if (string.IsNullOrEmpty(s)) return fallback;
        if (Enum.TryParse<T>(s, true, out var v)) return v;
        return fallback;
    }

    // 若字段存在则赋值（字符串）
    private static void SetIfExists(SerializedObject so, string name, string value)
    {
        var p = so.FindProperty(name);
        if (p != null && p.propertyType == SerializedPropertyType.String) p.stringValue = value;
    }

    // 若字段存在则赋值（整数）
    private static void SetIfExists(SerializedObject so, string name, int value)
    {
        var p = so.FindProperty(name);
        if (p != null && p.propertyType == SerializedPropertyType.Integer) p.intValue = value;
    }

    // 若字段存在则赋值（布尔）
    private static void SetIfExists(SerializedObject so, string name, bool value)
    {
        var p = so.FindProperty(name);
        if (p != null && p.propertyType == SerializedPropertyType.Boolean) p.boolValue = value;
    }

    // 若字段存在则赋值（浮点）
    private static void SetIfExists(SerializedObject so, string name, float value)
    {
        var p = so.FindProperty(name);
        if (p != null && p.propertyType == SerializedPropertyType.Float) p.floatValue = value;
    }

    // 设置枚举字段（子属性）
    private static void SetEnum(SerializedProperty parent, string name, Enum value)
    {
        var p = parent.FindPropertyRelative(name);
        if (p != null && p.propertyType == SerializedPropertyType.Enum) p.enumValueIndex = Convert.ToInt32(value);
    }

    // 设置字符串字段（子属性）
    private static void SetString(SerializedProperty parent, string name, string value)
    {
        var p = parent.FindPropertyRelative(name);
        if (p != null && p.propertyType == SerializedPropertyType.String) p.stringValue = value;
    }

    // 设置浮点字段（子属性）
    private static void SetFloat(SerializedProperty parent, string name, float value)
    {
        var p = parent.FindPropertyRelative(name);
        if (p != null && p.propertyType == SerializedPropertyType.Float) p.floatValue = value;
    }

    // 设置整数字段（子属性）
    private static void SetInt(SerializedProperty parent, string name, int value)
    {
        var p = parent.FindPropertyRelative(name);
        if (p != null && p.propertyType == SerializedPropertyType.Integer) p.intValue = value;
    }
}
