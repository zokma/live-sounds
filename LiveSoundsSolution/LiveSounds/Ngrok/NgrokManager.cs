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
    internal class NgrokManager : IDisposable
    {
        /// <summary>
        /// Ngrok tunnels api.
        /// </summary>
        private const string NGROK_TUNNELS_API = "http://127.0.0.1:{0}/api/tunnels";

        /// <summary>
        /// Initial delay for retry.
        /// </summary>
        private const int RETRY_INITIAL_DELAY = 500;

        /// <summary>
        /// Retry max.
        /// </summary>
        private const int RETRY_MAX = 4;

        /// <summary>
        /// ngrok process.
        /// </summary>
        private NgrokProcess ngrokProcess;

        /// <summary>
        /// ngrok Api port.
        /// </summary>
        private int ngrokApiPort;


        private bool disposedValue;


        /// <summary>
        /// Creates NgrokManager.
        /// </summary>
        /// <param name="ngrokApiPort">ngrok Api port.</param>
        public NgrokManager(int ngrokApiPort)
        {
            this.ngrokApiPort = ngrokApiPort;
        }


        /// <summary>
        /// Find Ngrok tunnel info.
        /// </summary>
        /// <param name="ngrokApiPort">Port for Ngrok api.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Ngrok tunnel info.</returns>
        public async Task<TunnelInfo> FindTunnel(int forwardingPort, string ngrokRegion, CancellationToken cancellationToken)
        {
            TunnelInfo result = null;

            try
            {
                NgrokTunnels tunnels = null;

                tunnels = await FindNgrokTunnels(this.ngrokApiPort, cancellationToken);
                result  = FindTunnelInfo(tunnels, forwardingPort);

                if(tunnels != null && result == null)
                {
                    if(this.ngrokProcess != null)
                    {
                        if(this.ngrokProcess.Close())
                        {
                            tunnels = null;
                        }

                        this.ngrokProcess.Dispose();
                    }
                }

                if (tunnels == null)
                {
                    this.ngrokProcess = NgrokProcess.StartProcess(forwardingPort, ngrokRegion);

                    int delay = RETRY_INITIAL_DELAY;

                    for (int i = 0; i < RETRY_MAX; i++)
                    {
                        // delay will be 500, 1000, 2000, 4000 ms, so about 7500 ms in max.
                        await Task.Delay(delay, cancellationToken);

                        tunnels = await FindNgrokTunnels(this.ngrokApiPort, cancellationToken);
                        result  = FindTunnelInfo(tunnels, forwardingPort);

                        if(result != null)
                        {
                            break;
                        }

                        delay *= 2;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to find Ngrok tunneling info.");
            }

            cancellationToken.ThrowIfCancellationRequested();

            return result;
        }

        private static TunnelInfo FindTunnelInfo(NgrokTunnels ngrokTunnels, int forwardingPort)
        {
            TunnelInfo result = null;

            if(ngrokTunnels != null && ngrokTunnels.Tunnels != null)
            {
                foreach (var item in ngrokTunnels.Tunnels)
                {
                    if (item.Protocol       == "https" &&
                        item.PublicUrl      != null    && item.PublicUrl.StartsWith("https://") &&
                        item.ForwardingInfo != null    && item.ForwardingInfo.Port == forwardingPort)
                    {
                        result = item;
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Find Ngrok tunnels.
        /// </summary>
        /// <param name="ngrokApiPort">Port for Ngrok api.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Ngrok tunnels.</returns>
        public static async Task<NgrokTunnels> FindNgrokTunnels(int ngrokApiPort, CancellationToken cancellationToken)
        {
            NgrokTunnels result = null;

            try
            {
                using var http = new HttpClient(new SocketsHttpHandler
                {
                    AllowAutoRedirect = false,
                    ConnectTimeout    = AppSettings.HTTP_CONNECTION_TIMEOUT,
                });

                http.Timeout = AppSettings.HTTP_CLIENT_TIMEOUT;

                http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                using var response = await http.GetAsync(String.Format(NGROK_TUNNELS_API, ngrokApiPort), cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    using var content = response.Content;

                    string text = await content.ReadAsStringAsync(cancellationToken);

                    if(Log.IsDebugEnabled)
                    {
                        Log.Debug("Ngrok Tunnels: {TunnelsInfo}", text);
                    }

                    result = JsonSerializer.Deserialize<NgrokTunnels>(text);
                }
            }
            catch(Exception ex)
            {
                Log.Error(ex, "Failed to find Ngrok tunnelings.");
            }

            cancellationToken.ThrowIfCancellationRequested();

            return result;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.ngrokProcess?.Close();
                    this.ngrokProcess?.Dispose();
                    this.ngrokProcess = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~NgrokManager()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
