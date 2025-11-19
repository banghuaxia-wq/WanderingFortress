/// <summary>
/// 定义了敌人的几种可能状态。
/// </summary>
public enum EnemyState 
{
    /// <summary>
    /// 激活状态，敌人可以自由移动和攻击。
    /// </summary>
    Active,
    /// <summary>
    /// 眩晕状态，敌人无法行动，此时可以被驯服。
    /// </summary>
    Stunned,
    /// <summary>
    /// 盟友状态，敌人已被玩家驯服，成为伙伴。
    /// </summary>
    Ally
}