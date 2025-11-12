using UnityEngine;

public class GameplayCameraProvider : MonoBehaviour
{
    private const string MissingCameraWarning = "GameplayCameraProvider 必须挂载在带 Camera 组件的对象上。";

    private static GameplayCameraProvider _instance;
    private Camera _cachedCamera;

    [SerializeField] private bool makeDontDestroyOnLoad = false;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning("重复的 GameplayCameraProvider 已被忽略。", this);
            Destroy(gameObject);
            return;
        }

        _instance = this;

        _cachedCamera = GetComponent<Camera>();
        if (_cachedCamera == null)
        {
            Debug.LogError(MissingCameraWarning, this);
            return;
        }

        if (makeDontDestroyOnLoad)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    /// <summary>
    /// 获取当前的游戏主相机。
    /// </summary>
    public static Camera GetGameplayCamera()
    {
        if (TryGetGameplayCamera(out Camera camera))
        {
            return camera;
        }

        return Camera.main;
    }

    /// <summary>
    /// 尝试获取当前的游戏主相机。
    /// </summary>
    public static bool TryGetGameplayCamera(out Camera camera)
    {
        camera = null;

        if (_instance != null && _instance._cachedCamera != null)
        {
            camera = _instance._cachedCamera;
            return true;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            camera = mainCamera;
            return true;
        }

        return false;
    }

    /// <summary>
    /// 返回当前主相机的 Transform。
    /// </summary>
    public static Transform GetCameraTransform()
    {
        if (TryGetGameplayCamera(out Camera camera))
        {
            return camera.transform;
        }

        return null;
    }
}

