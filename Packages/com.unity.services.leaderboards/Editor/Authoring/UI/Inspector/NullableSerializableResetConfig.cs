#nullable enable
using System;
using Unity.Services.Leaderboards.Authoring.Core.Model;
using UnityEngine;

namespace Unity.Services.Leaderboards.Editor.Authoring.UI.Inspector
{
    [Serializable]
    class NullableSerializableResetConfig
    {
        [Tooltip("Should your leaderboard reset?")]
        public bool HasValue = false;
        public SerializableResetConfig Value = new SerializableResetConfig();

        public void CopyFrom(ResetConfig? resetConfig)
        {
            if (resetConfig == null)
            {
                HasValue = false;
                Value = new SerializableResetConfig();
                return;
            }

            HasValue = true;
            Value.CopyFrom(resetConfig);
        }
    }
}
