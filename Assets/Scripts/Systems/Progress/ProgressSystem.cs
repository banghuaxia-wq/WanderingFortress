using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using WF.Gameplay.Core.Data;
using WF.Gameplay.Core.Events;

namespace WF.Gameplay.Systems.Progress
{
    public class ProgressSystem : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureInScene()
        {
            if (FindObjectOfType<ProgressSystem>() != null) return;
            var go = new GameObject("ProgressSystem");
            go.AddComponent<ProgressSystem>();
        }

        public static ProgressSystem Instance { get; private set; }

        private sealed class ActiveProgress
        {
            public ProgressSnapshot Snapshot;
            public long Seq;
        }

        private readonly Dictionary<string, ActiveProgress> _active = new Dictionary<string, ActiveProgress>();
        private readonly Dictionary<string, Coroutine> _running = new Dictionary<string, Coroutine>();
        private long _seqCounter;

        private bool _hasPendingTransition;
        private string _pendingLoadingSceneName;
        private string _pendingTargetSceneName;
        private string _pendingTitle;
        private float _pendingReadyToActivateHoldSeconds;
        private string _pendingProgressId;
        private float _pendingMinLoadingSceneShowSeconds;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!_hasPendingTransition) return;
            if (!string.Equals(scene.name, _pendingLoadingSceneName, StringComparison.Ordinal)) return;

            string target = _pendingTargetSceneName;
            string title = _pendingTitle;
            float hold = _pendingReadyToActivateHoldSeconds;
            string pid = _pendingProgressId;
            float minShowSeconds = _pendingMinLoadingSceneShowSeconds;

            _hasPendingTransition = false;
            _pendingLoadingSceneName = null;
            _pendingTargetSceneName = null;
            _pendingTitle = null;
            _pendingProgressId = null;
            _pendingMinLoadingSceneShowSeconds = 0f;

            StartCoroutine(BeginTargetLoadAfterLoadingScenePresented(target, title, hold, pid, minShowSeconds));
        }

        private System.Collections.IEnumerator BeginTargetLoadAfterLoadingScenePresented(string targetSceneName, string title, float readyToActivateHoldSeconds, string progressId, float minShowSeconds)
        {
            yield return null;
            if (minShowSeconds > 0f) yield return new WaitForSeconds(minShowSeconds);
            LoadSceneAsyncWithProgress(targetSceneName, title, readyToActivateHoldSeconds, progressId);
        }

        public string StartProgress(string title, ProgressViewMode viewMode, bool canCancel = false, float initialProgress01 = 0f, string progressId = null)
        {
            string id = string.IsNullOrWhiteSpace(progressId) ? Guid.NewGuid().ToString("N") : progressId;
            float p01 = Mathf.Clamp01(initialProgress01);

            var snapshot = new ProgressSnapshot(id, viewMode, title ?? string.Empty, p01, canCancel);
            var active = new ActiveProgress { Snapshot = snapshot, Seq = ++_seqCounter };
            _active[id] = active;

            EventBus.Publish(new ProgressStartedEvent(id, viewMode, snapshot.Title, snapshot.Progress01, snapshot.CanCancel));
            return id;
        }

        public bool UpdateProgress(string progressId, float progress01, string title = null)
        {
            if (string.IsNullOrWhiteSpace(progressId)) return false;
            if (!_active.TryGetValue(progressId, out var active)) return false;

            float p01 = Mathf.Clamp01(progress01);
            var snapshot = active.Snapshot;
            snapshot.Progress01 = p01;
            if (title != null) snapshot.Title = title;
            active.Snapshot = snapshot;

            EventBus.Publish(new ProgressUpdatedEvent(progressId, snapshot.Progress01, snapshot.Title));
            return true;
        }

        public bool End(string progressId, ProgressEndReason reason = ProgressEndReason.Completed)
        {
            if (string.IsNullOrWhiteSpace(progressId)) return false;
            if (_running.TryGetValue(progressId, out var co) && co != null)
            {
                StopCoroutine(co);
                _running.Remove(progressId);
            }
            if (!_active.Remove(progressId)) return false;
            EventBus.Publish(new ProgressEndedEvent(progressId, reason));
            return true;
        }

        public bool Cancel(string progressId)
        {
            return End(progressId, ProgressEndReason.Cancelled);
        }

        public bool TryGetActive(string progressId, out ProgressSnapshot snapshot)
        {
            snapshot = default;
            if (string.IsNullOrWhiteSpace(progressId)) return false;
            if (!_active.TryGetValue(progressId, out var active)) return false;
            snapshot = active.Snapshot;
            return true;
        }

        public bool TryGetLatestActiveByViewMode(ProgressViewMode viewMode, out ProgressSnapshot snapshot)
        {
            snapshot = default;
            long bestSeq = -1;
            ActiveProgress best = null;

            foreach (var kv in _active)
            {
                var active = kv.Value;
                if (active == null) continue;
                if (active.Snapshot.ViewMode != viewMode) continue;
                if (active.Seq <= bestSeq) continue;
                bestSeq = active.Seq;
                best = active;
            }

            if (best == null) return false;
            snapshot = best.Snapshot;
            return true;
        }

        public string RunTimedProgress(float durationSeconds, string title, ProgressViewMode viewMode, bool canCancel = true, string progressId = null)
        {
            float dur = Mathf.Max(0.0001f, durationSeconds);
            string id = StartProgress(title, viewMode, canCancel, 0f, progressId);
            _running[id] = StartCoroutine(RunTimedProgressRoutine(id, dur));
            return id;
        }

        private System.Collections.IEnumerator RunTimedProgressRoutine(string progressId, float durationSeconds)
        {
            float t = 0f;
            while (t < durationSeconds)
            {
                if (!_active.ContainsKey(progressId)) yield break;
                t += Time.deltaTime;
                UpdateProgress(progressId, t / durationSeconds, null);
                yield return null;
            }

            UpdateProgress(progressId, 1f, null);
            End(progressId, ProgressEndReason.Completed);
        }

        public string LoadSceneAsyncWithProgress(string sceneName, string title = null, float readyToActivateHoldSeconds = 0.1f, string progressId = null)
        {
            if (string.IsNullOrWhiteSpace(sceneName)) return null;
            string id = StartProgress(title ?? "Loading...", ProgressViewMode.Fullscreen, false, 0f, progressId);
            _running[id] = StartCoroutine(LoadSceneAsyncRoutine(id, sceneName, Mathf.Max(0f, readyToActivateHoldSeconds)));
            return id;
        }

        public string LoadSceneViaLoadingScene(string loadingSceneName, string targetSceneName, string title = null, float readyToActivateHoldSeconds = 0.1f, string progressId = null, float minLoadingSceneShowSeconds = 0.05f)
        {
            if (string.IsNullOrWhiteSpace(loadingSceneName)) return null;
            if (string.IsNullOrWhiteSpace(targetSceneName)) return null;

            if (!Application.CanStreamedLevelBeLoaded(loadingSceneName))
            {
                return LoadSceneAsyncWithProgress(targetSceneName, title, readyToActivateHoldSeconds, progressId);
            }

            _hasPendingTransition = true;
            _pendingLoadingSceneName = loadingSceneName;
            _pendingTargetSceneName = targetSceneName;
            _pendingTitle = title ?? "Loading...";
            _pendingReadyToActivateHoldSeconds = Mathf.Max(0f, readyToActivateHoldSeconds);
            _pendingProgressId = progressId;
            _pendingMinLoadingSceneShowSeconds = Mathf.Max(0f, minLoadingSceneShowSeconds);

            SceneManager.LoadScene(loadingSceneName);
            return _pendingProgressId;
        }

        private System.Collections.IEnumerator LoadSceneAsyncRoutine(string progressId, string sceneName, float readyToActivateHoldSeconds)
        {
            var op = SceneManager.LoadSceneAsync(sceneName);
            if (op == null)
            {
                End(progressId, ProgressEndReason.Failed);
                yield break;
            }

            op.allowSceneActivation = false;

            while (!op.isDone)
            {
                if (!_active.ContainsKey(progressId)) yield break;

                float raw = Mathf.Clamp01(op.progress / 0.9f);
                UpdateProgress(progressId, raw, null);

                if (op.progress >= 0.9f)
                {
                    UpdateProgress(progressId, 1f, null);
                    if (readyToActivateHoldSeconds > 0f) yield return new WaitForSeconds(readyToActivateHoldSeconds);
                    op.allowSceneActivation = true;
                }

                yield return null;
            }

            End(progressId, ProgressEndReason.Completed);
        }
    }
}
