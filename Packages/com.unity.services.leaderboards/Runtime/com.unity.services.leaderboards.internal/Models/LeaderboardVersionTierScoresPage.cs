using System.Collections.Generic;
using UnityEngine.Scripting;
using System.Runtime.Serialization;

namespace Unity.Services.Leaderboards.Internal.Models
{
    /// <summary>
    /// LeaderboardVersionTierScoresPage model
    /// </summary>
    [Preserve]
    [DataContract(Name = "LeaderboardVersionTierScoresPage")]
    internal class LeaderboardVersionTierScoresPage
    {
        /// <summary>
        /// Creates an instance of LeaderboardVersionTierScoresPage.
        /// </summary>
        /// <param name="tier">tier param</param>
        /// <param name="version">version param</param>
        /// <param name="offset">offset param</param>
        /// <param name="limit">limit param</param>
        /// <param name="total">total param</param>
        /// <param name="results">results param</param>
        [Preserve]
        public LeaderboardVersionTierScoresPage(string tier = default, LeaderboardVersion version = default, int offset = default, int limit = default, int total = default, List<LeaderboardEntry> results = default)
        {
            Tier = tier;
            Version = version;
            Offset = offset;
            Limit = limit;
            Total = total;
            Results = results;
        }

        /// <summary>
        /// Parameter tier of LeaderboardVersionTierScoresPage
        /// </summary>
        [Preserve]
        [DataMember(Name = "tier", EmitDefaultValue = false)]
        public string Tier{ get; }

        /// <summary>
        /// Parameter version of LeaderboardVersionTierScoresPage
        /// </summary>
        [Preserve]
        [DataMember(Name = "version", EmitDefaultValue = false)]
        public LeaderboardVersion Version{ get; }

        /// <summary>
        /// Parameter offset of LeaderboardVersionTierScoresPage
        /// </summary>
        [Preserve]
        [DataMember(Name = "offset", EmitDefaultValue = false)]
        public int Offset{ get; }

        /// <summary>
        /// Parameter limit of LeaderboardVersionTierScoresPage
        /// </summary>
        [Preserve]
        [DataMember(Name = "limit", EmitDefaultValue = false)]
        public int Limit{ get; }

        /// <summary>
        /// Parameter total of LeaderboardVersionTierScoresPage
        /// </summary>
        [Preserve]
        [DataMember(Name = "total", EmitDefaultValue = false)]
        public int Total{ get; }

        /// <summary>
        /// Parameter results of LeaderboardVersionTierScoresPage
        /// </summary>
        [Preserve]
        [DataMember(Name = "results", EmitDefaultValue = false)]
        public List<LeaderboardEntry> Results{ get; }
    }
}
