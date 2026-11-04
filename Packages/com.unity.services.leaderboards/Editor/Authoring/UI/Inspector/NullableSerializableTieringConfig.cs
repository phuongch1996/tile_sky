#nullable enable
using System;
using Unity.Services.Leaderboards.Authoring.Core.Model;
using UnityEngine;

namespace Unity.Services.Leaderboards.Editor.Authoring.UI.Inspector
{
    [Serializable]
    class NullableSerializableTieringConfig
    {
        [Tooltip("Should the leaderboard be split into tiers?")]
        public bool HasValue = false;
        public SerializableTieringConfig Value = new SerializableTieringConfig();

        public void CopyFrom(TieringConfig? tieringConfig)
        {
            if (tieringConfig?.Tiers == null
                || tieringConfig.Tiers.Count == 0)
            {
                HasValue = false;
                Value = new SerializableTieringConfig();
                return;
            }

            HasValue = true;
            Value.CopyFrom(tieringConfig);
        }
    }
}
