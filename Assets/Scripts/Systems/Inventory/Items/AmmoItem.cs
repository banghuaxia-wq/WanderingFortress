using UnityEngine;
using WF.Gameplay.Core.Interfaces;

namespace WF.Gameplay.Systems.Inventory.Items
{
    [CreateAssetMenu(fileName = "NewAmmoItem", menuName = "Items/Ammo")]
    public class AmmoItem : ItemBase, IConsumable, IStackable
    {
        [SerializeField] private int maxStack = 99;
        [SerializeField] private float cooldown = 0f;
        
        public int MaxStack => maxStack;
        public float Cooldown => cooldown;
        
        public int CurrentStack { get; set; }

        public bool CanConsume(GameObject user) => true;
        
        public void OnConsume(GameObject user) 
        {
            // Logic for consuming ammo
        }
        
        public bool CanStackWith(IItem other)
        {
            return other != null && other.ItemId == ItemId;
        }
    }
}
