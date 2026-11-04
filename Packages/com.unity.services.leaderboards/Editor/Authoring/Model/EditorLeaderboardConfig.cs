using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Unity.Services.Leaderboards.Authoring.Core.Model;
using Unity.Services.Leaderboards.Editor.Authoring.UI.Inspector;
using UnityEngine;

namespace Unity.Services.Leaderboards.Editor.Authoring.Model
{
    class EditorLeaderboardConfig : LeaderboardConfig
    {
        [JsonProperty("$schema")]
        public string Value = "https://ugs-config-schemas.unity3d.com/v1/leaderboards.schema.json";

        public EditorLeaderboardConfig(string name) : base(name) {}

        public void CopyFrom(LeaderboardInspectorConfig inspectorConfig)
        {
            Name = inspectorConfig.LeaderboardName;
            Id = inspectorConfig.LeaderboardId;
            SortOrder = inspectorConfig.SortOrder;
            UpdateType = inspectorConfig.UpdateType;
            BucketSize = inspectorConfig.BucketSize.HasValue
                ? inspectorConfig.BucketSize.Value
                : 0;

            if (inspectorConfig.ResetConfig.HasValue)
            {
                ResetConfig = new ResetConfig()
                {
                    Archive = inspectorConfig.ResetConfig.Value.Archive,
                    Start = inspectorConfig.ResetConfig.Value.Start.DateTime,
                    Schedule = inspectorConfig.ResetConfig.Value.Schedule,
                };
            }
            else
            {
                ResetConfig = null;
            }

            if (inspectorConfig.TieringConfig.HasValue)
            {
                var tiers = inspectorConfig.TieringConfig.Value.Tiers
                    .Select(tier => new Tier()
                    {
                        Cutoff = tier.Cutoff.HasValue ? tier.Cutoff.Value : null,
                        Id = tier.Id,
                    }).ToList();
                TieringConfig = new TieringConfig()
                {
                    Strategy = inspectorConfig.TieringConfig.Value.Strategy,
                    Tiers = tiers,
                };
            }
            else
            {
                TieringConfig = null;
            }
        }

        public void SaveToDisk()
        {
            var settings = new JsonSerializerSettings()
            {
                Converters = { new StringEnumConverter() },
                Formatting = Formatting.Indented,
                DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate
            };
            var jsonValue = JsonConvert.SerializeObject(this, settings);
            File.WriteAllText(Path, jsonValue);
        }

        internal void ClearOwnedStates()
        {
            var i = 0;
            while (i < States.Count)
            {
                if (LeaderboardsSerializer.IsDeserializationError(States[i].Description))
                {
                    States.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }
    }
}
