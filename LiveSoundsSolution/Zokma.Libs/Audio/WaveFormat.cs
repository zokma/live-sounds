using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zokma.Libs.Audio
{
    public class WaveFormat
    {
        /// <summary>
        /// NAudio <see cref="NAudio.Wave.WaveFormat"/>.
        /// </summary>
        internal NAudio.Wave.WaveFormat NAudioWaveFormat { get; private set; }

        /// <summary>
        /// Channel numbers.
        /// </summary>
        public int Channels => NAudioWaveFormat.Channels;

        /// <summary>
        /// Sample rate.
        /// </summary>
        public int SampleRate => NAudioWaveFormat.SampleRate;

        /// <summary>
        /// Average bytes per sec.
        /// </summary>
        public int AverageBytesPerSecond => NAudioWaveFormat.AverageBytesPerSecond;

        /// <summary>
        /// Block alignment.
        /// </summary>
        public int BlockAlign => NAudioWaveFormat.BlockAlign;

        /// <summary>
        /// Bits per sample.
        /// </summary>
        public int BitsPerSample => NAudioWaveFormat.BitsPerSample;

        /// <summary>
        /// Creats a new Wave format.
        /// </summary>
        /// <param name="sampleRate">Sample rate.</param>
        /// <param name="channels">Channel numbers.</param>
        public WaveFormat(int sampleRate, int channels)
        {
            this.NAudioWaveFormat = NAudio.Wave.WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channels);
        }

        internal WaveFormat(NAudio.Wave.WaveFormat naudioWaveFormat)
            : this(naudioWaveFormat.SampleRate, naudioWaveFormat.Channels)
        {
        }

    }
}
