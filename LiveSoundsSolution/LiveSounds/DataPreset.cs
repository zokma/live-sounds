using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LiveSounds
{
    /// <summary>
    /// Data preset class.
    /// </summary>
    internal class DataPreset
    {
        /// <summary>
        /// Data preset name.
        /// </summary>
        [JsonInclude]
        public string Name;

        /// <summary>
        /// Audio items.
        /// </summary>
        [JsonInclude]
        public AudioInfo[] AudioItems;
    }
}
