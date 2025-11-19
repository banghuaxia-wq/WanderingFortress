using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 管理敌人的状态UI，包括血条、眩晕条和提示信息，并使其跟随目标在屏幕上移动。
/// </summary>
public class EnemyStatusUI : MonoBehaviour
{
    // 默认的自动隐藏延迟时间
    private const float DefaultHideDelay = 3f;
    // 最小的自动隐藏延迟时间
    private const float MinHideDelay = 0.1f;
    // UI元素与摄像机的最小距离，小于此距离则隐藏
    private const float MinDistanceToCamera = 0.01f;

    [Tooltip("UI的根RectTransform")]
    [SerializeField] private RectTransform rootTransform;
    [Tooltip("显示生命值的滑动条")]
    [SerializeField] private Slider healthSlider;
    [Tooltip("显示眩晕值的滑动条")]
    [SerializeField] private Slider stunSlider;
    [Tooltip("用于显示提示信息的文本框")]
    [SerializeField] private Text tooltipText;
    [Tooltip("UI在目标头顶的偏移量")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2.2f, 0f);
    [Tooltip("状态栏显示后自动隐藏的延迟时间")]
    [SerializeField] private float hideDelay = DefaultHideDelay;
    [Tooltip("控制UI整体透明度的CanvasGroup")]
    [SerializeField] private CanvasGroup canvasGroup;
    [Tooltip("如果为true，状态栏将始终可见")]
    [SerializeField] private bool alwaysVisible = false;

    // UI跟随的目标
    private Transform _target;
    // 主摄像机
    private Camera _camera;
    // 自动隐藏的计时器
    private float _hideTimer;
    // 是否强制显示（例如，当显示工具提示时）
    private bool _isForcedVisible;
    // 父Canvas组件
    private Canvas _parentCanvas;
    // 父Canvas是否为世界空间模式
    private bool _isWorldSpaceCanvas;

    /// <summary>
    /// 初始化组件引用和Canvas模式。
    /// </summary>
    private void Awake()
    {
        if (rootTransform == null)
        {
            rootTransform = GetComponent<RectTransform>();
        }

        if (rootTransform != null)
        {
            _parentCanvas = rootTransform.GetComponentInParent<Canvas>();
            if (_parentCanvas != null)
            {
                _isWorldSpaceCanvas = _parentCanvas.renderMode == RenderMode.WorldSpace;
            }
        }

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        if (tooltipText != null)
        {
            tooltipText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 当组件启用时，获取摄像机引用。
    /// </summary>
    private void OnEnable()
    {
        AcquireCamera();
    }

    /// <summary>
    /// 在每帧的最后更新UI位置和可见性。
    /// </summary>
    private void LateUpdate()
    {
        if (_target == null || rootTransform == null)
        {
            return;
        }

        if (_camera == null)
        {
            AcquireCamera();
            if (_camera == null)
            {
                return;
            }
        }

        Vector3 worldPosition = _target.position + worldOffset;

        bool hasCanvasGroup = canvasGroup != null;

        if (_isWorldSpaceCanvas)
        {
            rootTransform.position = worldPosition;
            if (_camera != null)
            {
                rootTransform.forward = _camera.transform.forward;
            }
        }
        else
        {
            Vector3 screenPoint = _camera.WorldToScreenPoint(worldPosition);

            if (hasCanvasGroup && screenPoint.z < MinDistanceToCamera)
            {
                canvasGroup.alpha = 0f;
                return;
            }

            rootTransform.position = screenPoint;
        }

        if (!_isWorldSpaceCanvas && hasCanvasGroup && canvasGroup.alpha == 0f && _isForcedVisible)
        {
            canvasGroup.alpha = 1f;
        }

        if (!_isForcedVisible && hasCanvasGroup && canvasGroup.alpha > 0f && !alwaysVisible)
        {
            _hideTimer -= Time.deltaTime;
            if (_hideTimer <= 0f)
            {
                HideStatusImmediate();
            }
        }
    }

    /// <summary>
    /// 绑定 UI 到指定目标。
    /// </summary>
    /// <param name="target">要跟随的目标</param>
    public void AttachToTarget(Transform target)
    {
        _target = target;
        AcquireCamera();
    }

    /// <summary>
    /// 配置血条和眩晕条的最大值。
    /// </summary>
    /// <param name="maxHealth">最大生命值</param>
    /// <param name="maxStun">最大眩晕值</param>
    public void ConfigureBars(float maxHealth, float maxStun)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
        }

        if (stunSlider != null)
        {
            stunSlider.maxValue = maxStun;
        }
    }

    /// <summary>
    /// 更新血条的当前值。
    /// </summary>
    /// <param name="currentHealth">当前生命值</param>
    public void UpdateHealthBar(float currentHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
    }

    /// <summary>
    /// 更新眩晕条的当前值。
    /// </summary>
    /// <param name="currentStun">当前眩晕值</param>
    public void UpdateStunBar(float currentStun)
    {
        if (stunSlider != null)
        {
            stunSlider.value = currentStun;
        }
    }

    /// <summary>
    /// 显示状态栏并重新计时。
    /// </summary>
    public void ShowStatus()
    {
        if (canvasGroup == null)
        {
            return;
        }

        canvasGroup.alpha = 1f;
        float delay = Mathf.Max(hideDelay, MinHideDelay);
        _hideTimer = delay;

        if (alwaysVisible)
        {
            _hideTimer = float.MaxValue;
        }
    }

    /// <summary>
    /// 立即隐藏状态栏。
    /// </summary>
    public void HideStatusImmediate()
    {
        if (canvasGroup == null)
        {
            return;
        }

        if (alwaysVisible)
        {
            canvasGroup.alpha = 1f;
            return;
        }

        canvasGroup.alpha = 0f;
        _hideTimer = 0f;
    }

    /// <summary>
    /// 显示提示文本。
    /// </summary>
    /// <param name="message">要显示的提示信息</param>
    public void ShowTooltip(string message)
    {
        if (tooltipText == null)
        {
            return;
        }

        tooltipText.text = message;
        tooltipText.gameObject.SetActive(true);
        _isForcedVisible = true;
        ShowStatus();
    }

    /// <summary>
    /// 隐藏提示文本。
    /// </summary>
    public void HideTooltip()
    {
        if (tooltipText == null)
        {
            return;
        }

        tooltipText.gameObject.SetActive(false);
        _isForcedVisible = false;
    }

    /// <summary>
    /// 获取场景中的主摄像机。
    /// </summary>
    private void AcquireCamera()
    {
        if (_camera != null)
        {
            return;
        }

        if (GameplayCameraProvider.TryGetGameplayCamera(out Camera gameplayCamera))
        {
            _camera = gameplayCamera;
            return;
        }

        _camera = Camera.main;
    }
}

