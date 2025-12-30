using UnityEngine;
using WF.Gameplay.Core.Data;

namespace WF.Gameplay.Systems.Inventory.Items
{
    [CreateAssetMenu(fileName = "NewToolItem", menuName = "Items/Tool")]
    public class ToolItem : EquipmentItem
    {
        protected override void OnValidate()
        {
            base.OnValidate();
            SetDefaultType(ItemType.Tool);
        }
    }
}
