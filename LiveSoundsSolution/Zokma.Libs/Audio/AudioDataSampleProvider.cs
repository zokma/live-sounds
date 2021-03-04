using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zokma.Libs.Audio
{
    /// <summary>
    /// Sample provider for AudioData.
    /// </summary>
    internal abstract class AudioDataSampleProvider : ISampleProvider
    {
        /// <summary>
        /// Audio data.
        /// </summary>
        protected readonly AudioData audioData;

        /// <summary>
        /// Playback token.
        /// </summary>
        protected readonly PlaybackToken playbackToken;

        /// <summary>
        /// Master volume provider;
        /// </summary>
        protected readonly IMasterVolumeProvider masterVolumeProvider;

        /// <summary>
        /// Whether use parallel or not.
        /// </summary>
        protected readonly bool useParallel;

        /// <summary>
        /// Wave format.
        /// </summary>
        public NAudio.Wave.WaveFormat WaveFormat => this.audioData.WaveFormat.NAudioWaveFormat;

        /// <summary>
        /// Creates Sample provider for cached AudioData.
        /// </summary>
        /// <param name="audioData">Audio data.</param>
        /// <param name="playbackToken">Playback token.</param>
        /// <param name="masterVolumeProvider">Master volume provider.</param>
        /// <param name="useParallel">true if use parallel.</param>
        public AudioDataSampleProvider(AudioData audioData, PlaybackToken playbackToken, IMasterVolumeProvider masterVolumeProvider, bool useParallel = false)
        {
            this.audioData            = audioData;
            this.playbackToken        = playbackToken;
            this.masterVolumeProvider = masterVolumeProvider;
            this.useParallel          = useParallel;
        }

        public abstract int Read(float[] buffer, int offset, int count);
    }
}
