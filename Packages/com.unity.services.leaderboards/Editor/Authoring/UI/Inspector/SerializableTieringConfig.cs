using System;
using System.Collections.Generic;
using Unity.Services.Leaderboards.Authoring.Core.Model;
using UnityEngine;

namespace Unity.Services.Leaderboards.Editor.Authoring.UI.Inspector
{
    [Serializable]
    class SerializableTieringConfig
    {
        [Tooltip("The tiering strategy.")]
        public Strategy Strategy = Strategy.Score;
        [Tooltip("The different tiers in the leaderboard. Exactly one value must omit the Cutoff value.")]
        public List<SerializableTier> Tiers = new();

        public void CopyFrom(TieringConfig tieringConfig)
        {
            Strategy = tieringConfig.Strategy;
            Tiers = new List<SerializableTier>();
            foreach (var tier in tieringConfig.Tiers)
            {
                Tiers.Add(new SerializableTier(tier.Id, tier.Cutoff));
            }
        }
    }
}
