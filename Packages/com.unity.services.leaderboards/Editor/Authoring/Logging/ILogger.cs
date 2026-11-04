namespace Unity.Services.Leaderboards.Editor.Authoring.Logging
{
    interface ILogger
    {
        void LogError(object message);
        void LogWarning(object message);
        void LogInfo(object message);
        void LogVerbose(object message);
    }
}
