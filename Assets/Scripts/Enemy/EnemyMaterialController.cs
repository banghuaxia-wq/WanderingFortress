using UnityEngine;

public class EnemyMaterialController : MonoBehaviour
{
    private const string MissingRendererWarning = "EnemyMaterialController 未找到 Renderer 或材质。";

    [SerializeField] private Renderer[] targetRenderers;
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material allyMaterial;
    [SerializeField] private bool instantiateMaterials = true;

    private Material _runtimeDefaultMaterial;
    private Material _runtimeAllyMaterial;

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
