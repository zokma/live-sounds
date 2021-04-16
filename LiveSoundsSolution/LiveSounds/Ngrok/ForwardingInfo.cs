using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LiveSounds.Ngrok
{
    /// <summary>
    /// Forwarding info.
    /// </summary>
    internal class ForwardingInfo
    {
        /// <summary>
        /// Forwarding address.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("addr")]
        public string Address;

        /// <summary>
        /// Forwarding uri.
        /// </summary>
        [JsonIgnore]
        public Uri Uri 
        {
            get
            {
                return new Uri(this.Address);
            }
        }

        /// <summary>
        /// Forwarding port.
        /// </summary>
        [JsonIgnore]
        public int Port
        {
            get
            {
                return this.Uri.Port;
            }
        }
    }
}
