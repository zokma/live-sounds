using LiveSounds.Audio;
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
        /// Id.
        /// </summary>
        [JsonInclude]
        public string Id;

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
        /// Streaming title;
        /// </summary>
        [JsonInclude]
        public string StreamingTitle;

        /// <summary>
        /// Streaming Id;
        /// </summary>
        [JsonInclude]
        public string StreamingId;

        /// <summary>
        /// Secret.
        /// </summary>
        [JsonInclude]
        public string Secret;

        /// <summary>
        /// Validity seconds.
        /// </summary>
        [JsonInclude]
        public int ValiditySeconds;

    }
}
