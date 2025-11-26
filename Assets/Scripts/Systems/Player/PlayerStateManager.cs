using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using WF.Gameplay.Systems.Camera;
using UCamera = UnityEngine.Camera;

namespace WF.Gameplay.Systems.Player
{
    /// <summary>
    /// 玩家状态管理器（单例）。
    /// - 体力消耗/恢复的统一控制（基于 PlayerStats 参数）。
    /// - 连接 HUDCanvas 下的 "Stemina" UI，驱动体力条显示。
    /// - 为移动脚本提供体力门槛判断（CanSprint）。
    /// </summary>
    public class PlayerStateManager : MonoBehaviour
    {
        public static PlayerStateManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerMove playerMove;

    [Header("UI - Stemina")]
    [Tooltip("体力条根对象（HUDCanvas 的子物体 'Stemina'）")] 
    [SerializeField] private GameObject steminaUIObject;
    private Image _steminaImage;
    private Slider _steminaSlider;
    private Material _steminaMaterial;
    private RectTransform _steminaRect;
    private Canvas _steminaCanvas;
    private CanvasGroup _steminaCanvasGroup;

    [Tooltip("材质体力属性名（ArcStaminaBar 使用 'Stamina' 或 '_Stamina'）")]
    [SerializeField] private string[] materialStaminaPropertyNames = new[] { "Stamina", "_Stamina" };

    [Header("Auto Bind")]
    [Tooltip("启动时尝试自动绑定玩家与 UI 引用")]
    [SerializeField] private bool autoBindOnStart = true;

    [Header("Follow Settings")]
    [Tooltip("使体力条在屏幕上跟随玩家位置（HUDCanvas）")]
    [SerializeField] private bool followPlayerOnScreen = true;
    [Tooltip("世界空间偏移（体力条在玩家旁边/上方）")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2f, 0f);
    [Tooltip("屏幕空间偏移（像素）")]
    [SerializeField] private Vector2 screenOffset = new Vector2(30f, 0f);

    private UCamera _gameplayCamera;
    // 记录非奔跑状态持续时间，用于体力恢复延迟
    private float _timeNotSprinting;
    private float _invincibleTimer;
    private Dictionary<string, float> _moveSpeedMultipliers = new Dictionary<string, float>();

    [Header("UI Visibility")]
    [Tooltip("满值后保持显示的短暂停顿时间（秒）")]
    [SerializeField] private float staminaUIHideAfterFullSeconds = 0.5f;
    private float _hideAfterFullTimer;
    private bool _wasStaminaFull;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (autoBindOnStart)
        {
            TryBindPlayer();
            TryBindSteminaUI();
        }
        AcquireGameplayCamera();
    }

    private void Update()
    {
        if (playerStats == null)
        {
            TryBindPlayer();
        }

        if (playerMove == null)
        {
            TryBindPlayerMove();
        }

        if (playerStats == null || playerMove == null)
        {
            return;
        }

        // 体力消耗/恢复
        float dt = Time.deltaTime;
        if (playerMove.IsSprinting)
        {
            // 奔跑：消耗体力并重置“非奔跑计时”
            playerStats.ConsumeStamina(playerStats.sprintDrainPerSecond * dt);
            _timeNotSprinting = 0f;

            if (playerStats.stamina <= 0f)
            {
                playerMove.ForceStopSprint();
            }
        }
        else
        {
            // 非奔跑：先累计非奔跑时间，超过延迟后再恢复体力（加速）
            _timeNotSprinting += dt;
            if (_timeNotSprinting >= playerStats.staminaRegenDelaySeconds)
            {
                playerStats.RecoverStamina(playerStats.staminaRegenPerSecond * dt);
            }
        }

        // 驱动 UI
        UpdateSteminaUI(playerStats.StaminaNormalized);
        if (followPlayerOnScreen)
        {
            UpdateSteminaFollowPosition();
        }

        // 可见性控制：消耗、恢复延迟、恢复都显示；满值后短暂停顿再隐藏
        bool isConsuming = playerMove.IsSprinting && playerStats.stamina > 0f;
        bool delayActive = !playerMove.IsSprinting && (playerStats.stamina < playerStats.maxStamina) && (_timeNotSprinting < playerStats.staminaRegenDelaySeconds);
        bool regenActive = !playerMove.IsSprinting && (playerStats.stamina < playerStats.maxStamina) && (_timeNotSprinting >= playerStats.staminaRegenDelaySeconds);

        bool isFull = playerStats.stamina >= playerStats.maxStamina - 0.0001f;
        if (isFull && !_wasStaminaFull) { _hideAfterFullTimer = staminaUIHideAfterFullSeconds; }
        if (!isFull) { _hideAfterFullTimer = 0f; }
        _wasStaminaFull = isFull;
        if (_hideAfterFullTimer > 0f)
        {
            _hideAfterFullTimer -= dt;
            if (_hideAfterFullTimer < 0f) _hideAfterFullTimer = 0f;
        }

        bool visible = isConsuming || delayActive || regenActive || (_hideAfterFullTimer > 0f);
        SetSteminaVisibility(visible);

        if (_invincibleTimer > 0f)
        {
            _invincibleTimer -= Time.deltaTime;
            if (_invincibleTimer < 0f) _invincibleTimer = 0f;
        }
    }

    public bool CanSprint => playerStats != null && playerStats.HasStaminaToSprint();
    public bool CanRoll => playerStats != null && playerStats.stamina >= playerStats.rollStaminaCost;
    public bool IsInvincible => _invincibleTimer > 0f;
    public float MovementSpeedMultiplier
    {
        get
        {
            if (_moveSpeedMultipliers == null || _moveSpeedMultipliers.Count == 0) return 1f;
            float m = 1f;
            foreach (var kv in _moveSpeedMultipliers) m *= kv.Value;
            return m;
        }
    }

