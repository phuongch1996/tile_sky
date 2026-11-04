namespace Unity.Services.Leaderboards.Editor.Authoring.Analytics
{
    interface ILeaderboardsEditorAnalytics
    {
        public void SendEvent(
            string action,
            string context = default,
            long duration = default,
            string exception = default);
    }
}
