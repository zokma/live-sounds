using LiveSounds.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zokma.Libs;
using Zokma.Libs.Audio;

namespace LiveSounds.Service
{
    /// <summary>
    /// Service config.
    /// </summary>
    internal class ServiceConfig
    {
        /// <summary>
        /// Audio Items;
        /// </summary>
        public AudioInfo[] AudioItems;

        /// <summary>
        /// Audio data directory.
        /// </summary>
        public Pathfinder AudioDataDirectory;

        /// <summary>
        /// Audio player.
        /// </summary>
        public AudioPlayer AudioPlayer;

        /// <summary>
        /// Forwarding port.
        /// </summary>
        public int? ForwardingPort;
    }
}
