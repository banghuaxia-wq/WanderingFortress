using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyStats", menuName = "Enemy/Enemy Stats Data")]
public class ScriptableEnemy : ScriptableObject
{
    [Header("Base Combat Stats")]
    [Tooltip("敌人的最大生命值。")]
    public float MaxHealth = 100f;
    
    [Tooltip("敌人的最大眩晕度（即眩晕条满值）。")]
    public float MaxStunValue = 50f;
    
    [Tooltip("眩晕度随时间自动下降的速度（每秒）。")]
    public float StunDecayRate = 10f;

    [Header("Movement & AI")]
    // 示例：可以添加更多区分不同敌人的属性
    public float MovementSpeed = 5f;
    public float AttackRange = 2f;
}