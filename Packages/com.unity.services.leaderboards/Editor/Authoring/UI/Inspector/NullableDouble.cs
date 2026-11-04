using System;
using UnityEngine;

namespace Unity.Services.Leaderboards.Editor.Authoring.UI.Inspector
{
    [Serializable]
    class NullableDouble
    {
        [Tooltip("Should this tier have a cutoff? Exactly one tier must omit the Cutoff value.")]
        public bool HasValue;
        [Tooltip("The cutoff value for this tier.")]
        public double Value;

        public NullableDouble() { }

        public NullableDouble(double? value)
        {
            HasValue = value.HasValue;
            Value = HasValue ? value.Value : 0;
        }
    }
}
