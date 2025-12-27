using System.Collections.Generic;
using UnityEngine;
using WF.Gameplay.Core.Data;

namespace WF.Gameplay.Systems.Building
{
    public class BuildGridSystem : MonoBehaviour
    {
        private const string FixCubeNamePrefix = "FixCube"; // 用于补平落差的方块名前缀（建造模式需要忽略它们）（中文注释）

        [SerializeField] private BuildGridConfig config; // 网格配置（单元尺寸/原点）（中文注释）

        private readonly Dictionary<Vector2Int, int> _occupiedByInstance = new Dictionary<Vector2Int, int>(); // 占用表：cell -> instanceId（中文注释）
        private readonly Dictionary<int, PlacedBuilding> _placed = new Dictionary<int, PlacedBuilding>(); // 已放置建筑表：instanceId -> 数据（中文注释）
        private int _nextInstanceId = 1; // 自增实例ID（运行时）（中文注释）
        private readonly RaycastHit[] _surfaceRaycastHits = new RaycastHit[32];

        private struct PlacedBuilding
        {
            public BuildingDefinition Definition; // 建筑定义（中文注释）
            public Vector2Int AnchorCell; // 锚点格坐标（中文注释）
            public GridRotation Rotation; // 旋转（中文注释）
            public GameObject Instance; // 实例化的GameObject（可为空）（中文注释）
        }

        public BuildGridConfig Config => config; // 对外暴露网格配置（中文注释）

