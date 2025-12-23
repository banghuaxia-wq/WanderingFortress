using UnityEngine;
using WF.Gameplay.Core.Data;

namespace WF.Gameplay.Systems.Inventory.Items
{
    [CreateAssetMenu(fileName = "NewQuestItem", menuName = "Items/Quest")]
    public class QuestItem : ItemBase
    {
        protected override void OnValidate()
        {
            base.OnValidate();
            SetType(ItemType.Quest);
        }
    }
}

