using UnityEngine;
using WF.Gameplay.Systems.EventSystem;
using WF.Gameplay.Systems.Player;

namespace WF.Gameplay.Systems.InventorySystem
{
    public class WeightBuffSystem : MonoBehaviour
    {
        [SerializeField] private float overweightMultiplier = 0.7f;
        [SerializeField] private float overloadedMultiplier = 0f;
        private void OnEnable() { GameEvents.WeightChanged += OnWeightChanged; }
        private void OnDisable() { GameEvents.WeightChanged -= OnWeightChanged; }
        private void OnWeightChanged(float total, bool overweight, bool overloaded)
        {
            var psm = PlayerStateManager.Instance; if (psm == null) return;
            if (overloaded) { psm.SetMovementSpeedMultiplier("weight_overload", overloadedMultiplier); return; }
            if (overweight) { psm.SetMovementSpeedMultiplier("weight_overweight", overweightMultiplier); return; }
            psm.RemoveMovementSpeedMultiplier("weight_overweight");
            psm.RemoveMovementSpeedMultiplier("weight_overload");
        }
    }
}
