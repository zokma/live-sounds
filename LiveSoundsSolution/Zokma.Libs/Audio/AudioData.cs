using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zokma.Libs.Audio
{
    /// <summary>
    /// Audio data.
    /// </summary>
    public class AudioData
    {
        /// <summary>
        /// Min volume.
        /// </summary>
        private const float MIN_VOLUME = 0.0f;

        /// <summary>
        /// Max volume.
        /// </summary>
        private const float MAX_VOLUME = 1.0f;

        /// <summary>
        /// Audio file path.
        /// </summary>
        internal string FilePath { get; private set; }

        /// <summary>
        /// Cached audio data.
        /// </summary>
        internal float[] Data { get; private set; }

        /// <summary>
        /// Checks if the audio data is cached.
        /// </summary>
        public bool IsCached
        {
            get
            {
                return (this.Data != null);
            }
        }

        /// <summary>
        /// Audio wave format.
        /// </summary>
        public WaveFormat WaveFormat { get; private set; }

        /// <summary>
        /// Audio volume.
        /// </summary>
        public float Volume { get; private set; }

        /// <summary>
        /// Loads audio file.
        /// </summary>
        /// <param name="filePath">Audio file path.</param>
        /// <param name="isCached">true if the audio file is cached on the memory.</param>
        /// <param name="volume">Audio volume.</param>
        /// <returns>Audio data.</returns>
        public static AudioData LoadAudio(string filePath, bool isCached = true, float volume = 1.0f)
        {
            // Instanciate once to check format even if the audio data will not be cached.
            using var reader = new AudioFileReader(filePath);

            var audioData = new AudioData()
            {
                FilePath   = filePath,
                WaveFormat = reader.WaveFormat,
                Volume     = Math.Min(Math.Max(volume, MIN_VOLUME), MAX_VOLUME),
            };

            if (isCached)
            {
                var buffer     = new List<float>((int)(reader.Length << 2));
                var readBuffer = new float[reader.WaveFormat.SampleRate * reader.WaveFormat.Channels];

                int samplesRead;

                while ((samplesRead = reader.Read(readBuffer, 0, readBuffer.Length)) > 0)
                {
                    buffer.AddRange(readBuffer.Take(samplesRead));
                }

                audioData.Data = buffer.ToArray();
            }

            return audioData;
        }
    }
}
