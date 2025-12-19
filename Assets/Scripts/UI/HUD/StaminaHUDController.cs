using UnityEngine;
using UnityEngine.UI;
using WF.Gameplay.Core.Events;
using WF.Gameplay.Systems.Player;
using WF.Gameplay.Systems.Camera;
using UCamera = UnityEngine.Camera;

namespace WF.Gameplay.UI.HUD
{
    public class StaminaHUDController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("体力条根对象")] 
        [SerializeField] private GameObject steminaUIObject;
        private Image _steminaImage;
        private Material _steminaMaterial;
        private RectTransform _steminaRect;
        private Canvas _steminaCanvas;
        private CanvasGroup _canvasGroup; // 用于控制UI显隐的CanvasGroup（新字段，中文注释）

        [Tooltip("材质填充属性名（用于控制圆弧进度，默认'_Fill'）")]
        [SerializeField] private string materialFillPropertyName = "_Fill"; // 新字段：材质填充属性名（中文注释）

        [Tooltip("是否同时更新'_Stamina'以驱动颜色计算（可选）")]
        [SerializeField] private bool updateStaminaColor = false; // 新字段：是否同步更新颜色（中文注释）

        [Header("Follow Settings")]
        [Tooltip("使体力条在屏幕上跟随玩家位置")]
        [SerializeField] private bool followPlayerOnScreen = true;
        [Tooltip("世界空间偏移（体力条在玩家旁边/上方）")]
        [SerializeField] private Vector3 worldOffset = new Vector3(0f, 0f, 0f);
        [Tooltip("屏幕空间偏移（像素）")]
        [SerializeField] private Vector2 screenOffset = new Vector2(80f, 0f);

        [Header("UI Visibility")]
        [Tooltip("满值后保持显示的短暂停顿时间（秒）")]
        [SerializeField] private float staminaUIHideAfterFullSeconds = 0.5f;
        [Tooltip("游戏开始时是否显示体力UI（默认false）")]
        [SerializeField] private bool showOnStart = false; // 新字段：控制初始是否显示（中文注释）
        private float _hideAfterFullTimer;
        private bool _wasStaminaFull;
        private bool _initialized; // 新字段：是否已初始化显隐（中文注释）

        private PlayerMove _playerMove;
        private UCamera _gameplayCamera;

        

        private void Awake()
        {
            if (steminaUIObject == null) steminaUIObject = gameObject;
            CacheUIComponents();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<StaminaChangedEvent>(OnStaminaChanged);
            TryManualUpdateFromStats();
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<StaminaChangedEvent>(OnStaminaChanged);
        }

        private void Start()
        {
            _playerMove = FindObjectOfType<PlayerMove>();
            AcquireGameplayCamera();
            InitializeVisibility();
        }

        private void Update()
        {
            if (followPlayerOnScreen)
            {
                UpdateSteminaFollowPosition();
            }

            if (!_initialized) return;
            if (_hideAfterFullTimer > 0f)
            {
                _hideAfterFullTimer -= Time.deltaTime;
            }
            UpdateVisibility(_wasStaminaFull, false);
        }

        private void OnStaminaChanged(StaminaChangedEvent e)
        {
            float normalized = e.Max > 0 ? e.Current / e.Max : 0f;
            UpdateSteminaUI(normalized);

            bool isFull = e.Current >= e.Max - 0.0001f;
            bool changed = isFull != _wasStaminaFull;
            _wasStaminaFull = isFull;
            UpdateVisibility(isFull, changed);
        }

        private void UpdateSteminaUI(float normalizedValue)
        {
            if (_steminaImage != null)
            {
                // _steminaImage.fillAmount = normalizedValue; // Shader驱动圆形进度，不使用Image.fillAmount
            }

            if (_steminaMaterial != null)
            {
                normalizedValue = Mathf.Clamp01(normalizedValue);
                if (!string.IsNullOrEmpty(materialFillPropertyName) && _steminaMaterial.HasProperty(materialFillPropertyName))
                {
                    _steminaMaterial.SetFloat(materialFillPropertyName, normalizedValue);
                }
                if (updateStaminaColor && _steminaMaterial.HasProperty("_Stamina"))
                {
                    _steminaMaterial.SetFloat("_Stamina", normalizedValue);
                }
                if (_steminaImage != null)
                {
                    _steminaImage.SetMaterialDirty();
                }
            }
        }

        private void SetSteminaVisibility(bool visible)
        {
            if (steminaUIObject == null) return;
            if (_canvasGroup == null) _canvasGroup = steminaUIObject.AddComponent<CanvasGroup>();
            _canvasGroup.alpha = visible ? 1f : 0f;
            _canvasGroup.blocksRaycasts = visible;
            _canvasGroup.interactable = visible;
            if (!steminaUIObject.activeSelf) steminaUIObject.SetActive(true);
        }

        private void UpdateSteminaFollowPosition()
        {
            if (_steminaRect == null || _playerMove == null) return;

            if (_gameplayCamera == null) AcquireGameplayCamera();
            if (_gameplayCamera == null) return;

            Vector3 worldPos = _playerMove.transform.position + worldOffset;
            Vector3 screenPos = _gameplayCamera.WorldToScreenPoint(worldPos);

            if (_steminaCanvas == null)
            {
                _steminaCanvas = _steminaRect.GetComponentInParent<Canvas>();
                if (_steminaCanvas == null) return;
            }

            RectTransform canvasRect = _steminaCanvas.transform as RectTransform;
            if (canvasRect == null) return;

            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPos,
                _steminaCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _steminaCanvas.worldCamera,
                out localPoint);

            _steminaRect.localPosition = localPoint + screenOffset;
        }

        private void CacheUIComponents()
        {
            if (steminaUIObject == null) return;
            _steminaRect = steminaUIObject.GetComponent<RectTransform>();
            _steminaImage = steminaUIObject.GetComponent<Image>();
            _canvasGroup = steminaUIObject.GetComponent<CanvasGroup>();
            if (_canvasGroup == null) _canvasGroup = steminaUIObject.AddComponent<CanvasGroup>();
            
            if (_steminaImage != null)
            {
                if (_steminaImage.material != null)
                {
                    if (_steminaMaterial == null)
                    {
                        _steminaMaterial = Instantiate(_steminaImage.material);
                        _steminaImage.material = _steminaMaterial;
                    }
                }
            }
        }

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

        // 启动时初始化显隐（新方法，中文注释）
        private void InitializeVisibility()
        {
            if (_initialized) return;
            SetSteminaVisibility(showOnStart);
            _initialized = true;
        }

        // 在启用时手动刷新一次UI（新方法，中文注释）
        private void TryManualUpdateFromStats()
        {
            var stats = FindObjectOfType<PlayerStats>();
            if (stats == null) return;
            float normalized = stats.StaminaNormalized;
            UpdateSteminaUI(normalized);
            bool isFull = normalized >= 1f - 0.0001f;
            bool changed = isFull != _wasStaminaFull;
            _wasStaminaFull = isFull;
            UpdateVisibility(isFull, changed);
        }

        // 三态显隐更新：刚满→显示并计时；满且计时结束→隐藏；不满→显示并取消计时（新方法，中文注释）
        private void UpdateVisibility(bool isFull, bool stateChanged)
        {
            if (stateChanged && isFull)
            {
                SetSteminaVisibility(true);
                _hideAfterFullTimer = staminaUIHideAfterFullSeconds;
                return;
            }

            if (isFull)
            {
                if (_hideAfterFullTimer <= 0f)
                {
                    SetSteminaVisibility(false);
                }
                else
                {
                    SetSteminaVisibility(true);
                }
            }
            else
            {
                _hideAfterFullTimer = 0f;
                SetSteminaVisibility(true);
            }
        }

        private void OnDestroy()
        {
            if (Application.isPlaying && _steminaMaterial != null)
            {
                Destroy(_steminaMaterial); // 仅销毁运行时实例化的材质（中文注释）
            }
        }
    }
}
