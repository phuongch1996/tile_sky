using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Scripting;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Unity.Services.Leaderboards.Internal.Http;

namespace Unity.Services.Leaderboards.Internal.Models
{
    /// <summary>
    /// LeaderboardVersions model
    /// </summary>
    [Preserve]
    [DataContract(Name = "LeaderboardVersions")]
    internal class LeaderboardVersions
    {
        /// <summary>
        /// Creates an instance of LeaderboardVersions.
        /// </summary>
        /// <param name="leaderboardId">leaderboardId param</param>
        /// <param name="results">results param</param>
        /// <param name="nextReset">nextReset param</param>
        /// <param name="versionId">nextReset param</param>
        /// <param name="totalArchivedVersions">totalArchivedVersions param</param>
        [Preserve]
        public LeaderboardVersions(string leaderboardId = default, List<LeaderboardVersion> results = default, DateTime nextReset = default, string versionId = default, int totalArchivedVersions = default)
        {
            LeaderboardId = leaderboardId;
            Results = results;
            NextReset = nextReset;
            VersionId = versionId;
            TotalArchivedVersions = totalArchivedVersions;
        }

        /// <summary>
        /// Parameter leaderboardId of LeaderboardVersions
        /// </summary>
        [Preserve]
        [DataMember(Name = "leaderboardId", EmitDefaultValue = false)]
        public string LeaderboardId{ get; }

        /// <summary>
        /// Parameter results of LeaderboardVersions
        /// </summary>
        [Preserve]
        [DataMember(Name = "results", EmitDefaultValue = false)]
        public List<LeaderboardVersion> Results{ get; }

        /// <summary>
        /// Parameter nextReset of LeaderboardVersions
        /// </summary>
        [Preserve]
        [DataMember(Name = "nextReset", EmitDefaultValue = false)]
        public DateTime NextReset{ get; }

        /// <summary>
        /// Parameter versionId of LeaderboardVersions
        /// </summary>
        [Preserve]
        [DataMember(Name = "versionId", EmitDefaultValue = false)]
        public string VersionId{ get; }

        /// <summary>
        /// Parameter totalArchivedVersions of LeaderboardVersions
        /// </summary>
        [Preserve]
        [DataMember(Name = "totalArchivedVersions", EmitDefaultValue = false)]
        public int TotalArchivedVersions{ get; }
    }
}
