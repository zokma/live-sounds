using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LiveSounds.Api
{
    /// <summary>
    /// Audio Item.
    /// </summary>
    internal class AudioItem
    {
        /// <summary>
        /// Id.
        /// </summary>
        [JsonInclude]
        public string Id;

        /// <summary>
        /// Name.
        /// </summary>
        [JsonInclude]
        public string Name;
    }
}