        // 判断指定建筑在指定锚点/旋转下是否可放置（中文注释）
        public bool CanPlace(BuildingDefinition definition, Vector2Int anchorCell, GridRotation rotation)
        {
            if (definition == null) return false;
            if (config == null) return false;
            if (definition.FootprintOffsets == null || definition.FootprintOffsets.Length == 0) return false;

            var offsets = definition.FootprintOffsets;
            for (int i = 0; i < offsets.Length; i++)
            {
                Vector2Int cell = anchorCell + RotateOffset(offsets[i], rotation);
                if (_occupiedByInstance.ContainsKey(cell))
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsSurfaceValid(BuildingDefinition definition, Vector2Int anchorCell, GridRotation rotation, LayerMask surfaceMask, out float placementY)
        {
            placementY = 0f;
            if (definition == null) return false;
            if (config == null) return false;
            if (definition.FootprintOffsets == null || definition.FootprintOffsets.Length == 0) return false;

            if (surfaceMask.value == 0)
            {
                placementY = config.OriginWorld.y;
                return true;
            }

            float startY = config.SurfaceRayStartHeightMeters;
            float distance = config.SurfaceRayDistanceMeters;
            float maxSlope = config.MaxSurfaceSlopeDegrees;
            float maxDelta = config.MaxFootprintHeightDeltaMeters;

            float minY = float.PositiveInfinity;
            float maxY = float.NegativeInfinity;
            float maxObservedSlope = 0f;

            int roadLayer = LayerMask.NameToLayer("Road");
            int groundLayer = LayerMask.NameToLayer("Ground");

            var offsets = definition.FootprintOffsets;
            for (int i = 0; i < offsets.Length; i++)
            {
                Vector2Int cell = anchorCell + RotateOffset(offsets[i], rotation);
                Vector3 samplePos = CellToWorldCenter(cell);
                var rayOrigin = new Vector3(samplePos.x, startY, samplePos.z);

                int hitCount = Physics.RaycastNonAlloc(rayOrigin, Vector3.down, _surfaceRaycastHits, distance, ~0, QueryTriggerInteraction.Ignore);
                if (hitCount <= 0) return false;

                float topHitDistance = float.PositiveInfinity;
                Transform topHitTransform = null;
                for (int h = 0; h < hitCount; h++)
                {
                    var hit = _surfaceRaycastHits[h];
                    var collider = hit.collider;
                    if (collider == null) continue;
                    if (IsIgnoredPlacementCollider(collider)) continue;

                    if (hit.distance < topHitDistance)
                    {
                        topHitDistance = hit.distance;
                        topHitTransform = collider.transform;
                    }
                }

                if (topHitTransform == null) return false;
                if (!TryGetAllowedLayerFromHierarchy(topHitTransform, surfaceMask, out _)) return false;

                bool hasValidHit = false;
                int bestRank = int.MaxValue;
                float bestY = float.NegativeInfinity;
                float bestSlope = 0f;

                for (int h = 0; h < hitCount; h++)
                {
                    var hit = _surfaceRaycastHits[h];
                    var collider = hit.collider;
                    if (collider == null) continue;
                    if (IsIgnoredPlacementCollider(collider)) continue;

                    if (!TryGetAllowedLayerFromHierarchy(collider.transform, surfaceMask, out int allowedLayer))
                    {
                        continue;
                    }

                    int rank = GetSurfaceLayerRank(allowedLayer, roadLayer, groundLayer);
                    float y = hit.point.y;
                    if (rank < bestRank || (rank == bestRank && y > bestY))
                    {
                        bestRank = rank;
                        bestY = y;
                        bestSlope = Vector3.Angle(hit.normal, Vector3.up);
                        hasValidHit = true;
                    }
                }

                if (!hasValidHit) return false;

                if (bestY < minY) minY = bestY;
                if (bestY > maxY) maxY = bestY;
                if (bestSlope > maxObservedSlope) maxObservedSlope = bestSlope;
            }

            if (float.IsInfinity(minY) || float.IsInfinity(maxY)) return false;
            if ((maxY - minY) > maxDelta) return false;
            if (maxObservedSlope > maxSlope) return false;

            placementY = maxY;
            return true;
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

        private static bool TryGetAllowedLayerFromHierarchy(Transform transform, LayerMask allowedMask, out int allowedLayer)
        {
            allowedLayer = -1;
            while (transform != null)
            {
                int layer = transform.gameObject.layer;
                if (((1 << layer) & allowedMask.value) != 0)
                {
                    allowedLayer = layer;
                    return true;
                }
                transform = transform.parent;
            }
            return false;
        }

        private static int GetSurfaceLayerRank(int layer, int roadLayer, int groundLayer)
        {
            if (layer == roadLayer) return 0;
            if (layer == groundLayer) return 1;
            return 2;
        }

        // 尝试放置建筑：写入占用表并可选实例化预制体（中文注释）
        public bool TryPlace(BuildingDefinition definition, Vector2Int anchorCell, GridRotation rotation, LayerMask surfaceMask, out int instanceId)
        {
            instanceId = 0;
            if (!CanPlace(definition, anchorCell, rotation)) return false;
            if (!IsSurfaceValid(definition, anchorCell, rotation, surfaceMask, out float placementY)) return false;

            instanceId = _nextInstanceId++;

            var placed = new PlacedBuilding
            {
                Definition = definition,
                AnchorCell = anchorCell,
                Rotation = rotation,
                Instance = null
            };

            Occupy(instanceId, definition, anchorCell, rotation);

            if (definition.Prefab != null)
            {
                Quaternion rot = RotationToWorld(rotation);
                Vector3 pivotOffset = definition.Prefab.transform.localPosition;
                Vector3 pos = CellToWorldCenter(anchorCell) + (rot * pivotOffset);
                pos.y = placementY + pivotOffset.y + definition.PlacementYOffsetMeters;
                Quaternion finalRotation = rot * definition.Prefab.transform.localRotation;
                placed.Instance = Instantiate(definition.Prefab, pos, finalRotation);
            }

            _placed.Add(instanceId, placed);
            return true;
        }

        // 尝试移除建筑：清理占用并销毁实例（中文注释）
        public bool TryRemove(int instanceId)
        {
            if (!_placed.TryGetValue(instanceId, out var placed)) return false;

            Vacate(instanceId, placed.Definition, placed.AnchorCell, placed.Rotation);

            if (placed.Instance != null)
            {
                Destroy(placed.Instance);
            }

            _placed.Remove(instanceId);
            return true;
        }

        // 查询某个格子当前被哪个建筑实例占用（中文注释）
        public bool TryGetInstanceIdAtCell(Vector2Int cell, out int instanceId)
        {
            return _occupiedByInstance.TryGetValue(cell, out instanceId);
        }

        // 世界坐标转为格坐标（取floor，适用于落点到格子映射）（中文注释）
        public Vector2Int WorldToCell(Vector3 world)
        {
            if (config == null) return default;

            Vector3 local = world - config.OriginWorld;
            float size = config.CellSizeMeters;
            return new Vector2Int(Mathf.FloorToInt(local.x / size), Mathf.FloorToInt(local.z / size));
        }

        // 格坐标转为世界中心点（用于放置实例位置）（中文注释）
        public Vector3 CellToWorldCenter(Vector2Int cell)
        {
            if (config == null) return Vector3.zero;
            float size = config.CellSizeMeters;
            return config.OriginWorld + new Vector3((cell.x + 0.5f) * size, 0f, (cell.y + 0.5f) * size);
        }

        // 计算Footprint的世界包围盒（当前用于预览高亮矩形）（中文注释）
        public void GetFootprintWorldBounds(BuildingDefinition definition, Vector2Int anchorCell, GridRotation rotation, out Vector3 worldMin, out Vector3 worldMax)
        {
            worldMin = Vector3.zero;
            worldMax = Vector3.zero;
            if (definition == null || config == null || definition.FootprintOffsets == null || definition.FootprintOffsets.Length == 0)
            {
                return;
            }

            float size = config.CellSizeMeters;

            Vector2Int min = new Vector2Int(int.MaxValue, int.MaxValue);
            Vector2Int max = new Vector2Int(int.MinValue, int.MinValue);

            var offsets = definition.FootprintOffsets;
            for (int i = 0; i < offsets.Length; i++)
            {
                Vector2Int c = anchorCell + RotateOffset(offsets[i], rotation);
                if (c.x < min.x) min.x = c.x;
                if (c.y < min.y) min.y = c.y;
                if (c.x > max.x) max.x = c.x;
                if (c.y > max.y) max.y = c.y;
            }

            Vector3 minCorner = config.OriginWorld + new Vector3(min.x * size, 0f, min.y * size);
            Vector3 maxCorner = config.OriginWorld + new Vector3((max.x + 1) * size, 0f, (max.y + 1) * size);

            worldMin = minCorner;
            worldMax = maxCorner;
        }

        // 将局部偏移根据旋转映射到新的偏移（仅支持90度整数旋转）（中文注释）
        public Vector2Int RotateOffset(Vector2Int offset, GridRotation rotation)
        {
            switch (rotation)
            {
                case GridRotation.R90:
                    return new Vector2Int(offset.y, -offset.x);
                case GridRotation.R180:
                    return new Vector2Int(-offset.x, -offset.y);
                case GridRotation.R270:
                    return new Vector2Int(-offset.y, offset.x);
                default:
                    return offset;
            }
        }

        // 将网格旋转转换为世界Y轴旋转（中文注释）
        public Quaternion RotationToWorld(GridRotation rotation)
        {
            float degrees = rotation switch
            {
                GridRotation.R90 => 90f,
                GridRotation.R180 => 180f,
                GridRotation.R270 => 270f,
                _ => 0f
            };
            return Quaternion.Euler(0f, degrees, 0f);
        }

        // 写入占用表（中文注释）
        private void Occupy(int instanceId, BuildingDefinition definition, Vector2Int anchorCell, GridRotation rotation)
        {
            var offsets = definition.FootprintOffsets;
            for (int i = 0; i < offsets.Length; i++)
            {
                Vector2Int cell = anchorCell + RotateOffset(offsets[i], rotation);
                _occupiedByInstance[cell] = instanceId;
            }
        }

        // 清理占用表（中文注释）
        private void Vacate(int instanceId, BuildingDefinition definition, Vector2Int anchorCell, GridRotation rotation)
        {
            var offsets = definition.FootprintOffsets;
            for (int i = 0; i < offsets.Length; i++)
            {
                Vector2Int cell = anchorCell + RotateOffset(offsets[i], rotation);
                if (_occupiedByInstance.TryGetValue(cell, out int existing) && existing == instanceId)
                {
                    _occupiedByInstance.Remove(cell);
                }
            }
        }
    }
}
