using Unity.Services.Core.Editor;
using Unity.Services.Core.Editor.OrganizationHandler;
using UnityEditor;

namespace Unity.Services.Leaderboards.Settings
{
    struct LeaderboardsIdentifier : IEditorGameServiceIdentifier
    {
        public string GetKey() => "Leaderboards";
    }
    /// <summary>Internal class</summary>
    public class LeaderboardsEditorGameService : IEditorGameService
    {
        /// <summary>Internal class</summary>
        public string Name => "Leaderboards";
        /// <summary>Internal class</summary>
        public IEditorGameServiceIdentifier Identifier => k_Identifier;
        /// <summary>Internal class</summary>
        public bool RequiresCoppaCompliance => false;
        /// <summary>Internal class</summary>
        public bool HasDashboard => true;
        /// <summary>Internal class</summary>
        public IEditorGameServiceEnabler Enabler => null;

        static readonly LeaderboardsIdentifier k_Identifier = new LeaderboardsIdentifier();

        /// <summary>Internal implementation</summary>
        /// <returns>Internal implementation</returns>
        public string GetFormattedDashboardUrl()
        {
            return
                $"https://cloud.unity3d.com/organizations/{OrganizationProvider.Organization.Key}/projects/{CloudProjectSettings.projectId}/leaderboards/about";
        }
    }
}
