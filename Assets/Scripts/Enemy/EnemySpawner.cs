
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 负责在指定区域生成敌人，并为每个敌人绑定一个状态UI。
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnArea
    {
        public string areaName;
        public List<Transform> spawnPoints;
    }

    [Tooltip("敌人预制体")]
    [SerializeField] private GameObject enemyPrefab;
    [Tooltip("可供生成的多种敌人预制体列表")]
    [SerializeField] private List<GameObject> enemyPrefabs;

    [Tooltip("状态UI预制体")]
    [SerializeField] private EnemyStatusUI statusUIPrefab;

    [Tooltip("生成区域列表")]
    [SerializeField] private List<SpawnArea> spawnAreas;

    [Tooltip("UI实例的父物体，通常是一个Canvas")]
    [SerializeField] private Transform uiParent;

    [Tooltip("初始生成的敌人数量")]
    [SerializeField] private int initialSpawnCount = 5;

    private Dictionary<Transform, EnemyCombatController> _occupiedPoints = new Dictionary<Transform, EnemyCombatController>();

    private void Start()
    {
        bool hasSingle = enemyPrefab != null;
        bool hasList = enemyPrefabs != null && enemyPrefabs.Count > 0;
        if (!hasSingle && !hasList)
        {
            Debug.LogError("未设置任何敌人预制体！", this);
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

        SpawnInitialEnemies();
    }

    /// <summary>
    /// 生成初始数量的敌人。
    /// </summary>
    private void SpawnInitialEnemies()
    {
        for (int i = 0; i < initialSpawnCount; i++)
        {
            SpawnEnemy();
        }
    }

    /// <summary>
    /// 在随机的一个生成点生成一个敌人，并为其绑定UI。
    /// </summary>
    private void SpawnEnemy()
    {
        Transform spawnPoint = GetRandomSpawnPoint();
        if (spawnPoint == null)
        {
            Debug.LogWarning("没有可用的生成点。", this);
            return;
        }

        GameObject prefabToSpawn = GetRandomEnemyPrefab();
        GameObject enemyInstance = null;
        if (WF.Gameplay.PoolManager.Instance != null)
        {
            enemyInstance = WF.Gameplay.PoolManager.Instance.Get(prefabToSpawn, spawnPoint.position, spawnPoint.rotation);
        }
        if (enemyInstance == null)
        {
            enemyInstance = Instantiate(prefabToSpawn, spawnPoint.position, spawnPoint.rotation);
            var po = enemyInstance.GetComponent<WF.Gameplay.PooledObject>();
            if (po == null) po = enemyInstance.AddComponent<WF.Gameplay.PooledObject>();
            po.SourcePrefab = prefabToSpawn;
        }
        if (enemyInstance.GetComponent<WF.Gameplay.BuffManager>() == null)
        {
            enemyInstance.AddComponent<WF.Gameplay.BuffManager>();
        }
        EnemyCombatController combatController = enemyInstance.GetComponent<EnemyCombatController>();

        if (combatController == null)
        {
            Debug.LogError("敌人预制体上缺少 EnemyCombatController 组件！", enemyInstance);
            Destroy(enemyInstance);
            return;
        }

        EnemyStatusUI uiInstance = null;
        GameObject uiGO = null;
        if (WF.Gameplay.PoolManager.Instance != null && statusUIPrefab != null)
        {
            uiGO = WF.Gameplay.PoolManager.Instance.Get(statusUIPrefab.gameObject, Vector3.zero, Quaternion.identity, uiParent);
            if (uiGO != null)
            {
                uiInstance = uiGO.GetComponent<EnemyStatusUI>();
                if (uiInstance == null)
                {
                    uiInstance = uiGO.AddComponent<EnemyStatusUI>();
                }
            }
        }
        if (uiInstance == null)
        {
            uiInstance = Instantiate(statusUIPrefab, uiParent);
            var po = uiInstance.gameObject.GetComponent<WF.Gameplay.PooledObject>();
            if (po == null) po = uiInstance.gameObject.AddComponent<WF.Gameplay.PooledObject>();
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
            if (WF.Gameplay.PoolManager.Instance != null)
            {
                WF.Gameplay.PoolManager.Instance.Release(enemyInstance);
            }
            else
            {
                Destroy(enemyInstance);
            }
            if (uiGO != null)
            {
                if (WF.Gameplay.PoolManager.Instance != null)
                {
                    WF.Gameplay.PoolManager.Instance.Release(uiGO);
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

    private GameObject GetRandomEnemyPrefab()
    {
        if (enemyPrefabs != null && enemyPrefabs.Count > 0)
        {
            int index = Random.Range(0, enemyPrefabs.Count);
            GameObject p = enemyPrefabs[index];
            return p != null ? p : enemyPrefab;
        }
        return enemyPrefab;
    }
}
