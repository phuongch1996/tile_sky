using UnityEngine.Scripting;
using System.Runtime.Serialization;

namespace Unity.Services.Leaderboards.Internal.Models
{
    /// <summary>
    /// AddLeaderboardScore model
    /// </summary>
    [Preserve]
    [DataContract(Name = "AddLeaderboardScore")]
    public class AddLeaderboardScore
    {
        /// <summary>
        /// Creates an instance of AddLeaderboardScore.
        /// </summary>
        /// <param name="score">score param</param>
        /// <param name="metadata">metadata param</param>
        /// <param name="versionId">versionId param</param>
        [Preserve]
        public AddLeaderboardScore(double score, object metadata = null, string versionId = default)
        {
            Score = score;
            Metadata = metadata;
            VersionId = versionId;
        }

        /// <summary>
        /// Parameter score of AddLeaderboardScore
        /// </summary>
        [Preserve]
        [DataMember(Name = "score", IsRequired = true, EmitDefaultValue = true)]
        public double Score { get; }

        /// <summary>
        /// Parameter metadata of AddLeaderboardScore
        /// </summary>
        [Preserve]
        [DataMember(Name = "metadata", IsRequired = true, EmitDefaultValue = false)]
        public object Metadata { get; }
        
        /// <summary>
        /// Parameter versionId of AddLeaderboardScore
        /// </summary>
        [Preserve]
        [DataMember(Name = "versionId", EmitDefaultValue = false)]
        public string VersionId{ get; }
    }
}
