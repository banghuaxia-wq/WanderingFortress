using UnityEngine;
using UnityEngine.UI;

public class EnemyStatusUI : MonoBehaviour
{
    private const float DefaultHideDelay = 3f;
    private const float MinHideDelay = 0.1f;
    private const float MinDistanceToCamera = 0.01f;

    [SerializeField] private RectTransform rootTransform;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider stunSlider;
    [SerializeField] private Text tooltipText;
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2.2f, 0f);
    [SerializeField] private float hideDelay = DefaultHideDelay;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private bool alwaysVisible = false;

    private Transform _target;
    private Camera _camera;
    private float _hideTimer;
    private bool _isForcedVisible;
    private Canvas _parentCanvas;
    private bool _isWorldSpaceCanvas;

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

    private void OnEnable()
    {
        AcquireCamera();
    }

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
    public void AttachToTarget(Transform target)
    {
        _target = target;
        AcquireCamera();
    }

    /// <summary>
    /// 配置血条和眩晕条的最大值。
    /// </summary>
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

