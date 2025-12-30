using UnityEngine;
using WF.Gameplay.Core.Data;
using WF.Gameplay.Systems.Player;

namespace WF.Gameplay.Systems.Enemy
{
    // 敌人角色行为中心：聚合感知/记忆/移动/战斗/反应，供行为树调用（中文注释）
    public class EnemyActor : MonoBehaviour
    {
        private const float SurpriseBtHoldSeconds = 0.12f;

        [Header("Target")]
        [SerializeField] private Transform target; // 目标（通常是玩家）（中文注释）
        [SerializeField] private bool autoResolvePlayerTarget = true; // 自动绑定玩家为目标（中文注释）

        [Header("Mode")]
        [SerializeField] private EnemyAttackMode attackMode = EnemyAttackMode.Melee; // 攻击模式（中文注释）

        [Header("Modules")]
        [SerializeField] private EnemyVisionSensor visionSensor; // 视觉传感器（中文注释）
        [SerializeField] private EnemyHearingReceiver hearingReceiver; // 听觉接收器（中文注释）
        [SerializeField] private EnemyTargetMemory targetMemory; // 目标记忆（中文注释）
        [SerializeField] private EnemyNavMovementController movement; // 移动控制器（中文注释）
        [SerializeField] private EnemyPatrolController patrol; // 巡逻控制器（中文注释）
        [SerializeField] private EnemySearchController search; // 搜索控制器（中文注释）
        [SerializeField] private EnemyCombatController combat; // 战斗控制器（中文注释）
        [SerializeField] private EnemyReactionController reaction; // 反应控制器（惊吓/受击/霸体）（中文注释）

        private float _nextVisionCheckTime; // 下次视觉检测时间（中文注释）
        private float _surpriseBtEndTime;

        public Transform Target => target; // 当前目标（中文注释）
        public EnemyAttackMode AttackMode => attackMode; // 攻击模式（中文注释）
        public EnemyTargetMemory Memory => targetMemory; // 目标记忆模块（中文注释）
        public EnemyNavMovementController Movement => movement; // 移动模块（中文注释）
        public EnemyPatrolController Patrol => patrol; // 巡逻模块（中文注释）
        public EnemySearchController Search => search; // 搜索模块（中文注释）
        public EnemyCombatController Combat => combat; // 战斗模块（中文注释）
        public EnemyReactionController Reaction => reaction; // 反应模块（中文注释）

        private void Awake()
        {
            if (visionSensor == null) visionSensor = GetComponent<EnemyVisionSensor>();
            if (hearingReceiver == null) hearingReceiver = GetComponent<EnemyHearingReceiver>();
            if (targetMemory == null) targetMemory = GetComponent<EnemyTargetMemory>();
            if (movement == null) movement = GetComponent<EnemyNavMovementController>();
            if (patrol == null) patrol = GetComponent<EnemyPatrolController>();
            if (search == null) search = GetComponent<EnemySearchController>();
            if (combat == null) combat = GetComponent<EnemyCombatController>();
            if (reaction == null) reaction = GetComponent<EnemyReactionController>();

            if (hearingReceiver != null && targetMemory != null)
            {
                hearingReceiver.BindMemory(targetMemory);
            }

            if (combat != null)
            {
                combat.SetAttackMode(attackMode);
            }
        }

        private void Start()
        {
            if (autoResolvePlayerTarget)
            {
                ResolvePlayerTarget();
            }
        }

        private void Update()
        {
            if (autoResolvePlayerTarget && target == null)
            {
                ResolvePlayerTarget();
            }

            if (targetMemory != null)
            {
                targetMemory.Tick();
            }

            TickVision();

            if (targetMemory != null && targetMemory.ShouldTriggerSurprise)
            {
                if (reaction != null)
                {
                    reaction.TriggerSurprise(targetMemory.Target != null ? targetMemory.Target : target);
                }

                _surpriseBtEndTime = Time.time + SurpriseBtHoldSeconds;
                targetMemory.ConsumeSurprise();
            }
        }

        // 行为树：是否存在可用目标信息（看见/听见/记忆未过期）（中文注释）
        public bool HasTargetInfo()
        {
            return targetMemory != null && targetMemory.HasTargetInfo;
        }

        // 行为树：当前是否“看见”（含丢失延迟窗口）（中文注释）
        public bool HasVision()
        {
            if (targetMemory == null || visionSensor == null) return false;
            return targetMemory.HasVisionWithinLoseDelay(visionSensor.VisionLoseDelaySeconds);
        }

        // 行为树：获取最后已知位置（用于追击/搜索/不可见开火）（中文注释）
        public Vector3 GetLastKnownPosition()
        {
            if (targetMemory == null) return transform.position;
            return targetMemory.LastKnownPosition;
        }

        // 行为树：朝向目标/位置（中文注释）
        public void FaceToLastKnownPosition()
        {
            FaceToPosition(GetLastKnownPosition());
        }

        // 行为树：朝向目标（中文注释）
        public void FaceToTarget()
        {
            if (target == null) return;
            FaceToPosition(target.position);
        }

