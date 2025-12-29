using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WF.Gameplay.Core.Data;
using WF.Gameplay.Core.Events;
using WF.Gameplay.Systems.Hatch;
using WF.Gameplay.Systems.Inventory.Items;
using WF.Gameplay.UI.Inventory;

namespace WF.Gameplay.UI.HUD
{
    public class TooltipsPanelController : MonoBehaviour
    {
        [Header("InformationTooltipsPanel")]
        [SerializeField] private CanvasGroup informationCanvasGroup; // 信息提示面板CanvasGroup（中文注释）
        [SerializeField] private Slider skillReadingSlider; // 技能读条Slider（中文注释）
        [SerializeField] private TMP_Text tooltipText; // 提示文本（中文注释）

        [Header("HatchTooltipsPanel")]
        [SerializeField] private CanvasGroup hatchCanvasGroup; // Hatch提示面板CanvasGroup（中文注释）
        [SerializeField] private RectTransform hatchSlotAnchor; // Hatch物品槽锚点（中文注释）
        [SerializeField] private Vector2 hatchSlotOffset; // Hatch物品槽偏移（中文注释）
        [SerializeField] private PackageUISlotController hatchSlotPrefab; // Hatch物品槽预制体（中文注释）
        [SerializeField] private float hatchSlotAlpha = 0.5f; // Hatch物品槽透明度（中文注释）
        [SerializeField] private float hatchRefreshInterval = 0.1f; // Hatch检测刷新间隔（中文注释）
        [SerializeField] private string playerTag = "Player"; // 玩家Tag（中文注释）

        private readonly List<HatchCombatController> _trackedHatches = new List<HatchCombatController>();
        private PackageUISlotController _hatchSlotInstance;
        private Transform _playerTransform;
        private HatchCombatController _activeHatch;

        private string _activeProgressId;
        private float _messageHideAtUnscaledTime;
        private bool _messageVisible;
        private float _nextHatchRefreshUnscaledTime;

        private void Awake()
        {
            AutoResolveReferences();
            EnsureCanvasGroupDefaults(informationCanvasGroup);
            EnsureCanvasGroupDefaults(hatchCanvasGroup);
            ApplyInformationVisible(false);
            ApplyHatchVisible(false);

            var graphic = GetComponent<Graphic>();
            if (graphic != null)
            {
                graphic.raycastTarget = false;
                if (graphic is Image img)
                {
                    var c = img.color;
                    c.a = 0f;
                    img.color = c;
                }
            }
        }

        private void OnEnable()
        {
            EventBus.Subscribe<ProgressStartedEvent>(OnProgressStarted);
            EventBus.Subscribe<ProgressUpdatedEvent>(OnProgressUpdated);
            EventBus.Subscribe<ProgressEndedEvent>(OnProgressEnded);
            EventBus.Subscribe<HatchSpawnedEvent>(OnHatchSpawned);

            RefreshPlayerTransform();
            RefreshTrackedHatches();
            TrySyncExistingHudProgress();
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<ProgressStartedEvent>(OnProgressStarted);
            EventBus.Unsubscribe<ProgressUpdatedEvent>(OnProgressUpdated);
            EventBus.Unsubscribe<ProgressEndedEvent>(OnProgressEnded);
            EventBus.Unsubscribe<HatchSpawnedEvent>(OnHatchSpawned);
        }

        private void Update()
        {
            RefreshPlayerTransform();
            UpdateMessageLifetime();
            UpdateHatchPanel();
        }

        public void ShowTooltipText(string message, float seconds = 2f) // 显示一段提示文本（中文注释）
        {
            if (tooltipText != null) tooltipText.text = message ?? string.Empty;

            _messageVisible = !string.IsNullOrEmpty(message);
            _messageHideAtUnscaledTime = _messageVisible ? Time.unscaledTime + Mathf.Max(0.01f, seconds) : 0f;
            ApplyInformationVisible(_messageVisible || !string.IsNullOrWhiteSpace(_activeProgressId));
        }

