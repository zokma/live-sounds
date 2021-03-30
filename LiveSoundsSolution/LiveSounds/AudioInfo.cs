using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LiveSounds
{
    /// <summary>
    /// Audio info class.
    /// </summary>
    internal class AudioInfo
    {
        /// <summary>
        /// Volume min.
        /// </summary>
        private const float VOLUME_MIN = 0.0f;

        /// <summary>
        /// Volume max.
        /// </summary>
        private const float VOLUME_MAX = 4.0f;

        /// <summary>
        /// Id.
        /// </summary>
        [JsonInclude]
        public string Id;

        /// <summary>
        /// Name of the Audio Info.
        /// </summary>
        [JsonInclude]
        public string Name;

        /// <summary>
        /// File name.
        /// </summary>
        [JsonInclude]
        public string File;

        /// <summary>
        /// Audio volume value.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("Volume")]
        public float? VolumeValue { get; private set; } = null;

        /// <summary>
        /// Audio volume.
        /// </summary>
        [JsonIgnore]
        public float Volume
        {
            get
            {
                if(!this.VolumeValue.HasValue)
                {
                    return 1.0f;
                }

                return Math.Max(VOLUME_MIN, Math.Min(VOLUME_MAX, this.VolumeValue.Value));
            }
            set
            {
                this.VolumeValue = Math.Max(VOLUME_MIN, Math.Min(VOLUME_MAX, value));
            }
        }
    }
}
