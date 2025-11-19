using UnityEngine;

/// <summary>
/// 控制敌人的材质切换，用于区分不同状态（例如，默认状态和盟友状态）。
/// </summary>
public class EnemyMaterialController : MonoBehaviour
{
    // 当找不到渲染器或材质时发出的警告信息
    private const string MissingRendererWarning = "EnemyMaterialController 未找到 Renderer 或材质。";

    [Tooltip("需要更换材质的渲染器数组")]
    [SerializeField] private Renderer[] targetRenderers;
    [Tooltip("默认状态下使用的材质")]
    [SerializeField] private Material defaultMaterial;
    [Tooltip("成为盟友后使用的材质")]
    [SerializeField] private Material allyMaterial;
    [Tooltip("是否在运行时实例化材质，以避免修改原始材质资源")]
    [SerializeField] private bool instantiateMaterials = true;

    // 运行时的默认材质实例
    private Material _runtimeDefaultMaterial;
    // 运行时的盟友材质实例
    private Material _runtimeAllyMaterial;

    /// <summary>
    /// 初始化渲染器引用和材质实例。
    /// </summary>
    private void Awake()
    {
        if ((targetRenderers == null || targetRenderers.Length == 0))
        {
            Renderer foundRenderer = GetComponentInChildren<Renderer>();
            if (foundRenderer != null)
            {
                targetRenderers = new[] { foundRenderer };
            }
        }

        if (instantiateMaterials)
        {
            if (defaultMaterial != null)
            {
                _runtimeDefaultMaterial = new Material(defaultMaterial);
            }

            if (allyMaterial != null)
            {
                _runtimeAllyMaterial = new Material(allyMaterial);
            }
        }
        else
        {
            _runtimeDefaultMaterial = defaultMaterial;
            _runtimeAllyMaterial = allyMaterial;
        }
    }

    /// <summary>
    /// 应用默认材质到所有 Renderer。
    /// </summary>
    public void ApplyDefaultMaterial()
    {
        ApplyMaterial(_runtimeDefaultMaterial);
    }

    /// <summary>
    /// 应用驯服后的友方材质。
    /// </summary>
    public void ApplyAllyMaterial()
    {
        ApplyMaterial(_runtimeAllyMaterial);
    }

    /// <summary>
    /// 将指定材质应用到所有目标渲染器。
    /// </summary>
    /// <param name="materialToApply">要应用的材质</param>
    private void ApplyMaterial(Material materialToApply)
    {
        if (materialToApply == null || targetRenderers == null || targetRenderers.Length == 0)
        {
            Debug.LogWarning(MissingRendererWarning, this);
            return;
        }

        foreach (Renderer targetRenderer in targetRenderers)
        {
            if (targetRenderer == null)
            {
                continue;
            }

            targetRenderer.material = materialToApply;
        }
    }
}
