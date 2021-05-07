using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zokma.Libs.Audio
{
    /// <summary>
    /// Audio playback status.
    /// </summary>
    public enum PlaybackState
    {
        /// <summary>
        /// Audio playing.
        /// </summary>
        Playing,

        /// <summary>
        /// Audio playing in a loop.
        /// </summary>
        PlayingInLoop,

        /// <summary>
        /// Audio stop requested.
        /// </summary>
        StopRequested,

        /// <summary>
        /// Audio stopped.
        /// </summary>
        Stopped,
    }
}
