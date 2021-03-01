using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zokma.Libs.Audio
{
    /// <summary>
    /// Sample provider for cached AudioData.
    /// </summary>
    internal class CachedAudioDataSampleProvider : ISampleProvider
    {
        /// <summary>
        /// Audio data.
        /// </summary>
        private readonly AudioData audioData;

        /// <summary>
        /// Playback token.
        /// </summary>
        private readonly PlaybackToken playbackToken;

        /// <summary>
        /// Master volume provider;
        /// </summary>
        private readonly IMasterVolumeProvider masterVolumeProvider;

        /// <summary>
        /// Whether use parallel or not.
        /// </summary>
        private readonly bool useParallel;

        /// <summary>
        /// Play back position.
        /// </summary>
        private long position;

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
        public CachedAudioDataSampleProvider(AudioData audioData, PlaybackToken playbackToken, IMasterVolumeProvider masterVolumeProvider, bool useParallel = false)
        {
            this.audioData            = audioData;
            this.playbackToken        = playbackToken;
            this.masterVolumeProvider = masterVolumeProvider;
            this.useParallel          = useParallel;

            this.position = 0L;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            var playbackState = this.playbackToken.State;

            if (playbackState == PlaybackState.StopRequested)
            {
                this.playbackToken.State = PlaybackState.Stopped;

                return 0;
            }

            float volume = this.masterVolumeProvider.MasterVolume * this.audioData.Volume;

            long availableSamples = this.audioData.Data.Length - position;
            long samplesToCopy    = Math.Min(availableSamples, count);

            var source = this.audioData.Data;

            if (AudioPlayer.EqualsVolume(volume, 0.0f))
            {
                Array.Clear(buffer, offset, (int)samplesToCopy);
            }
            else if (AudioPlayer.EqualsVolume(volume, 1.0f))
            {
                Array.Copy(this.audioData.Data, position, buffer, offset, samplesToCopy);
            }
            else
            {
                if (!this.useParallel)
                {
                    for (long i = 0; i < samplesToCopy; i++)
                    {
                        buffer[offset + i] = source[position + i] * volume;
                    }
                }
                else
                {
                    Parallel.For(0, samplesToCopy,
                        (i) =>
                        {
                            buffer[offset + i] = source[position + i] * volume;
                        });
                }
            }

            position += samplesToCopy;

            // If loop mode is enabled, remaining area of buffer is zero filled.
            // This is a reasonable way, but there may be a very small difference according to source audio data.
            if (playbackState == PlaybackState.PlayingInLoop)
            {
                if(position >= source.Length)
                {
                    position = 0;
                }

                int toBeFilled = (int)(count - samplesToCopy);

                if(toBeFilled > 0)
                {
                    Array.Clear(buffer, (int)(offset + samplesToCopy), toBeFilled);
                }

                samplesToCopy = count;
            }

            return (int)samplesToCopy;
        }
    }
}
