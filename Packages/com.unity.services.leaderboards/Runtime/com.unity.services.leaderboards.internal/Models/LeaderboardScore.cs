using UnityEngine.Scripting;
using System.Runtime.Serialization;

namespace Unity.Services.Leaderboards.Internal.Models
{
    /// <summary>
    /// LeaderboardScore model
    /// </summary>
    [Preserve]
    [DataContract(Name = "LeaderboardScore")]
    internal class LeaderboardScore
    {
        /// <summary>
        /// Creates an instance of LeaderboardScore.
        /// </summary>
        /// <param name="score">score param</param>
        /// <param name="metadata">metadata param</param>
        [Preserve]
        public LeaderboardScore(double score, object metadata = null)
        {
            Score = score;
            Metadata = metadata;
        }

        /// <summary>
        /// Parameter score of LeaderboardScore
        /// </summary>
        [Preserve]
        [DataMember(Name = "score", IsRequired = true, EmitDefaultValue = true)]
        public double Score { get; }

        /// <summary>
        /// Parameter metadata of LeaderboardScore
        /// </summary>
        [Preserve]
        [DataMember(Name = "metadata", IsRequired = true, EmitDefaultValue = false)]
        public object Metadata { get; }
    }
}
