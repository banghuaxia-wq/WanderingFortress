using UnityEngine;

namespace WF.Gameplay.Systems.Enemy
{
    // 敌人目标记忆：记录最后看见/听见位置与时间，提供“目标信息是否有效”的判断（中文注释）
    public class EnemyTargetMemory : MonoBehaviour
    {
        [Header("Memory")]
        [SerializeField] private float forgetAfterSeconds = 10f; // 信息过期时间（中文注释）

        private Transform _target; // 当前目标（中文注释）
        private Vector3 _lastSeenPosition; // 最后看见位置（中文注释）
        private float _lastSeenTime = float.NegativeInfinity; // 最后看见时间（中文注释）
        private Vector3 _lastHeardPosition; // 最后听见位置（中文注释）
        private float _lastHeardTime = float.NegativeInfinity; // 最后听见时间（中文注释）
        private bool _canSeeNow; // 本次视觉检测结果（中文注释）
        private bool _surpriseConsumed; // 本轮目标获取是否已触发惊吓（中文注释）

        public Transform Target => _target; // 当前目标（中文注释）
        public bool CanSeeNow => _canSeeNow; // 当前是否直接看见（不含丢失延迟）（中文注释）

        public float LastSeenTime => _lastSeenTime; // 最后看见时间（中文注释）
        public float LastHeardTime => _lastHeardTime; // 最后听见时间（中文注释）

        // 是否仍有可用目标信息（看见/听见任一未过期）（中文注释）
        public bool HasTargetInfo
        {
            get
            {
                float now = Time.time;
                float last = Mathf.Max(_lastSeenTime, _lastHeardTime);
                return now - last <= Mathf.Max(0f, forgetAfterSeconds);
            }
        }

        // 最后已知位置（优先看见，其次听见）（中文注释）
        public Vector3 LastKnownPosition
        {
            get
            {
                if (_lastSeenTime >= _lastHeardTime) return _lastSeenPosition;
                return _lastHeardPosition;
            }
        }

        // 是否需要触发惊吓（同一轮目标获取只触发一次）（中文注释）
        public bool ShouldTriggerSurprise => HasTargetInfo && !_surpriseConsumed;

        // 更新（清理过期并在过期时重置惊吓标记）（中文注释）
        public void Tick()
        {
            if (HasTargetInfo) return;
            Clear();
        }

        // 写入视觉检测结果（中文注释）
        public void SetVisionResult(Transform target, bool canSeeNow)
        {
            if (target != null && _target != target)
            {
                SetTarget(target);
            }

            _canSeeNow = canSeeNow;
            if (!canSeeNow) return;

            _lastSeenTime = Time.time;
            _lastSeenPosition = target != null ? target.position : _lastSeenPosition;
        }

        // 记录听觉信息（中文注释）
        public void RecordHeard(Vector3 position, Transform source)
        {
            if (source != null && _target != source)
            {
                SetTarget(source);
            }

            _lastHeardTime = Time.time;
            _lastHeardPosition = position;
        }

        // 消费惊吓触发（中文注释）
        public void ConsumeSurprise()
        {
            _surpriseConsumed = true;
        }

        // 判断是否处于“视觉丢失延迟窗口内”（中文注释）
        public bool HasVisionWithinLoseDelay(float loseDelaySeconds)
        {
            if (_canSeeNow) return true;
            return Time.time - _lastSeenTime <= Mathf.Max(0f, loseDelaySeconds);
        }

        // 手动设置目标（中文注释）
        public void SetTarget(Transform target)
        {
            _target = target;
            _surpriseConsumed = false;
        }

        // 清空目标信息（中文注释）
        public void Clear()
        {
            _target = null;
            _canSeeNow = false;
            _lastSeenTime = float.NegativeInfinity;
            _lastHeardTime = float.NegativeInfinity;
            _lastSeenPosition = transform.position;
            _lastHeardPosition = transform.position;
            _surpriseConsumed = false;
        }
    }
}

