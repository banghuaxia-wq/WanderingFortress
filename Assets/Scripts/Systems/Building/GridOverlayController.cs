using UnityEngine;
using UCamera = UnityEngine.Camera;
using WF.Gameplay.Core.Data;

namespace WF.Gameplay.Systems.Building
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class GridOverlayController : MonoBehaviour
    {
        private static readonly int CellSizeId = Shader.PropertyToID("_CellSize"); // Shader属性ID缓存（格子尺寸）（中文注释）
        private static readonly int LineColorId = Shader.PropertyToID("_LineColor"); // Shader属性ID缓存（线颜色）（中文注释）
        private static readonly int BaseAlphaId = Shader.PropertyToID("_BaseAlpha"); // Shader属性ID缓存（基础透明度）（中文注释）
        private static readonly int LineWidthId = Shader.PropertyToID("_LineWidth"); // Shader属性ID缓存（线宽）（中文注释）
        private static readonly int FadeStartId = Shader.PropertyToID("_FadeStart"); // Shader属性ID缓存（淡出起点）（中文注释）
        private static readonly int FadeEndId = Shader.PropertyToID("_FadeEnd"); // Shader属性ID缓存（淡出终点）（中文注释）
        private static readonly int GridCenterId = Shader.PropertyToID("_GridCenter"); // Shader属性ID缓存（网格中心）（中文注释）
        private static readonly int HighlightEnabledId = Shader.PropertyToID("_HighlightEnabled"); // Shader属性ID缓存（高亮开关）（中文注释）
        private static readonly int HighlightColorId = Shader.PropertyToID("_HighlightColor"); // Shader属性ID缓存（高亮颜色）（中文注释）
        private static readonly int HighlightWorldMinId = Shader.PropertyToID("_HighlightWorldMin"); // Shader属性ID缓存（高亮包围盒最小值）（中文注释）
        private static readonly int HighlightWorldMaxId = Shader.PropertyToID("_HighlightWorldMax"); // Shader属性ID缓存（高亮包围盒最大值）（中文注释）

        [SerializeField] private Transform followTarget; // 网格中心跟随目标（通常为玩家）（中文注释）
        [SerializeField] private float overlaySizeMeters = 160f; // 覆盖平面边长（米）（中文注释）
        [SerializeField] private float overlayHeightOffset = 0.35f; // 覆盖平面高度偏移（避免Z-fighting，Terrain可适当加大）（中文注释）
        [SerializeField] private Color lineColor = new Color(1f, 1f, 1f, 1f); // 网格线颜色（中文注释）
        [SerializeField] private float baseAlpha = 0.35f; // 网格线基础透明度（中文注释）
        [SerializeField] private float lineWidthMeters = 0.04f; // 网格线宽（米）（中文注释）
        [SerializeField] private float fadeStartMeters = 50f; // 距离中心开始淡出（米）（中文注释）
        [SerializeField] private float fadeEndMeters = 80f; // 距离中心完全淡出（米）（中文注释）
        [SerializeField] private Color validHighlightColor = new Color(0.2f, 1f, 0.2f, 0.45f); // 可放置高亮颜色（中文注释）
        [SerializeField] private Color invalidHighlightColor = new Color(1f, 0.2f, 0.2f, 0.45f); // 不可放置高亮颜色（中文注释）

        private MeshRenderer _renderer; // 渲染器引用（中文注释）
        private MeshFilter _filter; // MeshFilter引用（中文注释）
        private Material _materialInstance; // 运行时材质实例（避免修改共享材质）（中文注释）
        private bool _visible; // 是否显示网格覆盖层（中文注释）

        // 初始化网格覆盖平面与材质（中文注释）
        private void Awake()
        {
            _renderer = GetComponent<MeshRenderer>();
            _filter = GetComponent<MeshFilter>();

            EnsureMesh();
            EnsureMaterial();
            ApplyStaticMaterialParams();
            if (_renderer != null) _renderer.enabled = false;

            if (followTarget == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) followTarget = player.transform;
            }

            if (followTarget == null && UCamera.main != null)
            {
                followTarget = UCamera.main.transform;
            }
        }

        // 跟随目标更新网格中心（中文注释）
        private void Update()
        {
            if (!_visible) return;
            Vector3 p = followTarget != null ? followTarget.position : transform.position;
            if (followTarget != null)
            {
                transform.position = new Vector3(p.x, overlayHeightOffset, p.z);
            }
            if (_materialInstance != null)
            {
                _materialInstance.SetVector(GridCenterId, new Vector4(p.x, 0f, p.z, 0f));
            }
        }

        // 销毁运行时材质实例（中文注释）
        private void OnDestroy()
        {
            if (Application.isPlaying && _materialInstance != null)
            {
                Destroy(_materialInstance);
            }
        }

        // 显示/隐藏网格覆盖层（中文注释）
        public void SetVisible(bool visible)
        {
            _visible = visible;
            if (_renderer != null) _renderer.enabled = visible;
            if (visible)
            {
                if (followTarget == null)
                {
                    var player = GameObject.FindGameObjectWithTag("Player");
                    if (player != null) followTarget = player.transform;
                    if (followTarget == null && UCamera.main != null) followTarget = UCamera.main.transform;
                }

                Vector3 p = followTarget != null ? followTarget.position : transform.position;
                if (followTarget != null)
                {
                    transform.position = new Vector3(p.x, overlayHeightOffset, p.z);
                }

                if (_materialInstance != null)
                {
                    _materialInstance.SetVector(GridCenterId, new Vector4(p.x, 0f, p.z, 0f));
                }
            }
            else
            {
                ClearPreview();
            }
        }

        // 设置当前建筑预览的高亮区域（当前为Footprint包围盒）（中文注释）
        public void SetPreview(BuildGridSystem grid, BuildingDefinition definition, Vector2Int anchorCell, GridRotation rotation, bool valid)
        {
            if (grid == null || definition == null) return;
            if (_materialInstance == null) return;

            if (grid.Config == null)
            {
                ClearPreview();
                return;
            }

            _materialInstance.SetFloat(CellSizeId, grid.Config.CellSizeMeters);

            grid.GetFootprintWorldBounds(definition, anchorCell, rotation, out var worldMin, out var worldMax);
            _materialInstance.SetFloat(HighlightEnabledId, 1f);
            _materialInstance.SetColor(HighlightColorId, valid ? validHighlightColor : invalidHighlightColor);
            _materialInstance.SetVector(HighlightWorldMinId, new Vector4(worldMin.x, 0f, worldMin.z, 0f));
            _materialInstance.SetVector(HighlightWorldMaxId, new Vector4(worldMax.x, 0f, worldMax.z, 0f));
        }

        // 清理高亮预览（中文注释）
        public void ClearPreview()
        {
            if (_materialInstance == null) return;
            _materialInstance.SetFloat(HighlightEnabledId, 0f);
        }

        // 应用不随预览变化的材质参数（中文注释）
        private void ApplyStaticMaterialParams()
        {
            if (_materialInstance == null) return;
            _materialInstance.SetColor(LineColorId, lineColor);
            _materialInstance.SetFloat(BaseAlphaId, baseAlpha);
            _materialInstance.SetFloat(LineWidthId, lineWidthMeters);
            _materialInstance.SetFloat(FadeStartId, fadeStartMeters);
            _materialInstance.SetFloat(FadeEndId, fadeEndMeters);
        }

        // 确保渲染器拥有可写的材质实例（中文注释）
        private void EnsureMaterial()
        {
            if (_renderer == null) return;
            const string shaderName = "WF/Building/GridOverlay";

            var desiredShader = Shader.Find(shaderName);
            if (desiredShader != null)
            {
                _materialInstance = new Material(desiredShader);
                _renderer.sharedMaterial = _materialInstance;
                return;
            }

            if (_renderer.sharedMaterial != null)
            {
                _materialInstance = Instantiate(_renderer.sharedMaterial);
                _renderer.sharedMaterial = _materialInstance;
            }
            else
            {
                Debug.LogError($"GridOverlay shader not found and no material assigned. ShaderName={shaderName}", this);
            }
        }

        // 确保覆盖用Quad Mesh存在（中文注释）
        private void EnsureMesh()
        {
            if (_filter == null) return;
            if (_filter.sharedMesh != null) return;

            float half = Mathf.Max(1f, overlaySizeMeters) * 0.5f;
            var mesh = new Mesh { name = "GridOverlayQuad" };

            mesh.vertices = new[]
            {
                new Vector3(-half, 0f, -half),
                new Vector3(half, 0f, -half),
                new Vector3(half, 0f, half),
                new Vector3(-half, 0f, half),
            };

            mesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            _filter.sharedMesh = mesh;
        }
    }
}
