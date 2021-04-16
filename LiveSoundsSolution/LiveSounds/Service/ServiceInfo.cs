using LiveSounds.Ngrok;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSounds.Service
{

    /// <summary>
    /// Service info.
    /// </summary>
    internal class ServiceInfo
    {
        /// <summary>
        /// Service manager.
        /// </summary>
        private ServiceManager manager;

        /// <summary>
        /// true if the service is running.
        /// </summary>
        public bool IsRunning => this.manager.IsRunning;

        /// <summary>
        /// Tunnel info.
        /// </summary>
        public TunnelInfo TunnelInfo { get; private set; }

        /// <summary>
        /// Creates ServiceInfo.
        /// </summary>
        /// <param name="manager">ServiceManager.</param>
        public ServiceInfo(ServiceManager manager, TunnelInfo tunnelInfo)
        {
            this.manager    = manager;
            this.TunnelInfo = tunnelInfo;
        }
    }
}
