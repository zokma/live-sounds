using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Zokma.Libs.Audio
{
    /// <summary>
    /// To get the playing state or control the playing sounds.
    /// Currently, getting state and stopping sound is implmented.
    /// To be implemtend Fadein, Fadeout and so on.
    /// </summary>
    public class PlaybackToken
    {
        /// <summary>
        /// Playback state.
        /// </summary>
        public PlaybackState State { get; internal set; }

        /// <summary>
        /// Loop count.
        /// </summary>
        public uint LoopCount => this.loopCount;

        /// <summary>
        /// Loop count.
        /// </summary>
        private uint loopCount = 1;

        /// <summary>
        /// Creates playback token.
        /// </summary>
        /// <param name="isLoop">true if the sound is loop.</param>
        internal PlaybackToken(bool isLoop)
        {
            this.State = (isLoop ? PlaybackState.PlayingInLoop : PlaybackState.Playing);
        }

        /// <summary>
        /// Increments loop count.
        /// </summary>
        internal void IncrementLoopCount()
        {
            Interlocked.Increment(ref this.loopCount);
        }

        /// <summary>
        /// Requests stop;
        /// </summary>
        public void Stop()
        {
            this.State = PlaybackState.StopRequested;
        }

    }
}
