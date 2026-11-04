using System.Runtime.Serialization;

namespace Unity.Services.Leaderboards.Authoring.Core.Model
{
    enum SortOrder
    {
        [EnumMember(Value = "asc")]
        Asc = 1,

        [EnumMember(Value = "desc")]
        Desc = 2,
    }
}
