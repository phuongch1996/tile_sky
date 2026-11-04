using System;
using UnityEngine.Scripting;
using System.Runtime.Serialization;

namespace Unity.Services.Leaderboards.Internal.Models
{
    /// <summary>
    /// LeaderboardEntry model
    /// </summary>
    [Preserve]
    [DataContract(Name = "LeaderboardEntry")]
    internal class LeaderboardEntry
    {
        /// <summary>
        /// Creates an instance of LeaderboardEntry.
        /// </summary>
        /// <param name="playerId">playerId param</param>
        /// <param name="playerName">playerName param</param>
        /// <param name="rank">rank param</param>
        /// <param name="score">score param</param>
        /// <param name="tier">tier param</param>
        /// <param name="updatedTime">updatedTime param</param>
        /// <param name="metadata">metadata param</param>
        [Preserve]
        public LeaderboardEntry(string playerId, string playerName, int rank, double score, string tier = default,
            DateTime updatedTime = default, object metadata = null)
        {
            PlayerId = playerId;
            PlayerName = playerName;
            Rank = rank;
            Score = score;
            Tier = tier;
            UpdatedTime = updatedTime;
            Metadata = metadata;
        }

        /// <summary>
        /// Parameter playerId of LeaderboardEntry
        /// </summary>
        [Preserve]
        [DataMember(Name = "playerId", IsRequired = true, EmitDefaultValue = true)]
        public string PlayerId { get; }

        /// <summary>
        /// Parameter playerName of LeaderboardEntry
        /// </summary>
        [Preserve]
        [DataMember(Name = "playerName", IsRequired = true, EmitDefaultValue = true)]
        public string PlayerName { get; }

        /// <summary>
        /// Parameter rank of LeaderboardEntry
        /// </summary>
        [Preserve]
        [DataMember(Name = "rank", IsRequired = true, EmitDefaultValue = true)]
        public int Rank { get; }

        /// <summary>
        /// Parameter score of LeaderboardEntry
        /// </summary>
        [Preserve]
        [DataMember(Name = "score", IsRequired = true, EmitDefaultValue = true)]
        public double Score { get; }

        /// <summary>
        /// Parameter tier of LeaderboardEntry
        /// </summary>
        [Preserve]
        [DataMember(Name = "tier", EmitDefaultValue = false)]
        public string Tier { get; }

        /// <summary>
        /// Parameter updatedTime of LeaderboardEntry
        /// </summary>
        [Preserve]
        [DataMember(Name = "updatedTime", EmitDefaultValue = false)]
        public DateTime UpdatedTime { get; }

        /// <summary>
        /// Parameter updatedTime of LeaderboardEntry
        /// </summary>
        [Preserve]
        [DataMember(Name = "metadata", EmitDefaultValue = false)]
        public object Metadata { get; }
    }
}
