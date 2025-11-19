using UnityEngine;

/// <summary>
/// 使用 ScriptableObject 定义敌人的基础属性，方便在编辑器中创建和调整不同类型的敌人。
/// </summary>
[CreateAssetMenu(fileName = "NewEnemyStats", menuName = "Enemy/Enemy Stats Data")]
public class ScriptableEnemy : ScriptableObject
{
    [Header("Base Combat Stats")]
    /// <summary>
    /// 敌人的最大生命值。
    /// </summary>
    [Tooltip("敌人的最大生命值。")]
    public float MaxHealth = 100f;
    
    /// <summary>
    /// 敌人的最大眩晕度（即眩晕条满值）。
    /// </summary>
    [Tooltip("敌人的最大眩晕度（即眩晕条满值）。")]
    public float MaxStunValue = 50f;
    
    /// <summary>
    /// 眩晕度随时间自动下降的速度（每秒）。
    /// </summary>
    [Tooltip("眩晕度随时间自动下降的速度（每秒）。")]
    public float StunDecayRate = 10f;

    [Header("Movement & AI")]
    /// <summary>
    /// 敌人的移动速度。
    /// </summary>
    // 示例：可以添加更多区分不同敌人的属性
    public float MovementSpeed = 5f;

    /// <summary>
    /// 敌人的攻击范围。
    /// </summary>
    public float AttackRange = 2f;
}