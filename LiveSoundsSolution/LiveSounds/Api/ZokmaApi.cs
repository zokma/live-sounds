using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LiveSounds.Api
{
    /// <summary>
    /// Zokma api.
    /// </summary>
    internal class ZokmaApi
    {
        /// <summary>
        /// Sounds api.
        /// </summary>
        private static readonly string SOUNDS_API = $"{AppSettings.ZOKMA_API_URI}/live/v1/sounds";

        /// <summary>
        /// Http client.
        /// </summary>
        private static HttpClient httpClient;

        static ZokmaApi()
        {
            httpClient = new HttpClient(new SocketsHttpHandler { 
                AllowAutoRedirect = false,
            });
        }
    }
}
