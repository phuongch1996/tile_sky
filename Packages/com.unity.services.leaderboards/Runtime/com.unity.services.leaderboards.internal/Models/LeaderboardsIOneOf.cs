using System;

namespace Unity.Services.Leaderboards.Internal.Models
{
    /// <summary>
    /// Interface to a result that could be one of many possible types.
    /// </summary>
    internal interface IOneOf
    {
        /// <summary>The type of the value.</summary>
        Type Type { get; }
        /// <summary>The value.</summary>
        object Value { get; }
    }
}
