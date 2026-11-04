using System.Collections.Generic;
using Unity.Services.Leaderboards.Authoring.Client.Models;
using Unity.Services.Leaderboards.Authoring.Core.Model;
using UnityEngine;
using LeaderboardConfig = Unity.Services.Leaderboards.Authoring.Core.Model.LeaderboardConfig;
using ResetConfig = Unity.Services.Leaderboards.Authoring.Core.Model.ResetConfig;
using TieringConfig = Unity.Services.Leaderboards.Authoring.Core.Model.TieringConfig;

namespace Unity.Services.Leaderboards.Editor.Authoring.AdminApi
{
    static class LeaderboardsConfigUtil
    {
        static List<LeaderboardConfig1TieringConfigTiersInner> GetTiers(ILeaderboardConfig config)
        {
            var tiers = new List<LeaderboardConfig1TieringConfigTiersInner>();
            foreach (var tier in config.TieringConfig?.Tiers ?? new List<Tier>())
            {
                tiers.Add(new LeaderboardConfig1TieringConfigTiersInner(
                    tier.Id,
                    tier.Cutoff
                ));
            }
            return tiers;
        }

        static List<Tier> GetTiers(UpdatedLeaderboardConfig1 config)
        {
            var tiers = new List<Tier>();
            foreach (var tier in config.TieringConfig?.Tiers ?? new List<LeaderboardConfig1TieringConfigTiersInner>())
            {
                tiers.Add(new Tier()
                {
                    Id = tier.Id,
                    Cutoff = tier.Cutoff
                });
            }

            return tiers;
        }

        public static LeaderboardIdConfig1 ToLeaderboardIdConfig1(ILeaderboardConfig config)
        {
            return new LeaderboardIdConfig1(
                config.Id,
                config.Name,
                (LeaderboardIdConfig1.SortOrderOptions)config.SortOrder,
                (LeaderboardIdConfig1.UpdateTypeOptions)config.UpdateType,
                (int)config.BucketSize,
                config.ResetConfig != null ? new LeaderboardConfig1ResetConfig(
                    config.ResetConfig.Start,
                    config.ResetConfig.Schedule,
                    config.ResetConfig.Archive
                ) : null,
                config.TieringConfig != null ? new LeaderboardConfig1TieringConfig(
                    (LeaderboardConfig1TieringConfig.StrategyOptions)config.TieringConfig.Strategy,
                    config.TieringConfig.Tiers.Count != 0 ? GetTiers(config) : null
                ) : null
            );
        }

        public static LeaderboardConfig1 ToLeaderboardConfig1(ILeaderboardConfig config)
        {
            return new LeaderboardConfig1Patch(
                config.Name,
                (LeaderboardConfig1.SortOrderOptions)config.SortOrder,
                (LeaderboardConfig1.UpdateTypeOptions)config.UpdateType,
                config.ResetConfig != null ? new LeaderboardConfig1ResetConfig(
                    config.ResetConfig.Start,
                    config.ResetConfig.Schedule,
                    config.ResetConfig.Archive
                ) : null,
                config.TieringConfig != null ? new LeaderboardConfig1TieringConfig(
                    (LeaderboardConfig1TieringConfig.StrategyOptions)config.TieringConfig.Strategy,
                    config.TieringConfig.Tiers.Count != 0 ? GetTiers(config) : null
                ) : null
            );
        }

        public static LeaderboardConfig UpdatedLeaderboardConfig1ToLeaderboardConfig(UpdatedLeaderboardConfig1 config)
        {
            return new LeaderboardConfig(
                config.Id,
                config.Name,
                (Unity.Services.Leaderboards.Authoring.Core.Model.SortOrder)config.SortOrder,
                (Unity.Services.Leaderboards.Authoring.Core.Model.UpdateType)config.UpdateType) {
                TieringConfig = config.TieringConfig != null ? new TieringConfig() {
                    Strategy = (Strategy)config.TieringConfig.Strategy,
                    Tiers = GetTiers(config)
                } : null,
                ResetConfig = config.ResetConfig != null ? new ResetConfig()
                {
                    Start = config.ResetConfig.Start,
                    Schedule = config.ResetConfig.Schedule,
                    Archive = config.ResetConfig.Archive
                } : null
            };
        }
    }
}
