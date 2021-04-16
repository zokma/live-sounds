using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Zokma.Libs.Logging;

namespace LiveSounds.Ngrok
{
    /// <summary>
    /// Ngrok manager.
    /// </summary>
    internal class NgrokManager
    {
        /// <summary>
        /// Ngrok tunnels api.
        /// </summary>
        private const string NGROK_TUNNELS_API = "http://127.0.0.1:{0}/api/tunnels";

        /// <summary>
        /// Find Ngrok tunnel info.
        /// </summary>
        /// <param name="ngrokApiPort">Port for Ngrok api.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Ngrok tunnel info.</returns>
        public static async Task<TunnelInfo> FindTunnel(int ngrokApiPort, CancellationToken cancellationToken)
        {
            TunnelInfo result = null;

            try
            {
                using var http = new HttpClient(new SocketsHttpHandler
                {
                    AllowAutoRedirect = false,
                });

                http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                using var response = await http.GetAsync(String.Format(NGROK_TUNNELS_API, ngrokApiPort), cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    using var content = response.Content;

                    string text = await content.ReadAsStringAsync();

                    var ngrokTunnels = JsonSerializer.Deserialize<NgrokTunnels>(text);

                    foreach (var item in ngrokTunnels.Tunnels)
                    {
                        if (item.Protocol == "https")
                        {
                            result = item;
                            break;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Log.Error(ex, "Failed to find Ngrok tunneling info.");
            }

            cancellationToken.ThrowIfCancellationRequested();

            return result;
        }
    }
}
