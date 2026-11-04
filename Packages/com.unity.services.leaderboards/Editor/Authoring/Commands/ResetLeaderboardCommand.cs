using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.Services.DeploymentApi.Editor;
using Unity.Services.Leaderboards.Authoring.Core.Model;
using Unity.Services.Leaderboards.Authoring.Core.Service;
using Unity.Services.Leaderboards.Editor.Authoring.Logging;
using Unity.Services.Leaderboards.Editor.Shared.UI;
using Unity.Services.Leaderboards.Infrastructure.Collections;
using UnityEditor;

namespace Unity.Services.Leaderboards.Editor.Authoring.Commands
{
    class ResetLeaderboardCommand : Command<ILeaderboardConfig>
    {
        internal const string ResetState = "RESET_STATE";
        readonly ILogger m_Logger;
        readonly ILeaderboardsClient m_Client;
        readonly IDisplayDialog m_Dialog;
        public override string Name => L10n.Tr("Reset Leaderboard");

        public ResetLeaderboardCommand(
            ILogger logger,
            ILeaderboardsClient client,
            IDisplayDialog dialog)
        {
            m_Logger = logger;
            m_Client = client;
            m_Dialog = dialog;
        }

        public override async Task ExecuteAsync(IEnumerable<ILeaderboardConfig> items,
            CancellationToken cancellationToken = default)
        {
            var itemsList = items.EnumerateOnce();
            if (itemsList.Count > 1)
                return;

            var leaderboard = itemsList.First();

            var dialogResult = m_Dialog.Show(
                "Reset leaderboard",
                $"Are you sure you want to reset the leaderboard '{leaderboard.Name}' (ID: {leaderboard.Id})?",
                "Yes",
                "Cancel");

            if (!dialogResult)
                return;

            try
            {
#if DEPLOYMENT_API_AVAILABLE_V1_1
                var states = leaderboard.States.Where(s => s.Type == ResetState).ToList();
                states.ForEach(leaderboard.States.Remove);
#endif
                await m_Client.Reset(leaderboard);
#if DEPLOYMENT_API_AVAILABLE_V1_1
                var dateTime = DateTime.Now.ToString("yyyy-MM-dd-Thh:mm");
                leaderboard.States.Add(new AssetState($"Reset on {dateTime}", string.Empty, SeverityLevel.Info, ResetState));
#endif
            }
            catch (Exception e)
            {
                m_Logger.LogError($"There was an error resting the leaderboard '{leaderboard.Id}'. {e.Message}");
            }
        }

        public override bool IsVisible(IEnumerable<ILeaderboardConfig> items)
        {
            return items.Count() == 1;
        }
    }
}
