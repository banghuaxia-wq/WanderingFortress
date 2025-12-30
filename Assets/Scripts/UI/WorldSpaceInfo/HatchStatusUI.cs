using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UCamera = UnityEngine.Camera;

/// <summary>
/// 管理Hatch的状态UI，包括血条、眩晕条和提示信息，并使其跟随目标在屏幕上移动。
/// </summary>
using WF.Gameplay.Systems.Camera;
using WF.Gameplay.Systems.Hatch;

namespace WF.Gameplay.UI.WorldSpaceInfo
{
    public class HatchStatusUI : MonoBehaviour
    {
        // 默认的自动隐藏延迟时间
        private const float DefaultHideDelay = 3f;
        // 最小的自动隐藏延迟时间
        private const float MinHideDelay = 0.1f;
        // UI元素与摄像机的最小距离，小于此距离则隐藏
        private const float MinDistanceToCamera = 0.01f;

        [FormerlySerializedAs("rootTransform")]
        [Tooltip("UI的根RectTransform")]
        [SerializeField] private RectTransform rootPanelTransform;

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
        [Tooltip("淡入持续时间（秒）")]
        [SerializeField] private float fadeInDuration = 0.2f;
        [Tooltip("淡出持续时间（秒）")]
        [SerializeField] private float fadeOutDuration = 0.2f;

        // UI跟随的目标
        private Transform _target;
        // 绑定的Hatch战斗控制器
        private HatchCombatController _combatController;
        // 主摄像机
        private UCamera _camera;
        // 自动隐藏的计时器
        private float _hideTimer;
        // 是否强制显示（例如，当显示工具提示时）
        private bool _isForcedVisible;
        private float _targetAlpha;
        private float _lastStun;
        // 父Canvas组件
        private Canvas _parentCanvas;
        // 父Canvas是否为世界空间模式
        private bool _isWorldSpaceCanvas;

        /// <summary>
        /// 初始化UI并将其与一个Hatch绑定。
        /// </summary>
        /// <param name="controller">Hatch的战斗控制器</param>
        public void Initialize(HatchCombatController controller)
        {
            if (controller == null)
            {
                Debug.LogError("传入的HatchCombatController为空！", this);
                gameObject.SetActive(false);
                return;
            }

            _combatController = controller;
            _target = controller.transform;

            SubscribeToEvents();

            AcquireCamera();
            HideStatusImmediate();
            ShowStatus();
        }

