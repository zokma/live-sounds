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
    internal class CachedAudioDataSampleProvider : AudioDataSampleProvider
    {
        /// <summary>
        /// Play back position.
        /// </summary>
        private long position;

        /// <summary>
        /// Creates Sample provider for cached AudioData.
        /// </summary>
        /// <param name="audioData">Audio data.</param>
        /// <param name="playbackToken">Playback token.</param>
        /// <param name="masterVolumeProvider">Master volume provider.</param>
        /// <param name="volume">Volume for audio playback.</param>
        /// <param name="useParallel">true if use parallel.</param>
        public CachedAudioDataSampleProvider(AudioData audioData, PlaybackToken playbackToken, IMasterVolumeProvider masterVolumeProvider, float volume = 1.0f, bool useParallel = false)
            : base(audioData, playbackToken, masterVolumeProvider, volume, useParallel)
        {
            this.position = 0L;
        }

        public override int Read(float[] buffer, int offset, int count)
        {
            var playbackState = this.playbackToken.State;

            if (playbackState == PlaybackState.StopRequested)
            {
                this.playbackToken.State = PlaybackState.Stopped;

                return 0;
            }

            var source = this.audioData.Data;

            if(source == null)
            {
                return 0;
            }

            float volume = this.audioData.Volume * this.volume * this.masterVolumeProvider.MasterVolume;

            long availableSamples = source.Length - position;
            long samplesToCopy    = Math.Min(availableSamples, count);


            if (AudioPlayer.EqualsVolume(volume, 0.0f))
            {
                Array.Clear(buffer, offset, (int)samplesToCopy);
            }
            else if (AudioPlayer.EqualsVolume(volume, 1.0f))
            {
                Array.Copy(source, position, buffer, offset, samplesToCopy);
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
                    this.playbackToken.IncrementLoopCount();
                }

                int toBeFilled = (int)(count - samplesToCopy);

                if(toBeFilled > 0)
                {
                    Array.Clear(buffer, (int)(offset + samplesToCopy), toBeFilled);
                }

                samplesToCopy = count;
            }
            else if(samplesToCopy < count)
            {
                this.playbackToken.State = PlaybackState.Stopped;
            }

            return (int)samplesToCopy;
        }
    }
}
