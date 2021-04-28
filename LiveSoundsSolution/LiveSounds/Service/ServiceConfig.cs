using LiveSounds.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zokma.Libs;

namespace LiveSounds.Service
{
    /// <summary>
    /// Service config.
    /// </summary>
    internal class ServiceConfig
    {
        /// <summary>
        /// Notification manager.
        /// </summary>
        public NotificationManager NotificationManager;

        /// <summary>
        /// Port for Ngrok api.
        /// </summary>
        public int NgrokApiPort;

        /// <summary>
        /// Audio Items;
        /// </summary>
        public AudioInfo[] AudioItems;

        /// <summary>
        /// Audio data directory.
        /// </summary>
        public Pathfinder AudioDataDirectory;

    }
}
