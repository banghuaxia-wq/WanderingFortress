using UnityEngine;
using WF.Gameplay.Systems.EventSystem;
using WF.Gameplay.Systems.Player;

namespace WF.Gameplay.UI.HUD
{
    public class HUDPanelController : MonoBehaviour
    {
        [SerializeField] private GameObject packagePanel;
        [SerializeField] private GameObject boxPanel;
        [SerializeField] private KeyCode packageKey = KeyCode.I;
        [SerializeField] private Transform crosshair;
        private PlayerMove _move;
        private PlayerShooter _shooter;
        private bool _modalOpen;

        private void Awake()
        {
            _move = FindObjectOfType<PlayerMove>();
            _shooter = FindObjectOfType<PlayerShooter>();
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
            GameEvents.ContainerOpened += OnContainerOpened;
            GameEvents.ContainerClosed += OnContainerClosed;
        }
        private void OnDisable()
        {
            GameEvents.ContainerOpened -= OnContainerOpened;
            GameEvents.ContainerClosed -= OnContainerClosed;
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

        private void OnContainerOpened(WF.Gameplay.Core.Data.ContainerData d)
        {
            SetPackage(true);
            SetBox(true);
        }
        private void OnContainerClosed(WF.Gameplay.Core.Data.ContainerData d)
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
            Cursor.lockState = CursorLockMode.None;
            if (_move != null) _move.enabled = !anyOpen;
            if (_shooter != null) _shooter.enabled = !anyOpen;
        }
    }
}
