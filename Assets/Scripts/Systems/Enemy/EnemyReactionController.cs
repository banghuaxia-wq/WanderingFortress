using UnityEngine;
using WF.Gameplay.Core.Data;
using WF.Gameplay.Core.Utilities.Pooling;

namespace WF.Gameplay.Systems.Enemy
{
    // 敌人反应控制器：惊吓、受击请求、霸体开关（中文注释）
    public class EnemyReactionController : MonoBehaviour
    {
        [Header("Surprise")]
        [SerializeField] private float surpriseSeconds = 0.5f; // 惊吓持续时间（中文注释）
        [SerializeField] private GameObject surpriseIconPrefab; // 惊讶图标预制体（中文注释）
        [SerializeField] private Transform surpriseIconSocket; // 图标挂点（中文注释）
        [SerializeField] private float surpriseIconLifetime = 0.8f; // 图标存在时间（中文注释）
        [SerializeField] private bool rotateToTargetDuringSurprise = true; // 惊吓期间是否持续朝向目标（中文注释）

        [Header("Animation")]
        [SerializeField] private Animator animator; // Animator引用（中文注释）
        [SerializeField] private string surpriseTrigger = "Surprise"; // 惊吓动画Trigger名（中文注释）
        [SerializeField] private string hurtTrigger = "Hurt"; // 受击动画Trigger名（中文注释）

        private float _surpriseEndTime; // 惊吓结束时间（中文注释）
        private Transform _faceTarget; // 惊吓朝向目标（中文注释）
        private GameObject _surpriseIconInstance; // 当前惊讶图标实例（中文注释）
        private float _iconDespawnTime; // 图标回收时间（中文注释）

        private bool _hurtRequested; // 是否请求进入受击状态（中文注释）
        private DamageInfo _lastDamage; // 最近一次伤害信息（中文注释）
        private bool _superArmorActive; // 霸体是否激活（中文注释）

        public bool IsSurprising => Time.time < _surpriseEndTime; // 是否处于惊吓状态（中文注释）
        public bool IsHurtRequested => _hurtRequested; // 是否有受击请求（中文注释）
        public bool IsSuperArmorActive => _superArmorActive; // 是否处于霸体（中文注释）
        public DamageInfo LastDamage => _lastDamage; // 最近伤害（用于行为树读取）（中文注释）

        private void Awake()
        {
            if (animator == null) animator = GetComponent<Animator>();
        }

        private void Update()
        {
            if (IsSurprising && rotateToTargetDuringSurprise && _faceTarget != null)
            {
                FaceTo(_faceTarget.position);
            }

            if (_surpriseIconInstance != null && Time.time >= _iconDespawnTime)
            {
                DespawnIcon();
            }
        }

        // 触发惊吓：面向目标并生成惊讶图标（中文注释）
        public void TriggerSurprise(Transform faceTarget)
        {
            _faceTarget = faceTarget;
            _surpriseEndTime = Time.time + Mathf.Max(0f, surpriseSeconds);

            if (animator != null && !string.IsNullOrEmpty(surpriseTrigger))
            {
                animator.SetTrigger(surpriseTrigger);
            }

            if (_faceTarget != null)
            {
                FaceTo(_faceTarget.position);
            }

            SpawnIcon();
        }

        // 请求受击：若非霸体则抛给行为树处理打断（中文注释）
        public void RequestHurt(DamageInfo damageInfo)
        {
            _lastDamage = damageInfo;

            if (_superArmorActive)
            {
                return;
            }

            _hurtRequested = true;

            if (animator != null && !string.IsNullOrEmpty(hurtTrigger))
            {
                animator.SetTrigger(hurtTrigger);
            }
        }

        // 消费受击请求（进入Hurt节点后调用）（中文注释）
        public void ConsumeHurtRequest()
        {
            _hurtRequested = false;
        }

        // 设置霸体（可由动画事件调用）（中文注释）
        public void SetSuperArmorActive(bool active)
        {
            _superArmorActive = active;
        }

        private void FaceTo(Vector3 worldPosition)
        {
            Vector3 dir = worldPosition - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f) return;
            transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
        }

        private void SpawnIcon()
        {
            if (surpriseIconPrefab == null) return;
            if (surpriseIconSocket == null) surpriseIconSocket = transform;

            DespawnIcon();

            Vector3 pos = surpriseIconSocket.position;
            Quaternion rot = surpriseIconSocket.rotation;

            if (PoolManager.Instance != null)
            {
                _surpriseIconInstance = PoolManager.Instance.Get(surpriseIconPrefab, pos, rot, surpriseIconSocket);
            }
            else
            {
                _surpriseIconInstance = Instantiate(surpriseIconPrefab, pos, rot, surpriseIconSocket);
            }

            _iconDespawnTime = Time.time + Mathf.Max(0f, surpriseIconLifetime);
        }

        private void DespawnIcon()
        {
            if (_surpriseIconInstance == null) return;

            var po = _surpriseIconInstance.GetComponent<PooledObject>();
            if (po != null)
            {
                po.ReturnToPool();
            }
            else
            {
                Destroy(_surpriseIconInstance);
            }

            _surpriseIconInstance = null;
        }
    }
}

