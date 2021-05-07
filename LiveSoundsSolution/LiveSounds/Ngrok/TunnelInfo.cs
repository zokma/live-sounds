using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LiveSounds.Ngrok
{
    /// <summary>
    /// Tunnel info.
    /// </summary>
    internal class TunnelInfo
    {
        /// <summary>
        /// Public url for tunnel.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("public_url")]
        public string PublicUrl;

        /// <summary>
        /// Protocol for tunnel.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("proto")]
        public string Protocol;

        /// <summary>
        /// Forwarding info.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("config")]
        public ForwardingInfo ForwardingInfo;
    }
}
