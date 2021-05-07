using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Zokma.Libs.Audio;

namespace LiveSounds.Audio
{

    /// <summary>
    /// Audio Item.
    /// </summary>
    internal class AudioItem
    {
        /// <summary>
        /// Audio item id.
        /// </summary>
        [JsonInclude]
        public string Id;

        /// <summary>
        /// Audio item name.
        /// </summary>
        [JsonInclude]
        public string Name;

        /// <summary>
        /// Audio Data.
        /// </summary>
        [JsonIgnore]
        public AudioData Data;
    }
}
