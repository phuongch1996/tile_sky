using System;
using Unity.Services.Leaderboards.Authoring.Core.Model;
using UnityEngine;

namespace Unity.Services.Leaderboards.Editor.Authoring.UI.Inspector
{
    [Serializable]
    class LeaderboardInspectorConfig : ScriptableObject
    {
        [Tooltip("Name of the leaderboard.")]
        public string LeaderboardName;
        [Tooltip("The sorting order for the values of the leaderboard.")]
        public SortOrder SortOrder;
        [Tooltip("This determines how to handle new scores that players submit.")]
        public UpdateType UpdateType;
        [Header("ID")]
        [Tooltip("ID of the leaderboard to use with the API. This ID cannot contains spaces.")]
        public string LeaderboardId;
        [Header("Buckets")]
        public NullableBucketSize BucketSize;
        [Header("Reset")]
        public NullableSerializableResetConfig ResetConfig;
        [Header("Tiers")]
        public NullableSerializableTieringConfig TieringConfig;

        public void Initialize(LeaderboardConfig leaderboardConfig)
        {
            LeaderboardName = leaderboardConfig.Name;
            LeaderboardId = leaderboardConfig.Id;
            SortOrder = leaderboardConfig.SortOrder;
            BucketSize = new NullableBucketSize((int)leaderboardConfig.BucketSize);
            UpdateType = leaderboardConfig.UpdateType;
            ResetConfig = new NullableSerializableResetConfig();
            ResetConfig.CopyFrom(leaderboardConfig.ResetConfig);
            TieringConfig = new NullableSerializableTieringConfig();
            TieringConfig.CopyFrom(leaderboardConfig.TieringConfig);
        }
    }
}