        private void AutoResolveReferences()
        {
            if (informationCanvasGroup == null)
            {
                var t = transform.Find("InformationTooltipsPanel");
                if (t != null) informationCanvasGroup = t.GetComponent<CanvasGroup>();
            }

            if (hatchCanvasGroup == null)
            {
                var t = transform.Find("HatchTooltipsPanel");
                if (t != null) hatchCanvasGroup = t.GetComponent<CanvasGroup>();
            }

            if (skillReadingSlider == null && informationCanvasGroup != null)
            {
                skillReadingSlider = informationCanvasGroup.GetComponentInChildren<Slider>(true);
            }

            if (tooltipText == null && informationCanvasGroup != null)
            {
                tooltipText = informationCanvasGroup.GetComponentInChildren<TMP_Text>(true);
            }

            if (hatchSlotAnchor == null && hatchCanvasGroup != null)
            {
                hatchSlotAnchor = hatchCanvasGroup.transform as RectTransform;
            }
        }

        private static void EnsureCanvasGroupDefaults(CanvasGroup cg)
        {
            if (cg == null) return;
            cg.blocksRaycasts = false;
            cg.interactable = false;
        }

        private void RefreshPlayerTransform()
        {
            if (_playerTransform != null) return;

            var player = GameObject.FindGameObjectWithTag(playerTag);
            if (player != null) _playerTransform = player.transform;
        }

        private void RefreshTrackedHatches()
        {
            _trackedHatches.Clear();
            var found = FindObjectsOfType<HatchCombatController>();
            for (int i = 0; i < found.Length; i++)
            {
                if (found[i] == null) continue;
                _trackedHatches.Add(found[i]);
            }
        }

        private void OnHatchSpawned(HatchSpawnedEvent e)
        {
            if (e.Instance == null) return;
            var combat = e.Instance.GetComponent<HatchCombatController>();
            if (combat == null) return;
            _trackedHatches.Add(combat);
        }

        private void UpdateHatchPanel()
        {
            if (Time.unscaledTime < _nextHatchRefreshUnscaledTime) return;
            _nextHatchRefreshUnscaledTime = Time.unscaledTime + Mathf.Max(0.02f, hatchRefreshInterval);

            if (_playerTransform == null)
            {
                SetActiveHatch(null);
                return;
            }

            for (int i = _trackedHatches.Count - 1; i >= 0; i--)
            {
                if (_trackedHatches[i] == null) _trackedHatches.RemoveAt(i);
            }

            HatchCombatController best = null;
            float bestDistSqr = float.MaxValue;

            for (int i = 0; i < _trackedHatches.Count; i++)
            {
                var hatch = _trackedHatches[i];
                if (hatch == null) continue;
                if (!hatch.CanBeTamedNow) continue;

                float range = Mathf.Max(0.01f, hatch.TameRange);
                Vector3 offset = hatch.transform.position - _playerTransform.position;
                float distSqr = offset.sqrMagnitude;
                if (distSqr > range * range) continue;
                if (distSqr >= bestDistSqr) continue;

                bestDistSqr = distSqr;
                best = hatch;
            }

            SetActiveHatch(best);
        }

        private void SetActiveHatch(HatchCombatController hatch)
        {
            if (_activeHatch == hatch) return;
            _activeHatch = hatch;

            if (_activeHatch == null)
            {
                ApplyHatchVisible(false);
                return;
            }

            EnsureHatchSlotInstance();
            UpdateHatchFavoriteItem();
            ApplyHatchVisible(true);
        }

        private void EnsureHatchSlotInstance()
        {
            if (_hatchSlotInstance != null) return;
            if (hatchSlotPrefab == null) return;
            if (hatchSlotAnchor == null) return;

            _hatchSlotInstance = Instantiate(hatchSlotPrefab, hatchSlotAnchor);
            _hatchSlotInstance.SetSlotType(UISlotType.Package);

            var rt = _hatchSlotInstance.transform as RectTransform;
            if (rt != null)
            {
                rt.anchoredPosition = hatchSlotOffset;
                rt.localRotation = Quaternion.identity;
                rt.localScale = Vector3.one;
            }

            var cg = _hatchSlotInstance.GetComponent<CanvasGroup>();
            if (cg == null) cg = _hatchSlotInstance.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = Mathf.Clamp01(hatchSlotAlpha);
            cg.blocksRaycasts = false;
            cg.interactable = false;
        }

