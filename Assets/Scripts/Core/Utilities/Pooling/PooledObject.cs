using UnityEngine;

namespace WF.Gameplay.Core.Utilities.Pooling
{
    public class PooledObject : MonoBehaviour
    {
        public GameObject SourcePrefab;
        public void OnSpawned() {}
        public void OnDespawned() { var p = GetComponent<WF.Gameplay.Core.Interfaces.IPoolable>(); if (p != null) p.OnRecycle(); }
        public void ReturnToPool()
        {
            if (PoolManager.Instance != null)
            {
                PoolManager.Instance.Release(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
                Destroy(gameObject);
            }
        }
    }
}
