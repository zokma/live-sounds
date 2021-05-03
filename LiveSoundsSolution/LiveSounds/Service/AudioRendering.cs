using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LiveSounds.Service
{

    /// <summary>
    /// Audio Rendering.
    /// </summary>
    internal class AudioRendering
    {
        /// <summary>
        /// Volume int min.
        /// </summary>
        private const int VOLUME_INT_MIN = 0;

        /// <summary>
        /// Volume int max.
        /// </summary>
        private const int VOLUME_INT_MAX = 100;

        /// <summary>
        /// Audio Id.
        /// </summary>
        [JsonInclude]
        public string Id { get; private set; }

        /// <summary>
        /// Sound Id.
        /// </summary>
        [JsonInclude]
        public string SoundId { get; private set; }

        /// <summary>
        /// Volume.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("volume")]
        public int VolumeNumber { get; private set; }

        /// <summary>
        /// Volume.
        /// </summary>
        [JsonIgnore]
        public float Volume 
        {
            get
            {
                return ((Math.Max(Math.Min(this.VolumeNumber, VOLUME_INT_MAX), VOLUME_INT_MIN)) / 100);
            }
        }

        /// <summary>
        /// Secret.
        /// </summary>
        [JsonInclude]
        public string Secret { get; private set; }

        /// <summary>
        /// User Hash.
        /// </summary>
        [JsonInclude]
        public string UserHash { get; private set; }
    }
}
