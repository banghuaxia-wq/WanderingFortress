using UnityEngine;
using WF.Gameplay.Core.Data;
using WF.Gameplay.Core.Interfaces;

namespace WF.Gameplay.Systems.Inventory.Items
{
    public abstract class ItemBase : ScriptableObject, IItem
    {
        [SerializeField] private string itemId;
        [SerializeField] private string displayName;
        [SerializeField] private ItemType type;
        [SerializeField] private ItemTag tags;
        [SerializeField] private ItemRarity rarity = ItemRarity.Common;
        [SerializeField] private Sprite icon;
        [SerializeField] private float weight;
        [TextArea] [SerializeField] private string description;

        public string ItemId => itemId;
        public string DisplayName => displayName;
        public ItemType Type => type;
        public ItemTag Tags => tags;
        public ItemRarity Rarity => rarity;
        public Sprite Icon => icon;
        public float Weight => weight;
        public string Description => description;

        public virtual bool CanUse(GameObject user) => false;
        public virtual void Use(GameObject user) { }

        protected void SetType(ItemType value)
        {
            type = value;
            tags |= MapTypeToTags(value);
        }

        public bool HasTag(ItemTag tag)
        {
            if (tag == ItemTag.None) return tags == ItemTag.None;
            return (tags & tag) == tag;
        }

        protected virtual void OnValidate()
        {
            if (string.IsNullOrEmpty(itemId)) itemId = name;
        }

        private static ItemTag MapTypeToTags(ItemType itemType)
        {
            switch (itemType)
            {
                case ItemType.Consumable:
                    return ItemTag.Consumable;
                case ItemType.Tool:
                    return ItemTag.Tool;
                case ItemType.Weapon:
                    return ItemTag.Weapon;
                case ItemType.Armor:
                    return ItemTag.Armor;
                case ItemType.Material:
                    return ItemTag.Material;
                case ItemType.Quest:
                    return ItemTag.Quest;
                case ItemType.Ammo:
                    return ItemTag.Ammo;
                case ItemType.Throwable:
                    return ItemTag.Throwable;
                default:
                    return ItemTag.None;
            }
        }
    }
}
