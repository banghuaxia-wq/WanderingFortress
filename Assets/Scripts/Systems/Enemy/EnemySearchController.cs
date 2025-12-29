using UnityEngine;

namespace WF.Gameplay.Systems.Enemy
{
    // 敌人搜索控制器：到达最后位置后原地搜索一段时间（中文注释）
    public class EnemySearchController : MonoBehaviour
    {
        [SerializeField] private float searchSeconds = 2.0f; // 搜索持续时间（中文注释）
        [SerializeField] private float rotateDegreesPerSecond = 180f; // 搜索旋转速度（度/秒）（中文注释）

        private float _searchEndTime; // 搜索结束时间（中文注释）
        private int _rotateDir = 1; // 旋转方向（中文注释）

        public bool IsSearching => Time.time < _searchEndTime; // 是否处于搜索中（中文注释）

        // 开始搜索（中文注释）
        public void BeginSearch()
        {
            _searchEndTime = Time.time + Mathf.Max(0f, searchSeconds);
            _rotateDir = Random.value < 0.5f ? -1 : 1;
        }

        // 搜索是否结束（中文注释）
        public bool IsSearchDone()
        {
            return Time.time >= _searchEndTime;
        }

        // 搜索期间的原地旋转（中文注释）
        public void TickRotate()
        {
            if (!IsSearching) return;
            float delta = rotateDegreesPerSecond * _rotateDir * Time.deltaTime;
            transform.Rotate(0f, delta, 0f, Space.World);
        }
    }
}

