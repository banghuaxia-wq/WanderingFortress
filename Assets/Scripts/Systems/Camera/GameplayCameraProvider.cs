using UnityEngine;
using UCamera = UnityEngine.Camera;

/// <summary>
/// 提供对主要游戏摄像机的集中访问方式。
/// 此类可确保游戏摄像机有一个单一、易于访问的实例。
/// </summary>
namespace WF.Gameplay.Systems.Camera
{
public class GameplayCameraProvider : MonoBehaviour
{
    // 当缺少 Camera 组件时的警告消息。
    private const string MissingCameraWarning = "GameplayCameraProvider 必须挂载在带 Camera 组件的对象上。";

    // GameplayCameraProvider 的单例实例。
    private static GameplayCameraProvider _instance;
    // 对 Camera 组件的缓存引用。
    private UCamera _cachedCamera;

    [Tooltip("如果为 true，则在加载新场景时不会销毁该摄像机。")]
    [SerializeField] private bool makeDontDestroyOnLoad;
    /// 初始化单例实例并缓存摄像机组件。
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning("重复的 GameplayCameraProvider 已被忽略。", this);
            Destroy(gameObject);
            return;
        }

        _instance = this;

        _cachedCamera = GetComponent<UCamera>();
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

    /// <summary>
    /// 当对象被销毁时，清除单例实例。
    /// </summary>
    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    /// <summary>
    /// 获取当前的主要游戏摄像机。
    /// 如果提供者不可用，则回退到 Camera.main。
    /// </summary>
    /// <returns>主要游戏摄像机。</returns>
    public static UCamera GetGameplayCamera()
    {
        if (TryGetGameplayCamera(out UCamera camera))
        {
            return camera;
        }

        return UCamera.main;
    }

    /// <summary>
    /// 尝试获取当前的主要游戏摄像机。
    /// </summary>
    /// <param name="camera">输出的摄像机实例。</param>
    /// <returns>如果成功检索到摄像机，则为 true，否则为 false。</returns>
    public static bool TryGetGameplayCamera(out UCamera camera)
    {
        camera = null;

        if (_instance != null && _instance._cachedCamera != null)
        {
            camera = _instance._cachedCamera;
            return true;
        }

        UCamera mainCamera = UCamera.main;
        if (mainCamera != null)
        {
            camera = mainCamera;
            return true;
        }

        return false;
    }

    /// <summary>
    /// 返回当前主摄像机的 Transform。
    /// </summary>
    /// <returns>摄像机的 transform，如果未找到摄像机，则为 null。</returns>
    public static Transform GetCameraTransform()
    {
        if (TryGetGameplayCamera(out UCamera camera))
        {
            return camera.transform;
        }

        return null;
    }
}
}


