using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSounds.Notification
{

    /// <summary>
    /// Notification level.
    /// </summary>
    internal enum NotificationLevel
    {
        /// <summary>
        /// None.
        /// </summary>
        None,
        
        /// <summary>
        /// Info level notification.
        /// </summary>
        Info,

        /// <summary>
        /// Success level notification.
        /// </summary>
        Success,

        /// <summary>
        /// Warn level notification.
        /// </summary>
        Warn,

        /// <summary>
        /// Error level notification.
        /// </summary>
        Error,
    }
}
