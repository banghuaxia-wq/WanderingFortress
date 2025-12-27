
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 负责在指定区域生成Hatch，并为每个Hatch绑定一个状态UI。
/// </summary>
using WF.Gameplay.Core.Utilities.Pooling;
using WF.Gameplay.UI.WorldSpaceInfo;

namespace WF.Gameplay.Systems.Hatch
{
    public class HatchSpawner : MonoBehaviour
    {
        [System.Serializable]
        public class SpawnArea
        {
            public string areaName;
            public List<Transform> spawnPoints;
        }

        [Tooltip("Hatch预制体")]
        [SerializeField] private GameObject hatchPrefab;
        [Tooltip("可供生成的多种Hatch预制体列表")]
        [SerializeField] private List<GameObject> hatchPrefabs;
        [Tooltip("可供生成的Hatch数据列表（优先使用）")]
        [SerializeField] private List<WF.Gameplay.Core.Data.SOHatch> hatchDatas;

        [Tooltip("状态UI预制体")]
        [SerializeField] private HatchStatusUI statusUIPrefab;

        [Tooltip("生成区域列表")]
        [SerializeField] private List<SpawnArea> spawnAreas;

        [Tooltip("UI实例的父物体，通常是一个Canvas")]
        [SerializeField] private Transform uiParent;

        [Tooltip("初始生成的Hatch数量")]
        [SerializeField] private int initialSpawnCount = 5;

        private Dictionary<Transform, HatchCombatController> _occupiedPoints = new Dictionary<Transform, HatchCombatController>();

        private void Start()
        {
            bool hasSingle = hatchPrefab != null;
            bool hasList = hatchPrefabs != null && hatchPrefabs.Count > 0;
            bool hasData = hatchDatas != null && hatchDatas.Count > 0;
            if (!hasSingle && !hasList && !hasData)
            {
                Debug.LogError("未设置任何Hatch数据或预制体！", this);
                return;
            }

            if (statusUIPrefab == null)
            {
                Debug.LogError("状态UI预制体未设置！", this);
                return;
            }

            if (uiParent == null)
            {
                Debug.LogError("UI父物体未设置！", this);
                return;
            }

            SpawnInitialHatchs();
        }

        /// <summary>
        /// 生成初始数量的Hatch。
        /// </summary>
        private void SpawnInitialHatchs()
        {
            for (int i = 0; i < initialSpawnCount; i++)
            {
                SpawnHatch();
            }
        }

        /// <summary>
        /// 在随机的一个生成点生成一个Hatch，并为其绑定UI。
        /// </summary>
        private void SpawnHatch()
        {
            Transform spawnPoint = GetRandomSpawnPoint();
            if (spawnPoint == null)
            {
                Debug.LogWarning("没有可用的生成点。", this);
                return;
            }

            GameObject hatchInstance = null;
            HatchCombatController combatController = null;
            var dataToUse = GetRandomHatchData();
            if (dataToUse != null)
            {
                var go = HatchService.EnsureInstance().SpawnFromData(dataToUse, spawnPoint.position, spawnPoint.rotation);
                hatchInstance = go;
                combatController = go != null ? go.GetComponent<HatchCombatController>() : null;
            }
            else
            {
                var prefabToSpawn = GetRandomHatchPrefab();
                var go = HatchService.EnsureInstance().SpawnFromPrefab(prefabToSpawn, null, spawnPoint.position, spawnPoint.rotation);
                hatchInstance = go;
                combatController = go != null ? go.GetComponent<HatchCombatController>() : null;
            }

            if (combatController == null || hatchInstance == null)
            {
                Debug.LogError("Hatch预制体上缺少 HatchCombatController 组件！", hatchInstance);
                Destroy(hatchInstance);
                return;
            }

            HatchStatusUI uiInstance = null;
            GameObject uiGO = null;
            if (PoolManager.Instance != null && statusUIPrefab != null)
            {
                uiGO = PoolManager.Instance.Get(statusUIPrefab.gameObject, Vector3.zero, Quaternion.identity, uiParent);
                if (uiGO != null)
                {
                    uiInstance = uiGO.GetComponent<HatchStatusUI>();
                    if (uiInstance == null)
                    {
                        uiInstance = uiGO.AddComponent<HatchStatusUI>();
                    }
                }
            }
            if (uiInstance == null)
            {
                uiInstance = Instantiate(statusUIPrefab, uiParent);
                var po = uiInstance.gameObject.GetComponent<PooledObject>();
                if (po == null) po = uiInstance.gameObject.AddComponent<PooledObject>();
                po.SourcePrefab = statusUIPrefab.gameObject;
                uiGO = uiInstance.gameObject;
            }

            uiInstance.Initialize(combatController);

            _occupiedPoints[spawnPoint] = combatController;
            combatController.OnDefeated += () =>
            {
                if (_occupiedPoints.ContainsKey(spawnPoint))
                {
                    _occupiedPoints.Remove(spawnPoint);
                }
                HatchService.EnsureInstance().Despawn(hatchInstance);
                if (uiGO != null)
                {
                    if (PoolManager.Instance != null)
                    {
                        PoolManager.Instance.Release(uiGO);
                    }
                    else
                    {
                        Destroy(uiGO);
                    }
                }
            };
        }

        /// <summary>
        /// 从所有生成区域中随机选择一个生成点。
        /// </summary>
        /// <returns>一个随机的生成点Transform，如果没有则返回null。</returns>
        private Transform GetRandomSpawnPoint()
        {
            List<Transform> allPoints = new List<Transform>();
            foreach (var area in spawnAreas)
            {
                allPoints.AddRange(area.spawnPoints);
            }

            if (allPoints.Count == 0)
            {
                return null;
            }
            List<Transform> available = new List<Transform>();
            for (int i = 0; i < allPoints.Count; i++)
            {
                Transform p = allPoints[i];
                if (p != null && !_occupiedPoints.ContainsKey(p))
                {
                    available.Add(p);
                }
            }

            if (available.Count == 0)
            {
                return null;
            }

            int randomIndex = Random.Range(0, available.Count);
            return available[randomIndex];
        }

        private GameObject GetRandomHatchPrefab()
        {
            if (hatchPrefabs != null && hatchPrefabs.Count > 0)
            {
                int index = Random.Range(0, hatchPrefabs.Count);
                GameObject p = hatchPrefabs[index];
                return p != null ? p : hatchPrefab;
            }
            return hatchPrefab;
        }

        private WF.Gameplay.Core.Data.SOHatch GetRandomHatchData()
        {
            if (hatchDatas != null && hatchDatas.Count > 0)
            {
                int index = Random.Range(0, hatchDatas.Count);
                var d = hatchDatas[index];
                return d;
            }
            return null;
        }
    }
}

