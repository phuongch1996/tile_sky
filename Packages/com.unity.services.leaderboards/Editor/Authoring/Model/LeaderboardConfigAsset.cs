using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Unity.Services.DeploymentApi.Editor;
using Unity.Services.Leaderboards.Editor.Shared.Assets;
using Unity.Services.Leaderboards.Authoring.Core.Model;
using Unity.Services.Leaderboards.Editor.Authoring.Analytics;
using UnityEditor;
using UnityEngine;
using JsonConvert = Newtonsoft.Json.JsonConvert;

namespace Unity.Services.Leaderboards.Editor.Authoring.Model
{
    [HelpURL("https://docs.unity3d.com/Packages/com.unity.services.leaderboards@2.1/manual/Authoring/leaderboard_configuration.html")]
    class LeaderboardConfigAsset : ScriptableObject, IPath
    {
        const string k_DefaultFileName = "leaderboard_config";
        const string k_AnalyticsEventCreateFile = "leaderboards_file_created";

        internal EditorLeaderboardConfig m_LeaderboardConfig;
        public EditorLeaderboardConfig Model => m_LeaderboardConfig;

        public string Path { get => Model?.Path; set => SetPath(value); }
        public ObservableCollection<AssetState> States { get; } = new ObservableCollection<AssetState>();

        void SetPath(string path)
        {
            if (Model is null && path is not null)
            {
                var id = System.IO.Path.GetFileNameWithoutExtension(path);
                m_LeaderboardConfig = new EditorLeaderboardConfig(id);
            }

            if (Model != null)
            {
                Model.Path = path;
            }
        }

        [MenuItem("Assets/Create/Services/Leaderboard Configuration", false, 81)]
        public static void CreateConfig()
        {
            // for analytics
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            Exception exception = null;
            var fileName = k_DefaultFileName + LeaderboardAssetsExtensions.configExtension;
            var defaultConfig = new EditorLeaderboardConfig(k_DefaultFileName)
            {
                Path = null,
                SortOrder = SortOrder.Asc,
                UpdateType = UpdateType.KeepBest,
                TieringConfig = new TieringConfig()
                {
                    Strategy = Strategy.Score,
                    Tiers = new List<Tier>()
                    {
                        new() { Id = "Gold", Cutoff = 200.0 },
                        new() { Id = "Silver", Cutoff = 100.0 },
                        new() { Id = "Bronze" },
                    }
                },
                ResetConfig = new ResetConfig()
                {
                    Schedule = "0 12 1 * *",
                    Start = DateTime.Today.AddDays(10).Date,
                }
            };
            var settings = new JsonSerializerSettings()
            {
                Converters = { new StringEnumConverter() },
                Formatting = Formatting.Indented,
                DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate
            };
            try
            {
                ProjectWindowUtil.CreateAssetWithTextContent(fileName, JsonConvert.SerializeObject(defaultConfig, settings));
            }
            catch (Exception e)
            {
                exception = e;
                throw;
            }
            finally
            {
                stopWatch.Stop();

                LeaderboardAuthoringServices.Instance.GetService<ILeaderboardsEditorAnalytics>()
                    .SendEvent(
                        k_AnalyticsEventCreateFile,
                        defaultConfig.Type,
                        stopWatch.ElapsedMilliseconds,
                        string.IsNullOrEmpty(exception?.InnerException?.Message)
                        ? exception?.InnerException?.Message
                        : exception?.Message
                    );
            }
        }
    }
}
