using System.Collections.Generic;
using UnityEngine;

namespace WF.Gameplay.Systems.LootSystem
{
    public class ZoneManager : MonoBehaviour
    {
        [SerializeField] private string zoneId = "Zone";
        [Min(1)]
        [SerializeField] private int zoneLevel = 1;
        [SerializeField] private Collider zoneCollider;
        [SerializeField] private bool autoCollectOnEnable = true;
        [SerializeField] private List<LootSpawnPoint> points = new List<LootSpawnPoint>();

        public string ZoneId => zoneId;
        public int ZoneLevel => Mathf.Max(1, zoneLevel);
        public IReadOnlyList<LootSpawnPoint> Points => points;

        private void OnEnable()
        {
            if (zoneCollider == null) zoneCollider = GetComponent<Collider>();
            if (!autoCollectOnEnable) return;
            CollectPointsInBounds();
        }

        [ContextMenu("Collect Points In Bounds")]
        public void CollectPointsInBounds()
        {
            if (zoneCollider == null) zoneCollider = GetComponent<Collider>();
            if (zoneCollider == null) return;

            var bounds = zoneCollider.bounds;
            points.Clear();

            var found = FindObjectsByType<LootSpawnPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < found.Length; i++)
            {
                var p = found[i];
                if (p == null) continue;
                if (!bounds.Contains(p.transform.position)) continue;
                points.Add(p);
                p.SetZone(this);
            }
        }

        private void OnDrawGizmos()
        {
            if (zoneCollider == null) zoneCollider = GetComponent<Collider>();
            if (zoneCollider == null) return;

            var bounds = zoneCollider.bounds;
            var color = GetZoneColor(ZoneLevel);

            Gizmos.color = new Color(color.r, color.g, color.b, 0.15f);
            Gizmos.DrawCube(bounds.center, bounds.size);

            Gizmos.color = new Color(color.r, color.g, color.b, 0.9f);
            Gizmos.DrawWireCube(bounds.center, bounds.size);

            if (points == null) return;
            for (int i = 0; i < points.Count; i++)
            {
                var p = points[i];
                if (p == null) continue;
                Gizmos.DrawLine(bounds.center, p.transform.position);
            }
        }

        private static Color GetZoneColor(int level)
        {
            int clamped = Mathf.Clamp(level, 1, 5);
            float t = (clamped - 1) / 4f;
            return Color.Lerp(new Color(0.25f, 0.9f, 0.35f, 1f), new Color(0.95f, 0.25f, 0.25f, 1f), t);
        }
    }
}

