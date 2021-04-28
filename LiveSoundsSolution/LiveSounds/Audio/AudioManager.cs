using LiveSounds.Localization;
using LiveSounds.Notification;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zokma.Libs;
using Zokma.Libs.Audio;
using Zokma.Libs.Logging;

namespace LiveSounds.Audio
{

    /// <summary>
    /// Audio Manager.
    /// </summary>
    internal class AudioManager : IDisposable
    {
        /// <summary>
        /// Dictionary for AudioData.
        /// </summary>
        private Dictionary<string, AudioData> audios;
        private bool disposedValue;

        /// <summary>
        /// Audio item list.
        /// </summary>
        public IList<AudioItem> AudioItems { get; private set; }

        /// <summary>
        /// Constactor.
        /// </summary>
        private AudioManager()
        {
            this.audios     = new Dictionary<string, AudioData>();
        }


        /// <summary>
        /// Creates AudioManager.
        /// </summary>
        /// <param name="audioInfoList">Audio info list.</param>
        /// <param name="dataDirectory">Audio data directory.</param>
        /// <param name="notification">Notification manager.</param>
        /// <returns>AudioManager.</returns>
        public static AudioManager CreateAudioManager(AudioInfo[] audioInfoList, Pathfinder dataDirectory, NotificationManager notification)
        {
            var audioManager = new AudioManager();

            var loadedAudios = new Dictionary<string, AudioData>();
            var audioItems   = new List<AudioItem>();

            var settings = App.Settings;

            var audioWaveFormat   = settings.AudioWaveFormat;
            int resamplingQuality = settings.AudioResamplingQuality;

            int itemsLimit = settings.ItemsLimit;

            foreach (var item in audioInfoList)
            {
                try
                {
                    if (!String.IsNullOrWhiteSpace(item.File) && item.Name != null)
                    {
                        AudioData data;

                        if (!loadedAudios.TryGetValue(item.File, out data))
                        {
                            data = AudioData.LoadAudio(dataDirectory.FindPathName(item.File), audioWaveFormat, item.IsCached, item.Volume, resamplingQuality);

                            loadedAudios.Add(item.File, data);
                        }

                        string guid = Guid.NewGuid().ToString("N");

                        audioManager.audios.Add(guid, data);
                        audioItems.Add(new AudioItem { Id = guid, Name = item.Name, Data = data });

                        itemsLimit--;
                        if(itemsLimit <= 0)
                        {
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Failed to load audio file: FileName = {FileName}", item.File);

                    notification.Notify(
                        String.Format(LocalizedInfo.MessagePatternFailedToLoadAudioFile, item.File),
                        NotificationLevel.Warn);
                }
            }

            audioManager.AudioItems = ImmutableList.Create(audioItems.ToArray());

            return audioManager;
        }

        /// <summary>
        /// Tries to get AudioData corresponds with the key.
        /// </summary>
        /// <param name="key">key for the audio.</param>
        /// <param name="audioData">Audio data.</param>
        /// <returns>true if the data is got successfully.</returns>
        public bool TryGetAudioData(string key, out AudioData audioData)
        {
            return this.audios.TryGetValue(key, out audioData);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (this.AudioItems != null)
                    {
                        foreach (var item in this.AudioItems)
                        {
                            item.Data?.Dispose();
                        }
                    }

                    this.audios.Clear();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~AudioManager()
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
