using UnityEngine;
using UCamera = UnityEngine.Camera;
using WF.Gameplay.Core.Data;
using WF.Gameplay.Systems.Camera;

namespace WF.Gameplay.Systems.Building
{
    public class BuildModeController : MonoBehaviour
    {
        [SerializeField] private BuildGridSystem gridSystem; // 网格系统引用（中文注释）
        [SerializeField] private BuildingDefinition selectedBuilding; // 当前选择的建筑定义（用于预览/放置）（中文注释）
        [SerializeField] private GridOverlayController gridOverlay; // 网格覆盖显示控制器（中文注释）
        [SerializeField] private LayerMask groundMask = ~0; // 地面射线检测层（Terrain需要在此层内）（中文注释）
        [SerializeField] private KeyCode toggleKey = KeyCode.B; // 建造模式开关键（中文注释）
        [SerializeField] private KeyCode rotateKey = KeyCode.R; // 旋转键（中文注释）

        private GridRotation _rotation; // 当前预览旋转（中文注释）
        private bool _enabled; // 是否处于建造模式（中文注释）
        private UCamera _camera; // 当前使用的摄像机（中文注释）

        // 初始化摄像机并关闭建造模式（中文注释）
        private void Start()
        {
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

            if (gridSystem == null || gridOverlay == null || selectedBuilding == null) return;
            if (_camera == null) AcquireCamera();
            if (_camera == null) return;

            if (!TryGetCellUnderCursor(out var cell)) return;

            bool canPlace = gridSystem.CanPlace(selectedBuilding, cell, _rotation);
            gridOverlay.SetPreview(gridSystem, selectedBuilding, cell, _rotation, canPlace);

            if (Input.GetMouseButtonDown(0))
            {
                if (canPlace)
                {
                    gridSystem.TryPlace(selectedBuilding, cell, _rotation, out _);
                }
            }
        }

        // 切换建造模式显示（中文注释）
        private void SetBuildMode(bool enable)
        {
            _enabled = enable;
            if (gridOverlay != null) gridOverlay.SetVisible(enable);
        }

        // 射线检测地面并转换为格坐标（中文注释）
        private bool TryGetCellUnderCursor(out Vector2Int cell)
        {
            cell = default;
            if (gridSystem == null) return false;

            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out var hit, 9999f, groundMask)) return false;

            cell = gridSystem.WorldToCell(hit.point);
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
    }
}
