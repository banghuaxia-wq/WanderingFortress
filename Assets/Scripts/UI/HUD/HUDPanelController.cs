using UnityEngine;
using WF.Gameplay.Core.Events;
using WF.Gameplay.Systems.Player;

namespace WF.Gameplay.UI.HUD
{
    public class HUDPanelController : MonoBehaviour
    {
        [SerializeField] private GameObject packagePanel;
        [SerializeField] private GameObject boxPanel;
        [SerializeField] private KeyCode packageKey = KeyCode.I;
        [SerializeField] private Transform crosshair;
        private bool _modalOpen;

        private void Awake()
        {
            if (crosshair == null)
            {
                var t = transform.Find("crosshair");
                if (t != null) crosshair = t;
            }
        }

        private void Start()
        {
            SetPackage(false);
            SetBox(false);
        }

        private void OnEnable()
        {
            EventBus.Subscribe<ContainerOpenedEvent>(OnContainerOpened);
            EventBus.Subscribe<ContainerClosedEvent>(OnContainerClosed);
        }
        private void OnDisable()
        {
            EventBus.Unsubscribe<ContainerOpenedEvent>(OnContainerOpened);
            EventBus.Unsubscribe<ContainerClosedEvent>(OnContainerClosed);
        }

        private void Update()
        {
            if (Input.GetKeyDown(packageKey))
            {
                if (boxPanel != null && boxPanel.activeSelf)
                {
                    SetBox(false);
                    SetPackage(false);
                }
                else
                {
                    bool open = !(packagePanel != null && packagePanel.activeSelf);
                    SetPackage(open);
                }
            }
        }

        private void OnContainerOpened(ContainerOpenedEvent e)
        {
            SetPackage(true);
            SetBox(true);
        }
        private void OnContainerClosed(ContainerClosedEvent e)
        {
            SetBox(false);
        }

        private void SetPackage(bool open)
        {
            if (packagePanel != null) packagePanel.SetActive(open);
            UpdateModalState();
        }

        private void SetBox(bool open)
        {
            if (boxPanel != null) boxPanel.SetActive(open);
            UpdateModalState();
        }

        private void UpdateModalState()
        {
            bool anyOpen = (packagePanel != null && packagePanel.activeSelf) || (boxPanel != null && boxPanel.activeSelf);
            if (_modalOpen == anyOpen) return;
            _modalOpen = anyOpen;
            if (crosshair != null) crosshair.gameObject.SetActive(!anyOpen);
            Cursor.visible = anyOpen;
            Cursor.lockState = anyOpen ? CursorLockMode.None : CursorLockMode.Confined;
            
            EventBus.Publish(new InputStateChangedEvent(!anyOpen));
        }
    }
}
