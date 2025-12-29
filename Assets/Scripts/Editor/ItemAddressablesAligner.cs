using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using WF.Gameplay.Systems.Inventory.Items;

namespace WF.Gameplay.Editor
{
    public static class ItemAddressablesAligner
    {
        [MenuItem("WF/Addressables/Align Selected ItemBase Address To ItemId")]
        private static void AlignSelected()
        {
            if (!TryGetAddressablesSettings(out var settings))
            {
                EditorUtility.DisplayDialog("WF", "未找到 AddressableAssetSettings。\n请先打开 Addressables Groups 并创建设置：Window > Asset Management > Addressables > Groups", "OK");
                return;
            }

            var items = CollectSelectedItems();
            if (items.Count == 0)
            {
                EditorUtility.DisplayDialog("WF", "未选中任何 ItemBase 资源或包含 ItemBase 的文件夹。", "OK");
                return;
            }

            int updated = 0;
            int created = 0;
            int skipped = 0;

            var duplicatedInSelection = new HashSet<string>();
            var seenAddress = new Dictionary<string, string>(StringComparer.Ordinal);

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item == null) { skipped++; continue; }

                var assetPath = AssetDatabase.GetAssetPath(item);
                if (string.IsNullOrEmpty(assetPath)) { skipped++; continue; }

                string guid = AssetDatabase.AssetPathToGUID(assetPath);
                if (string.IsNullOrEmpty(guid)) { skipped++; continue; }

                string desiredAddress = string.IsNullOrEmpty(item.ItemId) ? item.name : item.ItemId;

                if (seenAddress.TryGetValue(desiredAddress, out var otherGuid) && !string.Equals(otherGuid, guid, StringComparison.Ordinal))
                {
                    duplicatedInSelection.Add(desiredAddress);
                }
                else
                {
                    seenAddress[desiredAddress] = guid;
                }

                if (!TryGetOrCreateEntry(settings, guid, out var entry, out bool wasCreated))
                {
                    skipped++;
                    continue;
                }

                if (wasCreated) created++;

                if (TrySetEntryAddress(entry, desiredAddress))
                {
                    updated++;
                }
                else
                {
                    skipped++;
                }
            }

            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();

            string msg = $"处理 {items.Count} 个 ItemBase\n更新: {updated}\n新建Entry: {created}\n跳过: {skipped}";
            if (duplicatedInSelection.Count > 0)
            {
                msg += "\n\n检测到选中范围内 Address 重复：\n" + string.Join("\n", duplicatedInSelection);
            }

            EditorUtility.DisplayDialog("WF", msg, "OK");
        }

        [MenuItem("WF/Addressables/Align Selected ItemBase Address To ItemId", true)]
        private static bool AlignSelectedValidate()
        {
            var items = CollectSelectedItems();
            return items.Count > 0;
        }

        private static List<ItemBase> CollectSelectedItems()
        {
            var results = new List<ItemBase>();
            var seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var objs = Selection.objects;
            for (int i = 0; i < objs.Length; i++)
            {
                var obj = objs[i];
                if (obj == null) continue;

                var path = AssetDatabase.GetAssetPath(obj);
                if (string.IsNullOrEmpty(path)) continue;

                if (AssetDatabase.IsValidFolder(path))
                {
                    var guids = AssetDatabase.FindAssets("t:WF.Gameplay.Systems.Inventory.Items.ItemBase", new[] { path });
                    for (int g = 0; g < guids.Length; g++)
                    {
                        string p = AssetDatabase.GUIDToAssetPath(guids[g]);
                        if (string.IsNullOrEmpty(p)) continue;
                        if (!seenPaths.Add(p)) continue;
                        var asset = AssetDatabase.LoadAssetAtPath<ItemBase>(p);
                        if (asset != null) results.Add(asset);
                    }
                    continue;
                }

                if (!seenPaths.Add(path)) continue;

                var item = AssetDatabase.LoadAssetAtPath<ItemBase>(path);
                if (item != null) results.Add(item);
            }

            return results;
        }

        private static bool TryGetAddressablesSettings(out UnityEngine.Object settings)
        {
            settings = null;

            var defaultObjectType = FindTypeInLoadedAssemblies("UnityEditor.AddressableAssets.Settings.AddressableAssetSettingsDefaultObject");
            if (defaultObjectType != null)
            {
                var settingsProp = defaultObjectType.GetProperty("Settings", BindingFlags.Public | BindingFlags.Static);
                if (settingsProp != null)
                {
                    settings = settingsProp.GetValue(null) as UnityEngine.Object;
                    if (settings != null) return true;
                }
            }

            const string defaultSettingsPath = "Assets/AddressableAssetsData/AddressableAssetSettings.asset";
            settings = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(defaultSettingsPath);
            return settings != null;
        }

        private static Type FindTypeInLoadedAssemblies(string fullName)
        {
            if (string.IsNullOrEmpty(fullName)) return null;

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                var asm = assemblies[i];
                if (asm == null) continue;
                var t = asm.GetType(fullName, false);
                if (t != null) return t;
            }

            return null;
        }

        private static bool TryGetOrCreateEntry(UnityEngine.Object settings, string guid, out object entry, out bool created)
        {
            entry = null;
            created = false;

            var settingsType = settings.GetType();
            var findMethod = settingsType.GetMethod("FindAssetEntry", BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(string) }, null);
            if (findMethod == null) return false;

            entry = findMethod.Invoke(settings, new object[] { guid });
            if (entry != null) return true;

            var defaultGroupProp = settingsType.GetProperty("DefaultGroup", BindingFlags.Public | BindingFlags.Instance);
            var defaultGroup = defaultGroupProp?.GetValue(settings);
            if (defaultGroup == null) return false;

            var createMethod = FindCreateOrMoveEntry(settingsType);
            if (createMethod == null) return false;

            var ps = createMethod.GetParameters();
            object[] args;
            if (ps.Length == 2)
            {
                args = new[] { guid, defaultGroup };
            }
            else
            {
                args = new[] { guid, defaultGroup, (object)false };
            }

            entry = createMethod.Invoke(settings, args);
            created = entry != null;
            return entry != null;
        }

        private static MethodInfo FindCreateOrMoveEntry(Type settingsType)
        {
            var methods = settingsType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < methods.Length; i++)
            {
                var m = methods[i];
                if (!string.Equals(m.Name, "CreateOrMoveEntry", StringComparison.Ordinal)) continue;
                var ps = m.GetParameters();
                if (ps.Length == 2 && ps[0].ParameterType == typeof(string)) return m;
                if (ps.Length == 3 && ps[0].ParameterType == typeof(string)) return m;
            }
            return null;
        }

        private static bool TrySetEntryAddress(object entry, string address)
        {
            if (entry == null) return false;
            var entryType = entry.GetType();

            var prop = entryType.GetProperty("address", BindingFlags.Public | BindingFlags.Instance)
                       ?? entryType.GetProperty("Address", BindingFlags.Public | BindingFlags.Instance);
            if (prop == null || !prop.CanWrite) return false;

            prop.SetValue(entry, address);
            return true;
        }
    }
}
