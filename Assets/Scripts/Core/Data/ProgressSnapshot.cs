namespace WF.Gameplay.Core.Data
{
    public struct ProgressSnapshot
    {
        public string ProgressId;
        public ProgressViewMode ViewMode;
        public string Title;
        public float Progress01;
        public bool CanCancel;
        public ProgressSnapshot(string progressId, ProgressViewMode viewMode, string title, float progress01, bool canCancel)
        {
            ProgressId = progressId;
            ViewMode = viewMode;
            Title = title;
            Progress01 = progress01;
            CanCancel = canCancel;
        }
    }
}
