using LiveSounds.Localization;
using LiveSounds.Ngrok;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LiveSounds.Service
{
    /// <summary>
    /// Service Manager.
    /// </summary>
    internal class ServiceManager
    {
        /// <summary>
        /// TokenSource to cancel.
        /// </summary>
        private CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// Token to cancel.
        /// </summary>
        private CancellationToken cancellationToken;

        /// <summary>
        /// true if the service is running.
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Creates ServiceManager.
        /// </summary>
        public ServiceManager()
        {
            this.IsRunning = false;

            this.cancellationTokenSource = new CancellationTokenSource();
            this.cancellationToken       = this.cancellationTokenSource.Token;
        }

        public async Task<ServiceInfo> StartService(ServiceConfig config)
        {
            var notification = config.NotificationManager;

            var tunnelInfo = await NgrokManager.FindTunnel(config.NgrokApiPort, this.cancellationToken);

            if(tunnelInfo == null)
            {
                notification.ShowNotification(LocalizedInfo.MessageNoNgrokDetected, Notification.NotificationLevel.Error);
            }
            else
            {



                this.IsRunning = true;
            }

            return new ServiceInfo(this, tunnelInfo);
        }

        /// <summary>
        /// Stops the service.
        /// </summary>
        public void Stop()
        {
            this.IsRunning = false;
            this.cancellationTokenSource.Cancel();
        }
    }
}
