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
    internal class AudioDataFileSampleProvider : AudioDataSampleProvider
    {

        /// <summary>
        /// Audio File Reader;
        /// </summary>
        private readonly AudioFileReader reader;

        /// <summary>
        /// Whether audio file reader disposed or not.
        /// </summary>
        private bool isAudioFileReaderDisposed;

        /// <summary>
        /// Resampler.
        /// </summary>
        private readonly MediaFoundationResampler resampler;

        /// <summary>
        /// Read stream.
        /// </summary>
        private readonly ISampleProvider stream;

        /// <summary>
        /// Creates Sample provider for cached AudioData.
        /// </summary>
        /// <param name="audioData">Audio data.</param>
        /// <param name="playbackToken">Playback token.</param>
        /// <param name="masterVolumeProvider">Master volume provider.</param>
        /// <param name="useParallel">true if use parallel.</param>
        public AudioDataFileSampleProvider(AudioData audioData, PlaybackToken playbackToken, IMasterVolumeProvider masterVolumeProvider, bool useParallel = false)
            : base(audioData, playbackToken, masterVolumeProvider, useParallel)
        {
            this.reader = new AudioFileReader(audioData.FilePath);
            this.isAudioFileReaderDisposed = false;

            this.resampler = AudioData.CreateResampler(this.reader, audioData.WaveFormat, audioData.ResamplingQuality);

            if (this.resampler != null)
            {
                this.stream = resampler.ToSampleProvider();
            }
            else
            {
                this.stream = reader;
            }
        }

        public override int Read(float[] buffer, int offset, int count)
        {
            var playbackState = this.playbackToken.State;

            if (playbackState == PlaybackState.StopRequested || this.isAudioFileReaderDisposed)
            {
                this.playbackToken.State = PlaybackState.Stopped;

                return 0;
            }

            float volume = this.masterVolumeProvider.MasterVolume * this.audioData.Volume;

            int read = this.stream.Read(buffer, offset, count);

            if (read == 0)
            {
                if (playbackState == PlaybackState.PlayingInLoop)
                {
                    this.reader.Position = 0;
                    this.playbackToken.IncrementLoopCount();
                }
                else
                {
                    this.resampler?.Dispose();
                    this.reader.Dispose();
                    this.isAudioFileReaderDisposed = true;

                    this.playbackToken.State = PlaybackState.Stopped;
                }
            }
            else if (AudioPlayer.EqualsVolume(volume, 0.0f))
            {
                Array.Clear(buffer, offset, read);
            }
            else if (!AudioPlayer.EqualsVolume(volume, 1.0f))
            {
                if (!this.useParallel)
                {
                    for (long i = 0; i < read; i++)
                    {
                        buffer[offset + i] *= volume;
                    }
                }
                else
                {
                    Parallel.For(0, read,
                        (i) =>
                        {
                            buffer[offset + i] *= volume;
                        });
                }
            }

            // If loop mode is enabled, remaining area of buffer is zero filled.
            // This is a reasonable way, but there may be a very small difference according to source audio data.
            if (playbackState == PlaybackState.PlayingInLoop && read < count)
            {
                Array.Clear(buffer, (offset + read), (count - read));
                read = count;
            }

            return read;
        }
    }
}
