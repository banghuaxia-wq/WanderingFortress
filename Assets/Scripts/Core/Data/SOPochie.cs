using UnityEngine;

namespace WF.Gameplay.Core.Data
{

/// PochieData_SO：所有Pochie实体的通用配置模板
/// 在编辑器中通过该资产配置基础信息、驯服信息、战斗参数、子弹与动画引用。
[CreateAssetMenu(fileName = "PochieData_SO", menuName = "Pochie/Pochie Data")]
public class SOPochie : ScriptableObject
{
    [Header("基础信息")]
    [Tooltip("显示名称（例如：火龙果刺猬）")]
    public string pochieName = "火龙果刺猬";

    [Tooltip("Pochie类型（攻击/辅助/坦克/功能）")]
    public PochieType type = PochieType.Attack;

    [Tooltip("模型预制体引用（用于实例化外观）")]
    public GameObject modelPrefab;

    [Header("驯服信息")]
    [Tooltip("喜爱/驯服所需物品的资源引用（如 奶皮子糖葫芦）")]
    public Object preferredItem;

    [Tooltip("驯服所需物品数量")]
    public int tameCost = 1;

    [Tooltip("驯服方式（例如：击晕后驯服）")]
    public TameMethod tameMethod = TameMethod.Knockout;

    [Header("战斗属性")]
    [Tooltip("最大生命值（低血量示例：30）")]
    public float maxHealth = 30f;

    [Tooltip("最大眩晕值（低眩晕示例：40）")]
    public float maxStun = 40f;

    [Tooltip("眩晕值随时间自动下降的速度（每秒）")]
    public float stunDecayRate = 10f;

    [Tooltip("眩晕值随时间自动上涨的速度（每秒）")]
    public float stunAutoIncreaseRate = 0f;

    [Header("移动与攻击参数")]
    [Tooltip("移动速度（单位/秒）")]
    public float moveSpeed = 3.5f;

    [Tooltip("开始攻击玩家的距离（米）")]
    public float attackRange = 8f;

    [Tooltip("两次攻击之间的冷却时间（秒）")]
    public float attackInterval = 2.5f;

    [Tooltip("攻击伤害（例如寒冰尖刺的基础伤害）")]
    public float attackDamage = 5f;

    [Header("子弹设置")]
    [Tooltip("子弹预制体引用（例如 PF_Projectile_IceSpine）")]
    public GameObject projectilePrefab;

    [Tooltip("子弹飞行速度（直线）")]
    public float projectileSpeed = 15f;

    [Tooltip("子弹生命周期（秒），到期自动销毁")]
    public float projectileLifetime = 4f;

    [Header("动画与状态机")]
    [Tooltip("Animator 控制器（用于状态机）")]
    public RuntimeAnimatorController animatorController;

    [Tooltip("待机动画剪辑（闲置状态播放）")]
    public AnimationClip idleClip;

    [Tooltip("行走动画剪辑（移动/追击状态播放）")]
    public AnimationClip walkClip;

    [Tooltip("眩晕动画剪辑（达到最大眩晕时播放）")]
    public AnimationClip stunClip;

    [Tooltip("攻击动画剪辑（在关键帧触发发射事件）")]
    public AnimationClip attackClip;

    public float MaxHealth => maxHealth;
    public float MaxStunValue => maxStun;
    public float StunDecayRate => stunDecayRate;
    public float MovementSpeed => moveSpeed;
    public float AttackRange => attackRange;
    public float AttackInterval => attackInterval;
    public float AttackDamage => attackDamage;
}
}