        // 行为树：尝试攻击（近战/远程由配置决定）（中文注释）
        public bool TryAttack()
        {
            if (combat == null) return false;

            Vector3 aimPosition = GetLastKnownPosition();
            bool hasVision = HasVision();
            return combat.TryAttack(aimPosition, targetMemory != null ? targetMemory.Target : target, hasVision);
        }

        // 行为树：是否进入攻击范围（近战=目标距离，远程=最后已知点距离）（中文注释）
        public bool IsInAttackRange()
        {
            if (combat == null) return false;

            if (attackMode == EnemyAttackMode.Melee)
            {
                var t = targetMemory != null && targetMemory.Target != null ? targetMemory.Target : target;
                return combat.IsInMeleeRange(t);
            }

            return combat.IsInRangedRange(GetLastKnownPosition());
        }

        // 行为树：是否请求进入受击状态（中文注释）
        public bool IsHurtRequested()
        {
            return reaction != null && reaction.IsHurtRequested;
        }

        // 行为树：消费受击请求（进入Hurt节点时调用）（中文注释）
        public void ConsumeHurtRequest()
        {
            if (reaction == null) return;
            reaction.ConsumeHurtRequest();
        }

        // 行为树：进入受击减速（用于受击僵直期间“有移动则稍微减速”）（中文注释）
        public void EnterHurtSlow()
        {
            if (movement == null) return;
            movement.EnterHurtSlow();
        }

        // 行为树：退出受击减速并恢复正常速度（中文注释）
        public void ExitHurtSlow()
        {
            if (movement == null) return;
            movement.ExitHurtSlow();
        }


        // 行为树：是否处于惊吓（中文注释）
        public bool IsSurprising()
        {
            return reaction != null && reaction.IsSurprising && Time.time < _surpriseBtEndTime;
        }

        // 行为树：巡逻选择下一个点并移动（中文注释）
        public bool PatrolSelectNextPointAndMove()
        {
            if (patrol == null) return false;
            return patrol.SelectNextPatrolPointAndMove();
        }

        // 行为树：开始巡逻停留计时（中文注释）
        public void PatrolStartIdle()
        {
            if (patrol == null) return;
            patrol.StartIdle();
        }

        // 行为树：巡逻停留是否结束（中文注释）
        public bool PatrolIsIdleDone()
        {
            return patrol != null && patrol.IsIdleDone();
        }

        // 行为树：巡逻是否到达（中文注释）
        public bool PatrolIsArrived()
        {
            return patrol != null && patrol.IsArrived();
        }

        public bool PatrolHasPoint()
        {
            return patrol != null && patrol.HasPatrolPoint;
        }

        // 行为树：清空巡逻点（中文注释）
        public void PatrolClearPoint()
        {
            if (patrol == null) return;
            patrol.ClearPatrolPoint();
        }

        // 行为树：开始搜索（中文注释）
        public void SearchBegin()
        {
            if (search == null) return;
            search.BeginSearch();
        }

        // 行为树：搜索是否结束（中文注释）
        public bool SearchIsDone()
        {
            return search == null || search.IsSearchDone();
        }

        // 行为树：搜索旋转Tick（中文注释）
        public void SearchTickRotate()
        {
            if (search == null) return;
            search.TickRotate();
        }

        // 行为树：移动到最后已知位置（中文注释）
        public void MoveToLastKnownPosition(EnemyMoveSpeedProfile profile)
        {
            if (movement == null) return;
            movement.MoveTo(GetLastKnownPosition(), profile);
        }

        // 行为树：停止移动（中文注释）
        public void StopMove()
        {
            if (movement == null) return;
            movement.Stop();
        }

        // 行为树：是否到达当前移动目标（中文注释）
        public bool IsArrived()
        {
            return movement != null && movement.IsArrived();
        }

        // 受击入口：由伤害接收器转发（中文注释）
        public void NotifyDamaged(WF.Gameplay.Core.Data.DamageInfo damageInfo)
        {
            if (reaction == null) return;
            reaction.RequestHurt(damageInfo);
        }

        private void TickVision()
        {
            if (visionSensor == null || targetMemory == null) return;
            if (target == null && targetMemory.Target == null) return;

            float now = Time.time;
            if (now < _nextVisionCheckTime) return;

            _nextVisionCheckTime = now + Mathf.Max(0.02f, visionSensor.CheckIntervalSeconds);

            var t = targetMemory.Target != null ? targetMemory.Target : target;
            bool canSeeNow = t != null && visionSensor.CanSee(t);

            targetMemory.SetVisionResult(t, canSeeNow);
        }

        private void ResolvePlayerTarget()
        {
            if (target != null) return;

            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) target = player.transform;
            if (target != null) return;

            var move = FindObjectOfType<PlayerMove>();
            if (move != null) target = move.transform;
        }

        private void FaceToPosition(Vector3 worldPosition)
        {
            Vector3 dir = worldPosition - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f) return;
            transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
        }
    }
}
