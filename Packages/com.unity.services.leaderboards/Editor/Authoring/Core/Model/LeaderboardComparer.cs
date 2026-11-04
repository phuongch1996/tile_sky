using System.Collections.Generic;

namespace Unity.Services.Leaderboards.Authoring.Core.Model
{
    class LeaderboardComparer : IEqualityComparer<ILeaderboardConfig>
    {
        public bool Equals(ILeaderboardConfig x, ILeaderboardConfig y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            return x.Id == y.Id;
        }

        public int GetHashCode(ILeaderboardConfig obj)
        {
            return (obj.Id != null ? obj.Id.GetHashCode() : 0);
        }
    }
}
