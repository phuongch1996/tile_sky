using Unity.Services.Core.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Services.Leaderboards.Settings
{
    /// <summary>Class to support internal functionality</summary>
    public class LeaderboardsSettingsProvider : EditorGameServiceSettingsProvider
    {
        const string k_Title = "Leaderboards";
        const string k_GoToDashboardContainer = "dashboard-button-container";
        const string k_GoToDashboardBtn = "dashboard-link-button";

        static readonly LeaderboardsEditorGameService k_GameService = new LeaderboardsEditorGameService();

        /// <summary>Internal</summary>
        /// <returns>Internal</returns>
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            return new LeaderboardsSettingsProvider(SettingsScope.Project);
        }

        /// <summary>Class to support internal functionality</summary>
        protected override IEditorGameService EditorGameService => k_GameService;
        /// <summary>Class to support internal functionality</summary>
        protected override string Title => k_Title;
        /// <summary>Class to support internal functionality</summary>
        protected override string Description => "Leaderboards allows you to store, sort, rank, and retrieve player scores quickly and easily within your game.";

        /// <summary>Class to support internal functionality</summary>
        /// <param name="scopes">Internal</param>
        public LeaderboardsSettingsProvider(SettingsScope scopes)
            : base(GenerateProjectSettingsPath(k_Title), scopes) {}

        /// <summary>Internal</summary>
        /// <returns>Internal</returns>
        protected override VisualElement GenerateServiceDetailUI()
        {
            var containerVisualElement = new VisualElement();

            // No settings for Leaderboards at the moment

            return containerVisualElement;
        }

        /// <summary>Internal</summary>
        /// <param name="searchContext">Internal</param>
        /// <param name="rootElement">Internal</param>
        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            base.OnActivate(searchContext, rootElement);
            SetDashboardButton(rootElement);
        }

        static void SetDashboardButton(VisualElement rootElement)
        {
            rootElement.Q(k_GoToDashboardContainer).style.display = DisplayStyle.Flex;
            var goToDashboard = rootElement.Q(k_GoToDashboardBtn);

            if (goToDashboard != null)
            {
                var clickable = new Clickable(() =>
                {
                    Application.OpenURL(k_GameService.GetFormattedDashboardUrl());
                });
                goToDashboard.AddManipulator(clickable);
            }
        }
    }
}
