using UnityEngine;

public class CrosshairController : MonoBehaviour
{
    private const CursorLockMode GameCursorLockMode = CursorLockMode.Confined;

    [SerializeField] private RectTransform crosshairRect;
    [SerializeField] private Canvas parentCanvas;
    [SerializeField] private bool hideSystemCursor = true;

    private Camera _gameplayCamera;

    private void Awake()
    {
        if (crosshairRect == null)
        {
            crosshairRect = GetComponent<RectTransform>();
        }

        if (parentCanvas == null)
        {
            parentCanvas = GetComponentInParent<Canvas>();
        }
    }

    private void OnEnable()
    {
        AcquireGameplayCamera();
        EnsureCanvasCamera();
        ApplyCursorVisibility(true);
    }

    private void OnDisable()
    {
        ApplyCursorVisibility(false);
    }

    private void Update()
    {
        if (crosshairRect == null || parentCanvas == null)
        {
            return;
        }

        if (parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            if (_gameplayCamera == null)
            {
                AcquireGameplayCamera();
                EnsureCanvasCamera();
            }
        }

        if (parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            crosshairRect.position = Input.mousePosition;
        }
        else
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvas.transform as RectTransform,
                Input.mousePosition,
                parentCanvas.worldCamera,
                out Vector2 localPoint);

            crosshairRect.localPosition = localPoint;
        }
    }

    private void ApplyCursorVisibility(bool enableHide)
    {
        if (!hideSystemCursor)
        {
            return;
        }

        if (enableHide)
        {
            Cursor.visible = false;
            Cursor.lockState = GameCursorLockMode;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void AcquireGameplayCamera()
    {
        if (GameplayCameraProvider.TryGetGameplayCamera(out Camera camera))
        {
            _gameplayCamera = camera;
        }
        else
        {
            _gameplayCamera = Camera.main;
        }
    }

    private void EnsureCanvasCamera()
    {
        if (parentCanvas == null || parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            return;
        }

        if (_gameplayCamera != null && parentCanvas.worldCamera != _gameplayCamera)
        {
            parentCanvas.worldCamera = _gameplayCamera;
        }
    }
}

