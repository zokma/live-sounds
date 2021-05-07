using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LiveSounds.Ngrok
{
    /// <summary>
    /// Ngrok tunnels.
    /// </summary>
    internal class NgrokTunnels
    {
        /// <summary>
        /// Ngrok tunnels.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("tunnels")]
        public TunnelInfo[] Tunnels;
    }
}