        /// <summary>
        /// 初始化组件引用和Canvas模式。
        /// </summary>
        private void Awake()
        {
            if (rootPanelTransform == null)
            {
                rootPanelTransform = GetComponent<RectTransform>();
            }

            if (rootPanelTransform != null)
            {
                _parentCanvas = rootPanelTransform.GetComponentInParent<Canvas>();
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
            _targetAlpha = 0f;
            _lastStun = 0f;
        }

        /// <summary>
        /// 当组件启用时，获取摄像机引用。
        /// </summary>
        private void OnEnable()
        {
            AcquireCamera();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        /// <summary>
        /// 当组件被销毁时，取消事件订阅。
        /// </summary>
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        /// <summary>
        /// 在每帧的最后更新UI位置和可见性。
        /// </summary>
        private void LateUpdate()
        {
            if (_target == null || rootPanelTransform == null)
            {
                return;
            }

            Vector3 worldPosition = _target.position + worldOffset;

            bool hasCanvasGroup = canvasGroup != null;

            if (_isWorldSpaceCanvas)
            {
                rootPanelTransform.position = worldPosition;
                if (_camera == null)
                {
                    AcquireCamera();
                }
                if (_camera != null)
                {
                    rootPanelTransform.forward = _camera.transform.forward;
                }
            }
            else
            {
                if (_camera == null)
                {
                    AcquireCamera();
                    if (_camera == null)
                    {
                        return;
                    }
                }

                Vector3 screenPoint = _camera.WorldToScreenPoint(worldPosition);

                if (hasCanvasGroup && screenPoint.z < MinDistanceToCamera)
                {
                    canvasGroup.alpha = 0f;
                    _targetAlpha = 0f;
                    return;
                }

                rootPanelTransform.position = screenPoint;
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
                    HideStatus();
                }
            }

            UpdateFade();
        }

        #region 事件订阅与处理

        private void SubscribeToEvents()
        {
            if (_combatController == null) return;

            _combatController.OnInitialized += HandleInitialized;
            _combatController.OnHealthChanged += HandleHealthChanged;
            _combatController.OnStunChanged += HandleStunChanged;
            _combatController.OnStunned += HandleStunned;
            _combatController.OnTamed += HandleTamed;
            _combatController.OnDefeated += HandleDefeated;
            _combatController.OnTooltipChanged += HandleTooltipChanged;
            _combatController.OnShowStatus += ShowStatus;
        }

        private void UnsubscribeFromEvents()
        {
            if (_combatController == null) return;

            _combatController.OnInitialized -= HandleInitialized;
            _combatController.OnHealthChanged -= HandleHealthChanged;
            _combatController.OnStunChanged -= HandleStunChanged;
            _combatController.OnStunned -= HandleStunned;
            _combatController.OnTamed -= HandleTamed;
            _combatController.OnDefeated -= HandleDefeated;
            _combatController.OnTooltipChanged -= HandleTooltipChanged;
            _combatController.OnShowStatus -= ShowStatus;
        }

        private void HandleInitialized(float maxHealth, float maxStun)
        {
            ConfigureBars(maxHealth, maxStun);
            ShowStatus();
        }

        private void HandleHealthChanged(float currentHealth, float maxHealth)
        {
            UpdateHealthBar(currentHealth);
            ShowStatus();
        }

        private void HandleStunChanged(float currentStun, float maxStun)
        {
            UpdateStunBar(currentStun);
            ShowStatus();
            _lastStun = currentStun;
        }

        private void HandleStunned()
        {
            ShowStatus();
        }

        private void HandleTamed()
        {
            HideTooltip();
            ShowStatus();
        }

        private void HandleDefeated()
        {
            ShowStatus();
        }

        private void HandleTooltipChanged(string message, bool show)
        {
            if (show)
            {
                ShowTooltip(message);
            }
            else
            {
                HideTooltip();
            }
        }

        #endregion

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
        private void ConfigureBars(float maxHealth, float maxStun)
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
        private void UpdateHealthBar(float currentHealth)
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
        private void UpdateStunBar(float currentStun)
        {
            if (stunSlider != null)
            {
                stunSlider.value = currentStun;
            }
        }

        /// <summary>
        /// 显示状态栏并重新计时。
        /// </summary>
        private void ShowStatus()
        {
            if (canvasGroup == null) return;
            _targetAlpha = 1f;
            float delay = Mathf.Max(hideDelay, MinHideDelay);
            _hideTimer = delay;

            if (alwaysVisible)
            {
                _hideTimer = float.MaxValue;
                _targetAlpha = 1f;
            }
        }

        /// <summary>
        /// 立即隐藏状态栏。
        /// </summary>
        private void HideStatusImmediate()
        {
            if (canvasGroup == null) return;
            if (alwaysVisible)
            {
                canvasGroup.alpha = 1f;
                _targetAlpha = 1f;
                return;
            }
            canvasGroup.alpha = 0f;
            _targetAlpha = 0f;
            _hideTimer = 0f;
        }

        /// <summary>
        /// 显示提示文本。
        /// </summary>
        /// <param name="message">要显示的提示信息</param>
        private void ShowTooltip(string message)
        {
            if (tooltipText == null) return;
            tooltipText.text = message;
            tooltipText.gameObject.SetActive(true);
            _isForcedVisible = true;
            ShowStatus();
        }

        /// <summary>
        /// 隐藏提示文本。
        /// </summary>
        private void HideTooltip()
        {
            if (tooltipText == null) return;
            tooltipText.gameObject.SetActive(false);
            _isForcedVisible = false;
        }

        /// <summary>
        /// 获取场景中的主摄像机。
        /// </summary>
        private void AcquireCamera()
        {
            if (_camera != null) return;
            if (GameplayCameraProvider.TryGetGameplayCamera(out UCamera gameplayCamera))
            {
                _camera = gameplayCamera;
                return;
            }
            _camera = UCamera.main;
        }

        private void HideStatus()
        {
            if (canvasGroup == null) return;
            if (alwaysVisible)
            {
                _targetAlpha = 1f;
                return;
            }
            _targetAlpha = 0f;
            _hideTimer = 0f;
        }

        private void UpdateFade()
        {
            if (canvasGroup == null) return;
            float current = canvasGroup.alpha;
            float target = _targetAlpha;
            if (Mathf.Approximately(current, target)) return;
            float duration = target > current ? Mathf.Max(0.0001f, fadeInDuration) : Mathf.Max(0.0001f, fadeOutDuration);
            float step = Time.deltaTime / duration;
            canvasGroup.alpha = Mathf.MoveTowards(current, target, step);
        }
    }
}

