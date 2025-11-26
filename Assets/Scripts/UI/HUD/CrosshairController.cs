using UnityEngine;

/// <summary>
/// 控制游戏中的准星UI，使其跟随鼠标位置，并管理系统光标的可见性。
/// </summary>
using WF.Gameplay.Systems.Camera;
using UCamera = UnityEngine.Camera;

namespace WF.Gameplay.UI.HUD
{
public class CrosshairController : MonoBehaviour
{
    // 游戏进行时，限制光标在游戏窗口内。
    private const CursorLockMode GameCursorLockMode = CursorLockMode.Confined;

    [Tooltip("准星的 RectTransform 组件。")]
    [SerializeField] private RectTransform crosshairRect;
    [Tooltip("父 Canvas 组件，用于坐标转换。")]
    [SerializeField] private Canvas parentCanvas;
    [Tooltip("是否在游戏时隐藏系统光标。")]
    [SerializeField] private bool hideSystemCursor = true;
    [SerializeField] private Vector2 externalOffset;

    // 缓存的游戏主摄像机。
    private UCamera _gameplayCamera;

    /// <summary>
    /// 初始化组件引用。
    /// </summary>
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

    /// <summary>
    /// 当此组件启用时，设置摄像机和光标状态。
    /// </summary>
    private void OnEnable()
    {
        AcquireGameplayCamera();
        EnsureCanvasCamera();
        ApplyCursorVisibility(true);
    }

    /// <summary>
    /// 当此组件禁用时，恢复光标状态。
    /// </summary>
    private void OnDisable()
    {
        ApplyCursorVisibility(false);
    }

    /// <summary>
    /// 每帧更新准星的位置。
    /// </summary>
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
            Vector3 p = Input.mousePosition;
            p.x += externalOffset.x;
            p.y += externalOffset.y;
            crosshairRect.position = p;
        }
        else
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvas.transform as RectTransform,
                new Vector2(Input.mousePosition.x + externalOffset.x, Input.mousePosition.y + externalOffset.y),
                parentCanvas.worldCamera,
                out Vector2 localPoint);

            crosshairRect.localPosition = localPoint;
        }
    }

    /// <summary>
    /// 根据需要应用或恢复系统光标的可见性。
    /// </summary>
    /// <param name="enableHide">如果为 true，则隐藏光标；否则显示光标。</param>
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

    /// <summary>
    /// 获取并缓存游戏主摄像机。
    /// </summary>
    private void AcquireGameplayCamera()
    {
        if (GameplayCameraProvider.TryGetGameplayCamera(out UCamera camera))
        {
            _gameplayCamera = camera;
        }
        else
        {
            _gameplayCamera = UCamera.main;
        }
    }

    /// <summary>
    /// 确保 Canvas 的 worldCamera 已正确设置。
    /// </summary>
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

    public void SetExternalOffset(Vector2 offset)
    {
        externalOffset = offset;
    }
}
}

