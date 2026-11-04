using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.Services.DeploymentApi.Editor;
using Unity.Services.Leaderboards.Authoring.Core.Model;
using UnityEditor;
using UnityEngine;

namespace Unity.Services.Leaderboards.Editor.Authoring.Deployment
{
    class OpenLeaderboardDashboardCommand : Command
    {
        readonly ILeaderboardsDashboardUrlResolver m_DashboardUrlResolver;
        public override string Name => L10n.Tr("Open in Dashboard");

        public OpenLeaderboardDashboardCommand(ILeaderboardsDashboardUrlResolver dashboardUrlResolver)
        {
            m_DashboardUrlResolver = dashboardUrlResolver;
        }

        public override async Task ExecuteAsync(IEnumerable<IDeploymentItem> items, CancellationToken cancellationToken = default)
        {
            var ids = items
                .OfType<ILeaderboardConfig>()
                .Select(i => i.Id);

            foreach (var id in ids)
            {
                Application.OpenURL(await m_DashboardUrlResolver.Leaderboard(id));
            }
        }
    }
}
