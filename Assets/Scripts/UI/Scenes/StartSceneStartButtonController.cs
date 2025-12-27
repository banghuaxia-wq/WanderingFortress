using UnityEngine;
using WF.Gameplay.Systems.Progress;

namespace WF.Gameplay.UI.Scenes
{
    public class StartSceneStartButtonController : MonoBehaviour
    {
        [SerializeField] private string loadingSceneName = "LoadingScene";
        [SerializeField] private string targetSceneName = "GameScene";
        [SerializeField] private string loadingTitle = "Loading...";
        [SerializeField] private float readyToActivateHoldSeconds = 0.1f;
        [SerializeField] private float minLoadingSceneShowSeconds = 0.05f;

        public void OnStartClicked()
        {
            var progressSystem = ProgressSystem.Instance;
            if (progressSystem == null)
            {
                var go = new GameObject("ProgressSystem");
                progressSystem = go.AddComponent<ProgressSystem>();
            }

            progressSystem.LoadSceneViaLoadingScene(loadingSceneName, targetSceneName, loadingTitle, readyToActivateHoldSeconds, null, minLoadingSceneShowSeconds);
        }
    }
}

