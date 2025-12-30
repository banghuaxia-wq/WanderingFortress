using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using WF.Gameplay.Core.Data;
using WF.Gameplay.Core.Interfaces;
using WF.Gameplay.Systems.Inventory.Items;
using WF.Gameplay.Systems.Inventory.Items.Weapons;
using WeaponItemSO = WF.Gameplay.Systems.Inventory.Items.Weapons.WeaponItem;

namespace WF.Gameplay.Systems.Inventory
{
    public class ItemFactory : MonoBehaviour
    {
        public static ItemFactory Instance { get; private set; }

        private static readonly Dictionary<string, ItemBase> _itemCache = new Dictionary<string, ItemBase>();
#if UNITY_EDITOR
        private static Dictionary<string, ItemBase> _editorItemIdIndex;
#endif

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public static ItemStack CreateItemStack(ItemBase item, int count)
        {
            if (item == null) return null;
            return ItemStack.Create(item, count);
        }

        public static ItemStack CreateItemStack(string itemId, int count)
        {
            if (string.IsNullOrEmpty(itemId)) return null;

            if (_itemCache.TryGetValue(itemId, out var cached) && cached != null)
            {
                return ItemStack.Create(cached, count);
            }

            var item = LoadAddressableAsset<ItemBase>(itemId);

            if (item != null)
            {
                _itemCache[itemId] = item;
                return ItemStack.Create(item, count);
            }

            return null;
        }

        public IWeaponItem CreateWeapon(WeaponItemSO template)
        {
            if (template == null) return null;

            var instance = Instantiate(template);
            instance.name = template.name;
            
            return instance;
        }

        public IWeaponItem CreateWeapon(string addressKey)
        {
            var template = LoadAddressableAsset<WeaponItemSO>(addressKey);
            if (template == null)
            {
                Debug.LogError($"Weapon template not found at {addressKey}");
                return null;
            }
            return CreateWeapon(template);
        }

        private static T LoadAddressableAsset<T>(string addressKey) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(addressKey)) return null;

            try
            {
                var addressablesType =
                    Type.GetType("UnityEngine.AddressableAssets.Addressables, Unity.Addressables")
                    ?? FindTypeInLoadedAssemblies("UnityEngine.AddressableAssets.Addressables");

                if (addressablesType == null)
                {
                    return Resources.Load<T>(addressKey) ?? TryLoadFromEditorIndex<T>(addressKey);
                }

                var loadAssetAsync = FindLoadAssetAsyncGeneric(addressablesType);
                if (loadAssetAsync == null)
                {
                    return Resources.Load<T>(addressKey) ?? TryLoadFromEditorIndex<T>(addressKey);
                }

                var generic = loadAssetAsync.MakeGenericMethod(typeof(T));
                var handle = generic.Invoke(null, new object[] { addressKey });
                if (handle == null)
                {
                    return Resources.Load<T>(addressKey) ?? TryLoadFromEditorIndex<T>(addressKey);
                }

                var handleType = handle.GetType();
                var waitForCompletion = handleType.GetMethod("WaitForCompletion", BindingFlags.Public | BindingFlags.Instance);
                if (waitForCompletion != null)
                {
                    var loaded = waitForCompletion.Invoke(handle, null) as T;
                    return loaded != null ? loaded : (Resources.Load<T>(addressKey) ?? TryLoadFromEditorIndex<T>(addressKey));
                }

                var resultProp = handleType.GetProperty("Result", BindingFlags.Public | BindingFlags.Instance);
                var result = resultProp?.GetValue(handle) as T;
                return result != null ? result : (Resources.Load<T>(addressKey) ?? TryLoadFromEditorIndex<T>(addressKey));
            }
            catch
            {
                return Resources.Load<T>(addressKey) ?? TryLoadFromEditorIndex<T>(addressKey);
            }
        }

        private static MethodInfo FindLoadAssetAsyncGeneric(Type addressablesType)
        {
            var methods = addressablesType.GetMethods(BindingFlags.Public | BindingFlags.Static);
            for (int i = 0; i < methods.Length; i++)
            {
                var m = methods[i];
                if (!string.Equals(m.Name, "LoadAssetAsync", StringComparison.Ordinal)) continue;
                if (!m.IsGenericMethodDefinition) continue;
                var ps = m.GetParameters();
                if (ps.Length != 1) continue;
                return m;
            }
            return null;
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

        private static T TryLoadFromEditorIndex<T>(string addressKey) where T : UnityEngine.Object
        {
#if UNITY_EDITOR
            if (typeof(T) != typeof(ItemBase)) return null;
            if (string.IsNullOrEmpty(addressKey)) return null;

            if (_editorItemIdIndex == null || _editorItemIdIndex.Count == 0)
            {
                RebuildEditorItemIdIndex();
            }

            if (_editorItemIdIndex != null && _editorItemIdIndex.TryGetValue(addressKey, out var item) && item != null)
            {
                return item as T;
            }
#endif
            return null;
        }

#if UNITY_EDITOR
        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            _editorItemIdIndex = null;
        }

        private static void RebuildEditorItemIdIndex()
        {
            var index = new Dictionary<string, ItemBase>(StringComparer.Ordinal);
            var guids = AssetDatabase.FindAssets("t:WF.Gameplay.Systems.Inventory.Items.ItemBase");
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (string.IsNullOrEmpty(path)) continue;
                var item = AssetDatabase.LoadAssetAtPath<ItemBase>(path);
                if (item == null) continue;
                if (string.IsNullOrEmpty(item.ItemId)) continue;
                if (!index.ContainsKey(item.ItemId)) index.Add(item.ItemId, item);
            }
            _editorItemIdIndex = index;
        }
#endif
    }
}
