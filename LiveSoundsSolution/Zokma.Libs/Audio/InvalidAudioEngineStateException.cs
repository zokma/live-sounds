using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zokma.Libs.Audio
{
    /// <summary>
    /// Exception for invalid state;
    /// </summary>
    public class InvalidAudioEngineStateException : Exception
    {
        /// <summary>
        /// Creates Exception
        /// </summary>
        /// <param name="message">Error message.</param>
        public InvalidAudioEngineStateException(string message)
            : base(message)
        {
        }
    }
}
