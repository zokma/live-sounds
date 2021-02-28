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
        /// Min resampling quality.
        /// </summary>
        private const int MIN_RESAMPLING_QUALITY = 1;

        /// <summary>
        /// Max resampling quality.
        /// </summary>
        private const int MAX_RESAMPLING_QUALITY = 60;


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
        /// Resampling Quality.
        /// </summary>
        internal int ResamplingQuality { get; private set; }

        /// <summary>
        /// Audio volume.
        /// </summary>
        public float Volume { get; private set; }

        /// <summary>
        /// Creates resampler.
        /// </summary>
        /// <param name="reader">Audio file reader.</param>
        /// <param name="targetFormat">Target Wave format.</param>
        /// <param name="resamplingQuality">Resampling quality from min(1) to max(60).</param>
        /// <returns>Resampler.</returns>
        private static MediaFoundationResampler CreateResampler(AudioFileReader reader, WaveFormat targetFormat, int resamplingQuality)
        {
            if(reader.WaveFormat.SampleRate != targetFormat.SampleRate)
            {
                var format    = NAudio.Wave.WaveFormat.CreateIeeeFloatWaveFormat(targetFormat.SampleRate, targetFormat.Channels);
                var resampler = new MediaFoundationResampler(reader, format);

                resampler.ResamplerQuality = resamplingQuality;

                return resampler;
            }

            return null;
        }

        /// <summary>
        /// Loads audio file.
        /// </summary>
        /// <param name="filePath">Audio file path.</param>
        /// <param name="waveFormat">Target Wave format. The format will be resampled if needed.</param>
        /// <param name="isCached">true if the audio file is cached on the memory.</param>
        /// <param name="volume">Audio volume.</param>
        /// <param name="resamplingQuality">Resampling quality from min(1) to max(60).</param>
        /// <returns>Audio data.</returns>
        public static AudioData LoadAudio(string filePath, WaveFormat waveFormat = null, bool isCached = true, float volume = 1.0f, int resamplingQuality = 60)
        {
            // Instantiate once to check format even if the audio data will not be cached.
            using var reader = new AudioFileReader(filePath);

            var audioData = new AudioData()
            {
                FilePath          = filePath,
                WaveFormat        = ((waveFormat != null) ? (new WaveFormat(waveFormat.SampleRate, reader.WaveFormat.Channels)) : (new WaveFormat(reader.WaveFormat))),
                ResamplingQuality = Math.Min(Math.Max(resamplingQuality, MIN_RESAMPLING_QUALITY), MAX_RESAMPLING_QUALITY),
                Volume            = Math.Min(Math.Max(volume, MIN_VOLUME), MAX_VOLUME),
            };

            if (isCached)
            {
                var buffer     = new List<float>((int)(reader.Length << 2));
                var readBuffer = new float[audioData.WaveFormat.SampleRate * audioData.WaveFormat.Channels];

                using var resampler = CreateResampler(reader, audioData.WaveFormat, audioData.ResamplingQuality);

                ISampleProvider stream;

                if(resampler != null)
                {
                    stream = WaveExtensionMethods.ToSampleProvider(resampler);
                }
                else
                {
                    stream = reader;
                }

                int samplesRead;

                while ((samplesRead = stream.Read(readBuffer, 0, readBuffer.Length)) > 0)
                {
                    buffer.AddRange(readBuffer.Take(samplesRead));
                }

                audioData.Data = buffer.ToArray();
            }

            return audioData;
        }

        /// <summary>
        /// Loads audio file.
        /// </summary>
        /// <param name="filePath">Audio file path.</param>
        /// <param name="isCached">true if the audio file is cached on the memory.</param>
        /// <param name="volume">Audio volume.</param>
        /// <returns>Audio data.</returns>
        public static AudioData LoadAudio(string filePath, bool isCached = true, float volume = 1.0f)
        {
            return LoadAudio(filePath, null, isCached, volume);
        }

        /// <summary>
        /// Loads audio file.
        /// </summary>
        /// <param name="filePath">Audio file path.</param>
        /// <returns>Audio data.</returns>
        public static AudioData LoadAudio(string filePath)
        {
            return LoadAudio(filePath, null);
        }
    }
}
