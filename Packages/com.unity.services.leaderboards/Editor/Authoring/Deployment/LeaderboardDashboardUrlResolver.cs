using System;
using System.Threading;
using System.Threading.Tasks;
using Unity.Services.Core.Editor.Environments;
using Unity.Services.Core.Editor.OrganizationHandler;
using Unity.Services.Leaderboards.Authoring.Core.Service;
using Unity.Services.Leaderboards.Editor.Authoring.Logging;
using UnityEditor;

namespace Unity.Services.Leaderboards.Editor.Authoring.Deployment
{
    interface ILeaderboardsDashboardUrlResolver
    {
        Task<string> Leaderboard(string name);
    }

    class LeaderboardsDashboardUrlResolver : ILeaderboardsDashboardUrlResolver
    {
        readonly ILogger m_Logger;
        readonly IEnvironmentsApi m_EnvironmentsApi;
        readonly IOrganizationHandler m_OrganizationHandler;
        readonly ILeaderboardsClient m_LeaderboardsClient;

        public LeaderboardsDashboardUrlResolver(
            ILogger logger,
            IEnvironmentsApi environmentsApi,
            IOrganizationHandler organizationHandler,
            ILeaderboardsClient leaderboardsClient)
        {
            m_Logger = logger;
            m_EnvironmentsApi = environmentsApi;
            m_OrganizationHandler  = organizationHandler;
            m_LeaderboardsClient = leaderboardsClient;
        }

        string GetBaseUrl()
        {
            var projectId = CloudProjectSettings.projectId;
            var envId = m_EnvironmentsApi.ActiveEnvironmentId;
            var orgId = m_OrganizationHandler.Key;
            return $"https://cloud.unity.com/home/organizations/{orgId}/projects/{projectId}/environments/{envId}";
        }

        public async Task<string> Leaderboard(string name)
        {
            var url = GetBaseUrl();
            try
            {
                // check existence of item
                await m_LeaderboardsClient.Get(name, CancellationToken.None);
            }
            catch (Exception)
            {
                // fallback to generic url
                m_Logger.LogError($"Leaderboard '{name}' could not be found, has it been deployed to this environment?");
                return $"{url}/leaderboards/overview";
            }
            // return item url
            return $"{url}/leaderboards/leaderboard/{name}";
        }
    }
}
