using System.Threading.Tasks;
using Unity.Services.Leaderboards.Authoring.Core.Model;

namespace Unity.Services.Leaderboards.Authoring.Core.Serialization
{
    interface ILeaderboardsSerializer
    {
        string Serialize(ILeaderboardConfig config);
        LeaderboardConfig Deserialize(string path);
        void DeserializeAndPopulate(LeaderboardConfig config);
    }
}
