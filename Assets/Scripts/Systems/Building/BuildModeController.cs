using UnityEngine;
using UCamera = UnityEngine.Camera;
using WF.Gameplay.Core.Data;
using WF.Gameplay.Systems.Camera;

namespace WF.Gameplay.Systems.Building
{
    public class BuildModeController : MonoBehaviour
    {
        private const string FixCubeNamePrefix = "FixCube"; // 用于补平落差的方块名前缀（建造模式需要忽略它们）（中文注释）

        [SerializeField] private BuildGridSystem gridSystem; // 网格系统引用（中文注释）
        [SerializeField] private BuildingDefinition selectedBuilding; // 当前选择的建筑定义（用于预览/放置）（中文注释）
        [SerializeField] private GridOverlayController gridOverlay; // 网格覆盖显示控制器（中文注释）
        [SerializeField] private Material ghostMaterial; // 建造预览使用的Ghost材质（中文注释）
        [SerializeField] private Color ghostValidColor = new Color(0f, 1f, 0f, 1f); // 可放置时Ghost颜色（中文注释）
        [SerializeField] private Color ghostInvalidColor = new Color(1f, 0f, 0f, 1f); // 不可放置时Ghost颜色（中文注释）
        [SerializeField, Range(0f, 1f)] private float ghostOpacity = 0.5f; // Ghost透明度（中文注释）
        [SerializeField] private LayerMask blockingMask = 0; // 放置时阻挡检测层（0表示不启用）（中文注释）
        [SerializeField] private LayerMask groundMask = ~0; // 地面射线检测层（Terrain需要在此层内）（中文注释）
        [SerializeField] private KeyCode toggleKey = KeyCode.B; // 建造模式开关键（中文注释）
        [SerializeField] private KeyCode rotateKey = KeyCode.R; // 旋转键（中文注释）

        private GridRotation _rotation; // 当前预览旋转（中文注释）
        private bool _enabled; // 是否处于建造模式（中文注释）
        private UCamera _camera; // 当前使用的摄像机（中文注释）

        private GameObject _ghostInstance; // 预览物体实例（中文注释）
        private BuildingDefinition _ghostDefinition; // 当前预览对应的建筑定义（中文注释）
        private Renderer[] _ghostRenderers; // 预览渲染器缓存（中文注释）
        private MaterialPropertyBlock _ghostPropertyBlock; // 预览材质属性块（中文注释）
        private readonly RaycastHit[] _cursorRaycastHits = new RaycastHit[32]; // 鼠标射线命中缓存（避免分配）（中文注释）
        private readonly Collider[] _blockingOverlapColliders = new Collider[64]; // 阻挡检测Overlap缓存（避免分配）（中文注释）

        // 初始化运行时对象（避免在字段初始化阶段调用Unity原生CreateImpl）（中文注释）
        private void Awake()
        {
            _ghostPropertyBlock = new MaterialPropertyBlock();
        }

        // 初始化摄像机并关闭建造模式（中文注释）
        private void Start()
        {
            if (gridSystem == null)
            {
                gridSystem = FindObjectOfType<BuildGridSystem>();
            }

            if (gridOverlay == null)
            {
                gridOverlay = FindObjectOfType<GridOverlayController>();
            }

            AcquireCamera();
            SetBuildMode(false);
        }

        // 处理建造模式输入与预览/放置（中文注释）
        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                SetBuildMode(!_enabled);
            }

            if (!_enabled) return;

            if (selectedBuilding != null && selectedBuilding.AllowRotation && Input.GetKeyDown(rotateKey))
            {
                _rotation = (GridRotation)(((int)_rotation + 1) % 4);
            }

            if (gridSystem == null || selectedBuilding == null) return;
            if (_camera == null) AcquireCamera();
            if (_camera == null) return;

            if (!TryGetCellUnderCursor(out var cell)) return;

            bool canOccupy = gridSystem.CanPlace(selectedBuilding, cell, _rotation);
            LayerMask surfaceMask = groundMask;
            bool surfaceValid = gridSystem.IsSurfaceValid(selectedBuilding, cell, _rotation, surfaceMask, out float placementY);
            bool blocked = IsBlockedByWorld(selectedBuilding, cell, _rotation, placementY);
            bool canPlace = canOccupy && surfaceValid && !blocked;
            if (gridOverlay != null) gridOverlay.SetPreview(gridSystem, selectedBuilding, cell, _rotation, canPlace);
            UpdateGhostPreview(selectedBuilding, cell, _rotation, placementY, canPlace);

