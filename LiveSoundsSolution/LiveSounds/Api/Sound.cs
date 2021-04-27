using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LiveSounds.Api
{

    /// <summary>
    /// Sound.
    /// </summary>
    internal class Sound
    {
        /// <summary>
        /// Url.
        /// </summary>
        [JsonInclude]
        public string Url;

        /// <summary>
        /// Audio items.
        /// </summary>
        [JsonInclude]
        public AudioItem[] Items;

        /// <summary>
        /// Secret.
        /// </summary>
        [JsonInclude]
        public string Secret;
    }
}
