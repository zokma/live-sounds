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
        /// Device Id.
        /// </summary>
        public string DeviceId { get; private set; }

        /// <summary>
        /// Dispolay Name.
        /// </summary>
        public string DisplayName { get; private set; }

        /// <summary>
        /// Creates menu item.
        /// </summary>
        /// <param name="device">AudioDevice.</param>
        public AudioDeviceItem(AudioDevice device)
        {
            this.DeviceId    = device?.Id;
            this.DisplayName = device?.FriendlyName;
        }

        /// <summary>
        /// Returns display string.
        /// </summary>
        /// <returns>Display string of the menu.</returns>
        public override string ToString()
        {
            if (this.DeviceId == null)
            {
                return LocalizedInfo.MenuItemNotFoundForAudioDevice;
            }

            return this.DisplayName;
        }
    }
}
