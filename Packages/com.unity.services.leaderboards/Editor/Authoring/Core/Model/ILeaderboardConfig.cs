using System;
using Unity.Services.DeploymentApi.Editor;

namespace Unity.Services.Leaderboards.Authoring.Core.Model
{
    interface ILeaderboardConfig : IDeploymentItem, ITypedItem
    {
        string Id { get; set; }
        new float Progress { get; set; }

        new string Name { get; set; }

        SortOrder SortOrder { get; set; }
        UpdateType UpdateType { get; set; }
        int BucketSize { get; set; }
        ResetConfig ResetConfig { get; set; }
        TieringConfig TieringConfig { get; set; }
    }
}
