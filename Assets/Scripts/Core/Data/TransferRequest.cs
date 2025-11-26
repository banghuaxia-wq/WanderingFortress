namespace WF.Gameplay.Core.Data
{
    public enum TransferSource { Package, Box, Equipment, Hotbar }
    public class TransferRequest
    {
        public TransferSource From; // 来源类型（中文注释）
        public TransferSource To; // 目标类型（中文注释）
        public int FromIndex; // 来源槽位索引（中文注释）
        public int ToIndex; // 目标槽位索引（中文注释）
        public string FromContainerId; // 来源容器ID（当From为Box时使用）
        public string ToContainerId; // 目标容器ID（当To为Box时使用）
    }
}
