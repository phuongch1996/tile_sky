using System;
using UnityEngine;

namespace Unity.Services.Leaderboards.Editor.Authoring.UI.Inspector
{
    [Serializable]
    class SerializableTier
    {
        [Tooltip("The ID of the tier.")]
        public string Id;
        public NullableDouble Cutoff;

        public SerializableTier() { }

        public SerializableTier(string id, double? cutoff)
        {
            Id = id;
            Cutoff = new NullableDouble(cutoff);
        }
    }
}
