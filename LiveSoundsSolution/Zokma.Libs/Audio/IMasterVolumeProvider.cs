using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zokma.Libs.Audio
{

    /// <summary>
    /// Interface to get Master volume.
    /// </summary>
    public interface IMasterVolumeProvider
    {

        /// <summary>
        /// Master volume.
        /// </summary>
        public float MasterVolume { get; }
    }
}
