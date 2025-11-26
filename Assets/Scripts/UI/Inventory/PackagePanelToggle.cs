using UnityEngine;

namespace WF.Gameplay.UI.Inventory
{
    public class PackagePanelToggle : MonoBehaviour
    {
        [SerializeField] private KeyCode toggleKey = KeyCode.I;
        private bool _disabled;
        private void Awake()
        {
            var hud = FindObjectOfType<WF.Gameplay.UI.HUD.HUDPanelController>();
            if (hud != null) { enabled = false; _disabled = true; }
        }
        private void Update()
        {
            if (_disabled) return;
            if (Input.GetKeyDown(toggleKey))
            {
                gameObject.SetActive(!gameObject.activeSelf);
            }
        }
    }
}
