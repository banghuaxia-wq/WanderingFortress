namespace WF.Gameplay.Core.Data
{
    /// <summary>
    /// 定义了Hatch的几种可能状态。
    /// </summary>
    public enum HatchState 
    {
        /// <summary>
        /// 激活状态，Hatch可以自由移动和攻击。
        /// </summary>
        Active,
        /// <summary>
        /// 眩晕状态，Hatch无法行动，此时可以被驯服。
        /// </summary>
        Stunned,
        /// <summary>
        /// 盟友状态，Hatch已被玩家驯服，成为伙伴。
        /// </summary>
        Ally
    }
}

