using LiveSounds.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zokma.Libs.Audio;

namespace LiveSounds.MenuItem
{

    /// <summary>
    /// Menu item for an AudioDevice.
    /// </summary>
    internal class AudioDeviceItem
    {
        /// <summary>
        /// Audio device.
        /// </summary>
        internal AudioDevice Device { get; private set; }

        /// <summary>
        /// Creates menu item.
        /// </summary>
        /// <param name="device">AudioDevice.</param>
        public AudioDeviceItem(AudioDevice device)
        {
            this.Device = device;
        }

        /// <summary>
        /// Returns display string.
        /// </summary>
        /// <returns>Display string of the menu.</returns>
        public override string ToString()
        {
            if (this.Device == null)
            {
                return LocalizedInfo.MenuItemNotFoundForAudioDevice;
            }

            return this.Device.FriendlyName;
        }
    }
}
