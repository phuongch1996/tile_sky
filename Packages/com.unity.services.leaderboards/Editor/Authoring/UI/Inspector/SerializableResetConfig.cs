using System;
using Unity.Services.Leaderboards.Authoring.Core.Model;
using UnityEngine;

namespace Unity.Services.Leaderboards.Editor.Authoring.UI.Inspector
{
    [Serializable]
    class SerializableResetConfig
    {
        [Tooltip("The date and time of the first reset.")]
        public SerializableDateTime Start = new SerializableDateTime(DateTime.Now);
        [Tooltip("The schedule on which to reset the leaderboard. Can be either a valid five-element cron tab or a cron string using @every shorthand ")]
        public string Schedule;
        [Tooltip("Should the leaderboard be archived upon a reset?")]
        public bool Archive;

        public void CopyFrom(ResetConfig resetConfig)
        {
            Start = new SerializableDateTime(resetConfig.Start);
            Schedule = resetConfig.Schedule;
            Archive = resetConfig.Archive;
        }
    }
}
