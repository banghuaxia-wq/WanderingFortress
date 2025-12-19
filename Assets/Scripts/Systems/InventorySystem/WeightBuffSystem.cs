using UnityEngine;
using WF.Gameplay.Core.Events;
using WF.Gameplay.Systems.Player;

namespace WF.Gameplay.Systems.InventorySystem
{
    public class WeightBuffSystem : MonoBehaviour
    {
        [SerializeField] private float overweightMultiplier = 0.7f;
        [SerializeField] private float overloadedMultiplier = 0f;
        
        private void OnEnable() 
        { 
            EventBus.Subscribe<WeightChangedEvent>(OnWeightChanged); 
        }
        
        private void OnDisable() 
        { 
            EventBus.Unsubscribe<WeightChangedEvent>(OnWeightChanged); 
        }
        
        private void OnWeightChanged(WeightChangedEvent e)
        {
            var psm = PlayerStateManager.Instance; if (psm == null) return;
            if (e.IsOverloaded) { psm.SetMovementSpeedMultiplier("weight_overload", overloadedMultiplier); return; }
            if (e.IsOverweight) { psm.SetMovementSpeedMultiplier("weight_overweight", overweightMultiplier); return; }
            psm.RemoveMovementSpeedMultiplier("weight_overweight");
            psm.RemoveMovementSpeedMultiplier("weight_overload");
        }
    }
}
