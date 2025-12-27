using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WF.Gameplay.Core.Data;
using WF.Gameplay.Core.Events;
using WF.Gameplay.Systems.Progress;

namespace WF.Gameplay.UI.Progress
{
    public class ProgressBarController : MonoBehaviour
    {
        [SerializeField] private ProgressViewMode viewMode = ProgressViewMode.Hud;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Slider slider;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text percentText;

        private string _activeProgressId;

        private void Awake()
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
            ApplyVisible(false);
        }

        private void OnEnable()
        {
            EventBus.Subscribe<ProgressStartedEvent>(OnProgressStarted);
            EventBus.Subscribe<ProgressUpdatedEvent>(OnProgressUpdated);
            EventBus.Subscribe<ProgressEndedEvent>(OnProgressEnded);

            TrySyncFromSystem();
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<ProgressStartedEvent>(OnProgressStarted);
            EventBus.Unsubscribe<ProgressUpdatedEvent>(OnProgressUpdated);
            EventBus.Unsubscribe<ProgressEndedEvent>(OnProgressEnded);
        }

        private void TrySyncFromSystem()
        {
            var sys = ProgressSystem.Instance;
            if (sys == null)
            {
                ApplyVisible(false);
                return;
            }

            if (sys.TryGetLatestActiveByViewMode(viewMode, out var snapshot))
            {
                _activeProgressId = snapshot.ProgressId;
                ApplySnapshot(snapshot);
                ApplyVisible(true);
            }
            else
            {
                ApplyVisible(false);
            }
        }

        private void OnProgressStarted(ProgressStartedEvent e)
        {
            if (e.ViewMode != viewMode) return;
            _activeProgressId = e.ProgressId;

            var snapshot = new ProgressSnapshot(e.ProgressId, e.ViewMode, e.Title, e.Progress01, e.CanCancel);
            ApplySnapshot(snapshot);
            ApplyVisible(true);
        }

        private void OnProgressUpdated(ProgressUpdatedEvent e)
        {
            if (string.IsNullOrWhiteSpace(_activeProgressId)) return;
            if (!string.Equals(e.ProgressId, _activeProgressId)) return;

            float p01 = Mathf.Clamp01(e.Progress01);
            if (slider != null) slider.value = p01;

            if (titleText != null && e.Title != null) titleText.text = e.Title;
            if (percentText != null) percentText.text = Mathf.RoundToInt(p01 * 100f).ToString() + "%";
        }

        private void OnProgressEnded(ProgressEndedEvent e)
        {
            if (string.IsNullOrWhiteSpace(_activeProgressId)) return;
            if (!string.Equals(e.ProgressId, _activeProgressId)) return;

            _activeProgressId = null;
            ApplyVisible(false);
        }

        private void ApplySnapshot(ProgressSnapshot snapshot)
        {
            float p01 = Mathf.Clamp01(snapshot.Progress01);
            if (slider != null)
            {
                slider.minValue = 0f;
                slider.maxValue = 1f;
                slider.value = p01;
            }

            if (titleText != null) titleText.text = snapshot.Title ?? string.Empty;
            if (percentText != null) percentText.text = Mathf.RoundToInt(p01 * 100f).ToString() + "%";
        }

        private void ApplyVisible(bool visible)
        {
            if (canvasGroup == null) return;
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.blocksRaycasts = visible;
            canvasGroup.interactable = visible;
        }
    }
}

