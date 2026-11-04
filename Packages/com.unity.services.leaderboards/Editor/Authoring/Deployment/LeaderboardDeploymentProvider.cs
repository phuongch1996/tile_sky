using System.Collections.ObjectModel;
using Unity.Services.DeploymentApi.Editor;
using Unity.Services.Leaderboards.Editor.Authoring.Commands;
using UnityEditor;
using UnityEngine;

namespace Unity.Services.Leaderboards.Editor.Authoring.Deployment
{
    class LeaderboardDeploymentProvider : DeploymentProvider
    {
        public override string Service => L10n.Tr("Leaderboards");

        public override Command DeployCommand { get; }

        public LeaderboardDeploymentProvider(
            DeployCommand deployCommand,
            OpenLeaderboardDashboardCommand openLeaderboardDashboardCommand,
            ObservableCollection<IDeploymentItem> deploymentItems,
            ResetLeaderboardCommand resetLeaderboardCommand)
            : base(deploymentItems)
        {
            DeployCommand = deployCommand;
            Commands.Add(openLeaderboardDashboardCommand);
            Commands.Add(resetLeaderboardCommand);
        }
    }
}
