using System;
using UnityEngine;

namespace Unity.Services.Leaderboards.Editor.Authoring.UI.Inspector
{
    [Serializable]
    class NullableBucketSize
    {
        [Tooltip("Should your leaderboard segment players into buckets? NOTE: This value cannot be updated once the leaderboard has been created.")]
        public bool HasValue;
        [Tooltip("The maximum number of players assigned to a bucket.")]
        public int Value;

        public NullableBucketSize() { }

        public NullableBucketSize(int? value)
        {
            HasValue = value.HasValue && value.Value > 0;
            Value = HasValue ? value.Value : 0;
        }
    }
}
