using UnityEngine;
using WF.Gameplay.Core.Interfaces;

namespace WF.Gameplay.Systems.Inventory.Items
{
    public abstract class ConsumableItem : ItemBase, IConsumable, IStackable
    {
        [SerializeField] private int maxStack = 10;
        [SerializeField] private float cooldown = 1f;
        
        public int MaxStack => maxStack;
        public float Cooldown => cooldown;
        public int CurrentStack { get; set; }

        public virtual bool CanConsume(GameObject user) => true;
        
        public virtual void OnConsume(GameObject user) 
        {
            // Base consume logic
        }
        
        public bool CanStackWith(IItem other)
        {
            return other != null && other.ItemId == ItemId;
        }
    }
}
