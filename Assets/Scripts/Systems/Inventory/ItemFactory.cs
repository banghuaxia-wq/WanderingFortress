using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
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
                var addressablesType = Type.GetType("UnityEngine.AddressableAssets.Addressables, Unity.Addressables");
                if (addressablesType == null) return null;

                var loadAssetAsync = FindLoadAssetAsyncGeneric(addressablesType);
                if (loadAssetAsync == null) return null;

                var generic = loadAssetAsync.MakeGenericMethod(typeof(T));
                var handle = generic.Invoke(null, new object[] { addressKey });
                if (handle == null) return null;

                var handleType = handle.GetType();
                var waitForCompletion = handleType.GetMethod("WaitForCompletion", BindingFlags.Public | BindingFlags.Instance);
                if (waitForCompletion != null)
                {
                    return waitForCompletion.Invoke(handle, null) as T;
                }

                var resultProp = handleType.GetProperty("Result", BindingFlags.Public | BindingFlags.Instance);
                return resultProp?.GetValue(handle) as T;
            }
            catch
            {
                return null;
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
    }
}
