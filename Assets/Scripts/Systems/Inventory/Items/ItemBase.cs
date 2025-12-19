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
        [SerializeField] private Sprite icon;
        [SerializeField] private float weight;
        [TextArea] [SerializeField] private string description;

        public string ItemId => itemId;
        public string DisplayName => displayName;
        public ItemType Type => type;
        public Sprite Icon => icon;
        public float Weight => weight;
        public string Description => description;

        public virtual bool CanUse(GameObject user) => false;
        public virtual void Use(GameObject user) { }
        
        protected virtual void OnValidate()
        {
            if (string.IsNullOrEmpty(itemId)) itemId = name;
        }
    }
}
