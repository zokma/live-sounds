using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zokma.Libs.Audio
{
    /// <summary>
    /// Status of AudioEngin(Player or Recorder).
    /// </summary>
    public enum AudioEngineState
    {
        /// <summary>
        /// Engine created.
        /// </summary>
        Created,

        /// <summary>
        /// Engine initialized.
        /// </summary>
        Initialized,

        /// <summary>
        /// Engine started.
        /// </summary>
        Started,

        /// <summary>
        /// Engine stopped.
        /// </summary>
        Stopped,

        /// <summary>
        /// Engine paused.
        /// </summary>
        Paused,

        /// <summary>
        /// Engine disposed.
        /// </summary>
        Disposed,
    }
}