    public void SetInvincible(float seconds)
    {
        if (seconds <= 0f) return;
        _invincibleTimer = Mathf.Max(_invincibleTimer, seconds);
    }

    public void ConsumeStaminaForRoll()
    {
        if (playerStats == null) return;
        playerStats.ConsumeStamina(playerStats.rollStaminaCost);
        _timeNotSprinting = 0f;
    }

    public void SetMovementSpeedMultiplier(string sourceId, float multiplier)
    {
        if (string.IsNullOrEmpty(sourceId)) return;
        _moveSpeedMultipliers[sourceId] = Mathf.Max(0f, multiplier);
    }

    public void RemoveMovementSpeedMultiplier(string sourceId)
    {
        if (string.IsNullOrEmpty(sourceId)) return;
        if (_moveSpeedMultipliers.ContainsKey(sourceId)) _moveSpeedMultipliers.Remove(sourceId);
    }

    public System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<string, float>> GetMovementSpeedMultipliersSnapshot()
    {
        var list = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<string, float>>();
        foreach (var kv in _moveSpeedMultipliers) list.Add(kv);
        return list;
    }

    /// <summary>
    /// 外部可调用：绑定体力条 UI 根对象。
    /// </summary>
    public void BindSteminaUI(GameObject uiRoot)
    {
        steminaUIObject = uiRoot;
        CacheUIComponents();
    }

    private void TryBindPlayer()
    {
        if (playerStats == null)
        {
            playerStats = FindObjectOfType<PlayerStats>();
        }
        TryBindPlayerMove();
    }

    private void TryBindPlayerMove()
    {
        if (playerMove == null)
        {
            playerMove = FindObjectOfType<PlayerMove>();
        }
    }

    private void TryBindSteminaUI()
    {
        if (steminaUIObject == null)
        {
            var hud = GameObject.Find("HUDCanvas");
            if (hud != null)
            {
                // 直接子物体查找
                var t = hud.transform.Find("Stemina");
                if (t == null)
                {
                    // 深度查找备选
                    t = FindDeepChild(hud.transform, "Stemina");
                }
                if (t != null)
                {
                    steminaUIObject = t.gameObject;
                }
            }
        }
        CacheUIComponents();
    }

    private void CacheUIComponents()
    {
        _steminaImage = null;
        _steminaSlider = null;
        _steminaMaterial = null;
        _steminaRect = null;
        _steminaCanvas = null;
        _steminaCanvasGroup = null;

        if (steminaUIObject == null) return;

        _steminaImage = steminaUIObject.GetComponent<Image>();
        _steminaSlider = steminaUIObject.GetComponent<Slider>();
        _steminaRect = steminaUIObject.GetComponent<RectTransform>();
        _steminaCanvas = steminaUIObject.GetComponentInParent<Canvas>();
        _steminaCanvasGroup = steminaUIObject.GetComponent<CanvasGroup>();
        if (_steminaCanvasGroup == null)
        {
            _steminaCanvasGroup = steminaUIObject.AddComponent<CanvasGroup>();
        }
        // 初始隐藏
        _steminaCanvasGroup.alpha = 0f;
        _steminaCanvasGroup.interactable = false;
        _steminaCanvasGroup.blocksRaycasts = false;

        if (_steminaImage != null)
        {
            // 如果 Image 的材质存在并且带有我们需要的属性，则缓存材质
            var mat = _steminaImage.material;
            if (mat != null && materialStaminaPropertyNames.Any(mat.HasProperty))
            {
                _steminaMaterial = mat;
            }
        }
    }

    private void UpdateSteminaUI(float normalized)
    {
        // Slider（0-1）
        if (_steminaSlider != null)
        {
            if (_steminaSlider.maxValue != 1f) _steminaSlider.maxValue = 1f;
            _steminaSlider.value = normalized;
        }

        // Image.fillAmount（适用于 Filled 类型或 Radial）
        if (_steminaImage != null)
        {
            _steminaImage.fillAmount = normalized;
        }

        // ArcStaminaBar 材质驱动（Stamina / _Stamina + _Fill）
        if (_steminaMaterial != null)
        {
            // 角度填充（决定圆环长度）
            if (_steminaMaterial.HasProperty("_Fill"))
            {
                _steminaMaterial.SetFloat("_Fill", normalized);
            }

            // 颜色驱动（决定高/低颜色切换）
            foreach (var prop in materialStaminaPropertyNames)
            {
                if (_steminaMaterial.HasProperty(prop))
                {
                    _steminaMaterial.SetFloat(prop, normalized);
                    break;
                }
            }
        }
    }

    private void UpdateSteminaFollowPosition()
    {
        if (_steminaRect == null || playerMove == null)
            return;

        if (_gameplayCamera == null)
        {
        AcquireGameplayCamera();
            if (_gameplayCamera == null) return;
        }

        Vector3 worldPos = playerMove.transform.position + worldOffset;
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

    private void SetSteminaVisibility(bool visible)
    {
        if (_steminaCanvasGroup == null) return;
        _steminaCanvasGroup.alpha = visible ? 1f : 0f;
        _steminaCanvasGroup.interactable = visible;
        _steminaCanvasGroup.blocksRaycasts = visible;
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

    private static Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            var result = FindDeepChild(child, name);
            if (result != null) return result;
        }
        return null;
    }
}
}
