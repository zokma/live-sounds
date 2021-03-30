using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zokma.Libs.Audio
{
    /// <summary>
    /// Type of Audio Device.
    /// </summary>
    public enum AudioDeviceType
    {
        /// <summary>
        /// WASAPI.
        /// </summary>
        WASAPI,


        /// <summary>
        /// WaveOut/WaveIn.
        /// </summary>
        Wave,

        /// <summary>
        /// DirectSound.
        /// </summary>
        DirectSound,

        /// <summary>
        /// ASIO.
        /// </summary>
        ASIO,
    }

    public enum AudioDeviceRole
    {
        /// <summary>
        /// Games and system notifications.
        /// </summary>
        Console,

        /// <summary>
        /// Music and movies.
        /// </summary>
        Multimedia,

        /// <summary>
        /// Chat and VoIP.
        /// </summary>
        Communications,
    }


    /// <summary>
    /// Audio Device.
    /// </summary>
    public class AudioDevice
    {
        /// <summary>
        /// Audio Device Type.
        /// </summary>
        public AudioDeviceType DeviceType { get; set; }

        /// <summary>
        /// Audio Data flow.
        /// </summary>
        public AudioDataFlow DataFlow { get; set; }

        /// <summary>
        /// Device Id. Used for initializing.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Device Guid. Unique id for the device.
        /// </summary>
        public Guid Guid { get; private set; } = Guid.Empty;

        /// <summary>
        /// Device Number.
        /// </summary>
        public int Number { get; private set; }

        /// <summary>
        /// Device
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Friendly Name.
        /// </summary>
        public string FriendlyName { get; private set; }

        /// <summary>
        /// WASAPI only MMDevice.
        /// </summary>
        public MMDevice MMDevice { get; private set; }

        /// <summary>
        /// Gets <see cref="AudioDevice"/> from MMDevice.
        /// </summary>
        /// <param name="mmDevice">MMDevice.</param>
        /// <param name="dataFlow">Audio Data flow.</param>
        /// <returns><see cref="AudioDevice"/>.</returns>
        private static AudioDevice GetAudioDeviceFromMMDevice(MMDevice mmDevice, AudioDataFlow dataFlow)
        {
            var device = new AudioDevice
            {
                Id           = mmDevice.ID,
                DataFlow     = dataFlow,
                DeviceType   = AudioDeviceType.WASAPI,
                Name         = mmDevice.DeviceFriendlyName,
                FriendlyName = mmDevice.FriendlyName,
                MMDevice     = mmDevice,
            };

            return device;
        }

        /// <summary>
        /// Gets WASAPI Audio device.
        /// </summary>
        /// <param name="dataFlow">Audio data flow.</param>
        /// <param name="deviceRole">Audio device role.</param>
        /// <returns>Audio device list.</returns>
        private static AudioDevice[] GetWASAPIAudioDevices(AudioDataFlow dataFlow, AudioDeviceRole deviceRole = AudioDeviceRole.Multimedia)
        {
            var devices = new List<AudioDevice>();

            var mmde = new MMDeviceEnumerator();
            var role = deviceRole switch
            {
                AudioDeviceRole.Console        => Role.Console,
                AudioDeviceRole.Multimedia     => Role.Multimedia,
                AudioDeviceRole.Communications => Role.Communications,
                _                              => Role.Multimedia,
            };

            if (dataFlow.HasFlag(AudioDataFlow.Render))
            {
                try
                {
                    var device = mmde.GetDefaultAudioEndpoint(NAudio.CoreAudioApi.DataFlow.Render, role);
                    devices.Add(GetAudioDeviceFromMMDevice(device, AudioDataFlow.Render));
                }
                catch (Exception) { }

                foreach (var item in mmde.EnumerateAudioEndPoints(NAudio.CoreAudioApi.DataFlow.Render, DeviceState.Active))
                {
                    devices.Add(GetAudioDeviceFromMMDevice(item, AudioDataFlow.Render));
                }
            }

            return devices.ToArray();
        }

        /// <summary>
        /// Gets WaveOut/WaveIn audio device.
        /// </summary>
        /// <param name="dataFlow">Audio data flow.</param>
        /// <returns>Audio device list.</returns>
        private static AudioDevice[] GetWaveAudioDevices(AudioDataFlow dataFlow)
        {
            var devices = new List<AudioDevice>();

            if (dataFlow.HasFlag(AudioDataFlow.Render))
            {
                for (int i = -1; i < WaveOut.DeviceCount; i++)
                {
                    var caps = WaveOut.GetCapabilities(i);

                    devices.Add(
                        new AudioDevice
                        {
                            Id           = Convert.ToBase64String(Encoding.UTF8.GetBytes(String.Format("{0}.{1}", i, caps.ProductName)), Base64FormattingOptions.None).TrimEnd('='),
                            Guid         = caps.ProductGuid,
                            Number       = i,
                            DataFlow     = AudioDataFlow.Render,
                            DeviceType   = AudioDeviceType.Wave,
                            Name         = caps.ProductName,
                            FriendlyName = caps.ProductName,
                        }
                        );
                }
            }

            return devices.ToArray();
        }

        /// <summary>
        /// Get DirectSound audio device.
        /// </summary>
        /// <param name="dataFlow">Audio data flow.</param>
        /// <returns>Audio device list.</returns>
        private static AudioDevice[] GetDirectSoundAudioDevices(AudioDataFlow dataFlow)
        {
            var devices = new List<AudioDevice>();

            if (dataFlow.HasFlag(AudioDataFlow.Render))
            {
                foreach (var item in DirectSoundOut.Devices)
                {
                    devices.Add(
                        new AudioDevice
                        {
                            Id           = item.Guid.ToString(),
                            Guid         = item.Guid,
                            DataFlow     = AudioDataFlow.Render,
                            DeviceType   = AudioDeviceType.DirectSound,
                            Name         = item.ModuleName,
                            FriendlyName = item.Description,
                        }
                        );
                }
            }

            return devices.ToArray();
        }

        /// <summary>
        /// Gets ASIO audio device.
        /// </summary>
        /// <param name="dataFlow">Audio data flow.</param>
        /// <returns>Audio device list.</returns>
        private static AudioDevice[] GetASIOAudioDevices(AudioDataFlow dataFlow)
        {
            var devices = new List<AudioDevice>();

            if (dataFlow.HasFlag(AudioDataFlow.Render))
            {
                foreach (var item in AsioOut.GetDriverNames())
                {
                    devices.Add(
                        new AudioDevice
                        {
                            Id           = item,
                            DataFlow     = AudioDataFlow.Render,
                            DeviceType   = AudioDeviceType.ASIO,
                            Name         = item,
                            FriendlyName = item,
                        }
                        );
                }
            }

            return devices.ToArray();
        }

        /// <summary>
        /// Gets Audio devices.
        /// </summary>
        /// <param name="dataFlow">Audio data flow. In current version, only <see cref="AudioDataFlow.Render"/> is supported.</param>
        /// <param name="deviceType">Audio device type.</param>
        /// <param name="deviceRole">Audio device role.</param>
        /// <returns>Audio device list.</returns>
        public static AudioDevice[] GetAudioDevices(AudioDataFlow dataFlow, AudioDeviceType deviceType = AudioDeviceType.WASAPI, AudioDeviceRole deviceRole = AudioDeviceRole.Multimedia)
        {
            AudioDevice[] result;

            if (deviceType == AudioDeviceType.WASAPI)
            {
                result = GetWASAPIAudioDevices(dataFlow, deviceRole);
            }
            else if (deviceType == AudioDeviceType.Wave)
            {
                result = GetWaveAudioDevices(dataFlow);
            }
            else if (deviceType == AudioDeviceType.DirectSound)
            {
                result = GetDirectSoundAudioDevices(dataFlow);
            }
            else if (deviceType == AudioDeviceType.ASIO)
            {
                result = GetASIOAudioDevices(dataFlow);
            }
            else
            {
                throw new ArgumentException(String.Format("Invalid device type: {0}", deviceType), nameof(deviceType));
            }

            return result;
        }

        /// <summary>
        /// Gets Audio render devices.
        /// </summary>
        /// <param name="deviceType">Audio device type.</param>
        /// <param name="deviceRole">Audio device role.</param>
        /// <returns>Audio device list.</returns>
        public static AudioDevice[] GetAudioRenderDevices(AudioDeviceType deviceType = AudioDeviceType.WASAPI, AudioDeviceRole deviceRole = AudioDeviceRole.Multimedia)
        {
            return GetAudioDevices(AudioDataFlow.Render, deviceType, deviceRole);
        }

        public override bool Equals(object obj)
        {
            return (obj is AudioDevice device && this == device);
        }

        public override int GetHashCode()
        {
            return String.Format("{0}.{1}", this.DeviceType, this.Id).GetHashCode();
        }

        public static bool operator ==(AudioDevice a, AudioDevice b)
        {
            if(Object.ReferenceEquals(a, b))
            {
                return true;
            }

            if((Object)a == null || (Object)b == null)
            {
                return false;
            }

            return (a.DeviceType == b.DeviceType && a.Id == b.Id);
        }

        public static bool operator !=(AudioDevice a, AudioDevice b)
        {
            return !(a == b);
        }
    }
}
