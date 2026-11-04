using Unity.Services.Leaderboards;

namespace Unity.Services.Core
{
    /// <summary>
    /// Leaderboards extension methods
    /// </summary>
    public static class UnityServicesExtensions
    {
        /// <summary>
        /// Retrieve the leaderboards service from the core service registry
        /// </summary>
        /// <param name="unityServices">The core services instance</param>
        /// <returns>The leaderboards service instance</returns>
        public static ILeaderboardsService GetLeaderboardsService(this IUnityServices unityServices)
        {
            return unityServices.GetService<ILeaderboardsService>();
        }
    }
}