        private void UpdateHatchFavoriteItem()
        {
            if (_hatchSlotInstance == null) return;

            var stats = _activeHatch != null ? _activeHatch.HatchStats : null;
            var preferred = stats != null ? stats.preferredItem : null;
            if (preferred is ItemBase itemBase)
            {
                _hatchSlotInstance.Bind(ItemStack.Create(itemBase, 1));
                return;
            }

            _hatchSlotInstance.Bind((ItemStack)null);
        }

        private void OnProgressStarted(ProgressStartedEvent e)
        {
            if (e.ViewMode != ProgressViewMode.Hud) return;

            _activeProgressId = e.ProgressId;

            if (skillReadingSlider != null)
            {
                skillReadingSlider.minValue = 0f;
                skillReadingSlider.maxValue = 1f;
                skillReadingSlider.value = Mathf.Clamp01(e.Progress01);
                skillReadingSlider.gameObject.SetActive(true);
            }

            if (!string.IsNullOrEmpty(e.Title) && tooltipText != null)
            {
                tooltipText.text = e.Title;
                _messageVisible = true;
                _messageHideAtUnscaledTime = float.MaxValue;
            }

            ApplyInformationVisible(true);
        }

        private void OnProgressUpdated(ProgressUpdatedEvent e)
        {
            if (string.IsNullOrWhiteSpace(_activeProgressId)) return;
            if (!string.Equals(_activeProgressId, e.ProgressId)) return;

            if (skillReadingSlider != null)
            {
                skillReadingSlider.value = Mathf.Clamp01(e.Progress01);
            }

            if (e.Title != null && tooltipText != null)
            {
                tooltipText.text = e.Title;
                _messageVisible = !string.IsNullOrEmpty(e.Title);
                _messageHideAtUnscaledTime = float.MaxValue;
            }
        }

        private void OnProgressEnded(ProgressEndedEvent e)
        {
            if (string.IsNullOrWhiteSpace(_activeProgressId)) return;
            if (!string.Equals(_activeProgressId, e.ProgressId)) return;

            _activeProgressId = null;

            if (skillReadingSlider != null)
            {
                skillReadingSlider.gameObject.SetActive(false);
                skillReadingSlider.value = 0f;
            }

            if (!_messageVisible)
            {
                ApplyInformationVisible(false);
            }
        }

        private void TrySyncExistingHudProgress()
        {
            var sys = WF.Gameplay.Systems.Progress.ProgressSystem.Instance;
            if (sys == null) return;

            if (!sys.TryGetLatestActiveByViewMode(ProgressViewMode.Hud, out var snapshot))
            {
                ApplyInformationVisible(_messageVisible);
                return;
            }

            _activeProgressId = snapshot.ProgressId;

            if (skillReadingSlider != null)
            {
                skillReadingSlider.minValue = 0f;
                skillReadingSlider.maxValue = 1f;
                skillReadingSlider.value = Mathf.Clamp01(snapshot.Progress01);
                skillReadingSlider.gameObject.SetActive(true);
            }

            if (tooltipText != null) tooltipText.text = snapshot.Title ?? string.Empty;

            _messageVisible = !string.IsNullOrEmpty(snapshot.Title);
            _messageHideAtUnscaledTime = float.MaxValue;
            ApplyInformationVisible(true);
        }

        private void UpdateMessageLifetime()
        {
            if (!_messageVisible) return;
            if (!string.IsNullOrWhiteSpace(_activeProgressId)) return;
            if (Time.unscaledTime < _messageHideAtUnscaledTime) return;

            _messageVisible = false;
            if (tooltipText != null) tooltipText.text = string.Empty;
            ApplyInformationVisible(false);
        }

        private void ApplyInformationVisible(bool visible)
        {
            if (informationCanvasGroup == null) return;
            informationCanvasGroup.alpha = visible ? 1f : 0f;
        }

        private void ApplyHatchVisible(bool visible)
        {
            if (hatchCanvasGroup == null) return;
            hatchCanvasGroup.alpha = visible ? 1f : 0f;
        }
    }
}
