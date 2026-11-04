using System;
using UnityEngine;

namespace Unity.Services.Leaderboards.Editor.Authoring.UI.Inspector
{
    [Serializable]
    class SerializableDateTime
    {
        public long Ticks;

        DateTime m_DateTime;
        bool m_Initialized;

        public DateTime DateTime
        {
            get
            {
                if (!m_Initialized)
                {
                    m_DateTime = new DateTime(Ticks);
                    m_Initialized = true;
                }

                return m_DateTime;
            }
        }

        public SerializableDateTime(DateTime dateTime)
        {
            Ticks = dateTime.Ticks;
            m_DateTime = dateTime;
            m_Initialized = true;
        }
    }
}
