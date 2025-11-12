// 不需要继承任何东西，直接定义即可
public enum EnemyState 
{
    Active,     // 默认状态，可以移动和攻击
    Stunned,    // 眩晕状态，停止行动，可被转化
    Ally        // 已被转化，成为玩家朋友
}