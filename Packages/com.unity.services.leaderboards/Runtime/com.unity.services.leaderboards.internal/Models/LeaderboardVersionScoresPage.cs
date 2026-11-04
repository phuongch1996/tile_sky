using System.Collections.Generic;
using UnityEngine.Scripting;
using System.Runtime.Serialization;

namespace Unity.Services.Leaderboards.Internal.Models
{
    /// <summary>
    /// LeaderboardVersionScoresPage model
    /// </summary>
    [Preserve]
    [DataContract(Name = "LeaderboardVersionScoresPage")]
    internal class LeaderboardVersionScoresPage
    {
        /// <summary>
        /// Creates an instance of LeaderboardVersionScoresPage.
        /// </summary>
        /// <param name="version">version param</param>
        /// <param name="offset">offset param</param>
        /// <param name="limit">limit param</param>
        /// <param name="total">total param</param>
        /// <param name="results">results param</param>
        [Preserve]
        public LeaderboardVersionScoresPage(LeaderboardVersion version = default, int offset = default, int limit = default, int total = default, List<LeaderboardEntry> results = default)
        {
            Version = version;
            Offset = offset;
            Limit = limit;
            Total = total;
            Results = results;
        }

        /// <summary>
        /// Parameter version of LeaderboardVersionScoresPage
        /// </summary>
        [Preserve]
        [DataMember(Name = "version", EmitDefaultValue = false)]
        public LeaderboardVersion Version{ get; }

        /// <summary>
        /// Parameter offset of LeaderboardVersionScoresPage
        /// </summary>
        [Preserve]
        [DataMember(Name = "offset", EmitDefaultValue = false)]
        public int Offset{ get; }

        /// <summary>
        /// Parameter limit of LeaderboardVersionScoresPage
        /// </summary>
        [Preserve]
        [DataMember(Name = "limit", EmitDefaultValue = false)]
        public int Limit{ get; }

        /// <summary>
        /// Parameter total of LeaderboardVersionScoresPage
        /// </summary>
        [Preserve]
        [DataMember(Name = "total", EmitDefaultValue = false)]
        public int Total{ get; }

        /// <summary>
        /// Parameter results of LeaderboardVersionScoresPage
        /// </summary>
        [Preserve]
        [DataMember(Name = "results", EmitDefaultValue = false)]
        public List<LeaderboardEntry> Results{ get; }
    }
}
