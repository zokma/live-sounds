using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zokma.Libs.Audio
{
    /// <summary>
    /// Audio Data flow.
    /// </summary>
    [Flags]
    public enum AudioDataFlow
    {
        /// <summary>
        /// No data flow.
        /// </summary>
        None    = 0x0000,

        /// <summary>
        /// Audio playing.
        /// </summary>
        Render  = 0x0001,

        /// <summary>
        /// Audio recording.
        /// </summary>
        Capture = 0x0002,
    }
}
