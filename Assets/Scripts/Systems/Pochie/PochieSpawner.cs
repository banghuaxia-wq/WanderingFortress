
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 负责在指定区域生成Pochie，并为每个Pochie绑定一个状态UI。
/// </summary>
using WF.Gameplay.Core.Utilities.Pooling;
using WF.Gameplay.UI.WorldSpaceInfo;

namespace WF.Gameplay.Systems.Pochie
{
    public class PochieSpawner : MonoBehaviour
    {
        [System.Serializable]
        public class SpawnArea
        {
            public string areaName;
            public List<Transform> spawnPoints;
        }

        [Tooltip("Pochie预制体")]
        [SerializeField] private GameObject pochiePrefab;
        [Tooltip("可供生成的多种Pochie预制体列表")]
        [SerializeField] private List<GameObject> pochiePrefabs;
        [Tooltip("可供生成的Pochie数据列表（优先使用）")]
        [SerializeField] private List<WF.Gameplay.Core.Data.SOPochie> pochieDatas;

        [Tooltip("状态UI预制体")]
        [SerializeField] private PochieStatusUI statusUIPrefab;

        [Tooltip("生成区域列表")]
        [SerializeField] private List<SpawnArea> spawnAreas;

        [Tooltip("UI实例的父物体，通常是一个Canvas")]
        [SerializeField] private Transform uiParent;

        [Tooltip("初始生成的Pochie数量")]
        [SerializeField] private int initialSpawnCount = 5;

        private Dictionary<Transform, PochieCombatController> _occupiedPoints = new Dictionary<Transform, PochieCombatController>();

        private void Start()
        {
            bool hasSingle = pochiePrefab != null;
            bool hasList = pochiePrefabs != null && pochiePrefabs.Count > 0;
            bool hasData = pochieDatas != null && pochieDatas.Count > 0;
            if (!hasSingle && !hasList && !hasData)
            {
                Debug.LogError("未设置任何Pochie数据或预制体！", this);
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

            SpawnInitialPochies();
        }

        /// <summary>
        /// 生成初始数量的Pochie。
        /// </summary>
        private void SpawnInitialPochies()
        {
            for (int i = 0; i < initialSpawnCount; i++)
            {
                SpawnPochie();
            }
        }

        /// <summary>
        /// 在随机的一个生成点生成一个Pochie，并为其绑定UI。
        /// </summary>
        private void SpawnPochie()
        {
            Transform spawnPoint = GetRandomSpawnPoint();
            if (spawnPoint == null)
            {
                Debug.LogWarning("没有可用的生成点。", this);
                return;
            }

            GameObject pochieInstance = null;
            PochieCombatController combatController = null;
            var dataToUse = GetRandomPochieData();
            if (dataToUse != null)
            {
                var go = PochieFactory.CreateFromDataStatic(dataToUse, spawnPoint.position, spawnPoint.rotation);
                pochieInstance = go;
                combatController = go != null ? go.GetComponent<PochieCombatController>() : null;
            }
            else
            {
                var prefabToSpawn = GetRandomPochiePrefab();
                var go = PochieFactory.CreateFromPrefabStatic(prefabToSpawn, null, spawnPoint.position, spawnPoint.rotation);
                pochieInstance = go;
                combatController = go != null ? go.GetComponent<PochieCombatController>() : null;
            }

            if (combatController == null || pochieInstance == null)
            {
                Debug.LogError("Pochie预制体上缺少 PochieCombatController 组件！", pochieInstance);
                Destroy(pochieInstance);
                return;
            }

            PochieStatusUI uiInstance = null;
            GameObject uiGO = null;
            if (PoolManager.Instance != null && statusUIPrefab != null)
            {
                uiGO = PoolManager.Instance.Get(statusUIPrefab.gameObject, Vector3.zero, Quaternion.identity, uiParent);
                if (uiGO != null)
                {
                    uiInstance = uiGO.GetComponent<PochieStatusUI>();
                    if (uiInstance == null)
                    {
                        uiInstance = uiGO.AddComponent<PochieStatusUI>();
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
                if (PoolManager.Instance != null)
                {
                    PoolManager.Instance.Release(pochieInstance);
                }
                else
                {
                    Destroy(pochieInstance);
                }
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

        private GameObject GetRandomPochiePrefab()
        {
            if (pochiePrefabs != null && pochiePrefabs.Count > 0)
            {
                int index = Random.Range(0, pochiePrefabs.Count);
                GameObject p = pochiePrefabs[index];
                return p != null ? p : pochiePrefab;
            }
            return pochiePrefab;
        }

        private WF.Gameplay.Core.Data.SOPochie GetRandomPochieData()
        {
            if (pochieDatas != null && pochieDatas.Count > 0)
            {
                int index = Random.Range(0, pochieDatas.Count);
                var d = pochieDatas[index];
                return d;
            }
            return null;
        }
    }
}
