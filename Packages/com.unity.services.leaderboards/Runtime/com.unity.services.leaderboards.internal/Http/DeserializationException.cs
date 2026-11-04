using System;

namespace Unity.Services.Leaderboards.Internal.Http
{
    /// <summary>
    /// DeserializationException class.
    /// </summary>
    [Serializable]
    internal class DeserializationException : Exception
    {
        /// <summary>Default Constructor.</summary>
        public DeserializationException() : base()
        {
        }

        /// <summary>Constructor.</summary>
        /// <param name="message">Custom error message</param>
        public DeserializationException(string message) : base(message)
        {
        }

        /// <summary>DeserializationException with message and inner exception.</summary>
        /// <param name="message">Custom error message.</param>
        /// <param name="inner">Inner exception.</param>
        DeserializationException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