            if (Input.GetMouseButtonDown(0))
            {
                if (canPlace)
                {
                    gridSystem.TryPlace(selectedBuilding, cell, _rotation, surfaceMask, out _);
                }
            }
        }

        // 切换建造模式显示（中文注释）
        private void SetBuildMode(bool enable)
        {
            _enabled = enable;
            if (gridOverlay != null) gridOverlay.SetVisible(enable);
            if (!enable) ClearGhostPreview();
        }

        // 射线检测地面并转换为格坐标（中文注释）
        private bool TryGetCellUnderCursor(out Vector2Int cell)
        {
            cell = default;
            if (gridSystem == null) return false;

            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            int hitCount = Physics.RaycastNonAlloc(ray, _cursorRaycastHits, 9999f, groundMask, QueryTriggerInteraction.Ignore);
            if (hitCount <= 0) return false;

            float bestDistance = float.PositiveInfinity;
            Vector3 bestPoint = default;
            bool hasBest = false;
            for (int i = 0; i < hitCount; i++)
            {
                var hit = _cursorRaycastHits[i];
                var collider = hit.collider;
                if (collider == null) continue;
                if (IsIgnoredPlacementCollider(collider)) continue;

                if (hit.distance < bestDistance)
                {
                    bestDistance = hit.distance;
                    bestPoint = hit.point;
                    hasBest = true;
                }
            }

            if (!hasBest) return false;

            cell = gridSystem.WorldToCell(bestPoint);
            return true;
        }

        // 获取游戏摄像机：优先GameplayCameraProvider，其次Camera.main（中文注释）
        private void AcquireCamera()
        {
            if (GameplayCameraProvider.TryGetGameplayCamera(out var camera))
            {
                _camera = camera;
                return;
            }

            _camera = UCamera.main;
        }

        // 生命周期结束时清理Ghost预览，避免残留（中文注释）
        private void OnDestroy()
        {
            ClearGhostPreview();
        }

        // 检测当前建筑Footprint是否与世界阻挡物发生重叠（中文注释）
        private bool IsBlockedByWorld(BuildingDefinition definition, Vector2Int anchorCell, GridRotation rotation, float placementY)
        {
            if (blockingMask.value == 0) return false;

            gridSystem.GetFootprintWorldBounds(definition, anchorCell, rotation, out var worldMin, out var worldMax);
            Vector3 center = (worldMin + worldMax) * 0.5f;
            Vector3 extents = (worldMax - worldMin) * 0.5f;
            center.y = placementY + 0.75f;
            extents.y = Mathf.Max(0.25f, extents.y) + 0.75f;

            int count = Physics.OverlapBoxNonAlloc(center, extents, _blockingOverlapColliders, Quaternion.identity, blockingMask, QueryTriggerInteraction.Ignore);
            if (count <= 0) return false;

            for (int i = 0; i < count; i++)
            {
                var collider = _blockingOverlapColliders[i];
                if (collider == null) continue;
                if (IsIgnoredPlacementCollider(collider)) continue;
                return true;
            }

            return false;
        }

        private static bool IsIgnoredPlacementCollider(Collider collider) // 是否需要被建造模式忽略的碰撞体（中文注释）
        {
            if (collider == null) return false;

            Transform t = collider.transform;
            while (t != null)
            {
                string name = t.name;
                if (!string.IsNullOrEmpty(name) && name.StartsWith(FixCubeNamePrefix))
                {
                    return true;
                }
                t = t.parent;
            }

            return false;
        }

        // 更新Ghost预览的位置、旋转与可放置表现（中文注释）
        private void UpdateGhostPreview(BuildingDefinition definition, Vector2Int anchorCell, GridRotation rotation, float placementY, bool canPlace)
        {
            if (definition == null || definition.Prefab == null)
            {
                ClearGhostPreview();
                return;
            }

            EnsureGhostInstance(definition);
            if (_ghostInstance == null) return;

            Quaternion gridRotation = gridSystem.RotationToWorld(rotation);
            Vector3 pivotOffset = definition.Prefab.transform.localPosition;
            Vector3 pos = gridSystem.CellToWorldCenter(anchorCell) + (gridRotation * pivotOffset);
            pos.y = placementY + pivotOffset.y + definition.PlacementYOffsetMeters;
            Quaternion rot = gridRotation * definition.Prefab.transform.localRotation;

            _ghostInstance.transform.SetPositionAndRotation(pos, rot);
            ApplyGhostAppearance(canPlace);
        }

        // 确保Ghost实例与当前选择建筑匹配并已初始化（中文注释）
        private void EnsureGhostInstance(BuildingDefinition definition)
        {
            if (_ghostInstance != null && _ghostDefinition == definition) return;

            ClearGhostPreview();
            _ghostDefinition = definition;
            _ghostInstance = Instantiate(definition.Prefab);
            _ghostInstance.name = $"{definition.Prefab.name}_Ghost";
            _ghostInstance.hideFlags = HideFlags.DontSave;

            foreach (var collider in _ghostInstance.GetComponentsInChildren<Collider>(true))
            {
                collider.enabled = false;
            }

            foreach (var rb in _ghostInstance.GetComponentsInChildren<Rigidbody>(true))
            {
                rb.isKinematic = true;
                rb.detectCollisions = false;
            }

            foreach (var behaviour in _ghostInstance.GetComponentsInChildren<MonoBehaviour>(true))
            {
                behaviour.enabled = false;
            }

            _ghostRenderers = _ghostInstance.GetComponentsInChildren<Renderer>(true);
            if (ghostMaterial != null && _ghostRenderers != null)
            {
                for (int i = 0; i < _ghostRenderers.Length; i++)
                {
                    if (_ghostRenderers[i] == null) continue;
                    _ghostRenderers[i].sharedMaterial = ghostMaterial;
                    _ghostRenderers[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    _ghostRenderers[i].receiveShadows = false;
                }
            }
        }

        // 应用Ghost材质属性（颜色/透明度）到所有渲染器（中文注释）
        private void ApplyGhostAppearance(bool canPlace)
        {
            if (_ghostRenderers == null || _ghostRenderers.Length == 0) return;

            _ghostPropertyBlock.SetColor("_BaseColor", canPlace ? ghostValidColor : ghostInvalidColor);
            _ghostPropertyBlock.SetFloat("_Opacity", ghostOpacity);
            for (int i = 0; i < _ghostRenderers.Length; i++)
            {
                var renderer = _ghostRenderers[i];
                if (renderer == null) continue;
                renderer.SetPropertyBlock(_ghostPropertyBlock);
            }
        }

        // 清理当前Ghost预览实例（中文注释）
        private void ClearGhostPreview()
        {
            _ghostDefinition = null;
            _ghostRenderers = null;
            if (_ghostInstance != null)
            {
                Destroy(_ghostInstance);
                _ghostInstance = null;
            }
        }
    }
}
