using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zokma.Libs.Audio
{
    /// <summary>
    /// Audio player.
    /// </summary>
    public class AudioPlayer : IMasterVolumeProvider, IDisposable
    {
        /// <summary>
        /// Epsilon value for volume.
        /// </summary>
        internal const float VOLUME_EPSILON = 0.00001f;

        /// <summary>
        /// Min volume.
        /// </summary>
        internal const float MIN_VOLUME = 0.0f;

        /// <summary>
        /// Max volume.
        /// </summary>
        internal const float MAX_VOLUME = 1.0f;

        /// <summary>
        /// Default Wave format.
        /// </summary>
        public static readonly WaveFormat DefaultWaveFormat = new WaveFormat(44100, 2);

        /// <summary>
        /// Audio device.
        /// </summary>
        private readonly AudioDevice device;

        /// <summary>
        /// Wave format.
        /// </summary>
        public WaveFormat WaveFormat { get; private set; }

        /// <summary>
        /// Master volume.
        /// </summary>
        private float masterVolume = 1.0f;

        /// <summary>
        /// Audio engine share mode.
        /// </summary>
        private readonly AudioEngineShareMode shareMode;

        /// <summary>
        /// Audio engine latency.
        /// </summary>
        private readonly int latency;

        /// <summary>
        /// Indicates whether use parallel or not.
        /// </summary>
        private readonly bool useParallel;

        /// <summary>
        /// Master volume.
        /// </summary>
        public float MasterVolume
        {
            get => this.masterVolume;

            set
            {
                this.masterVolume = Math.Min(Math.Max(value, MIN_VOLUME), MAX_VOLUME);
            }
        }

        /// <summary>
        /// Status of this engine.
        /// </summary>
        public AudioEngineState State { get; private set; }

        /// <summary>
        /// Wave player;
        /// </summary>
        private IWavePlayer player;

        /// <summary>
        /// Wave mixer;
        /// </summary>
        private readonly MixingSampleProvider mixer;

        /// <summary>
        /// RockLock to lock operations.
        /// </summary>
        private readonly RockLock rockLock;

        private bool disposedValue;


        /// <summary>
        /// Creates audio engine.
        /// </summary>
        /// <param name="device">Audio playback device.</param>
        /// <param name="waveFormat">Wave format to process.</param>
        /// <param name="shareMode">Audio share mode.</param>
        /// <param name="latency">Desired latency.</param>
        /// <param name="useParallel">true if parralle is used.</param>
        public AudioPlayer(AudioDevice device, WaveFormat waveFormat, AudioEngineShareMode shareMode = AudioEngineShareMode.Shared, int latency = 200, bool useParallel = false)
        {
            this.device     = device;
            this.WaveFormat = waveFormat;

            this.shareMode   = shareMode;
            this.latency     = latency;
            this.useParallel = useParallel;

            this.mixer = new MixingSampleProvider(waveFormat.NAudioWaveFormat)
            {
                ReadFully = true
            };

            this.rockLock = new RockLock();
            this.State    = AudioEngineState.Created;
        }

        /// <summary>
        /// Creates audio engine.
        /// </summary>
        /// <param name="device">Audio playback device.</param>
        /// <param name="shareMode">Audio share mode.</param>
        /// <param name="latency">Desired latency.</param>
        /// <param name="useParallel">true if parralle is used.</param>
        public AudioPlayer(AudioDevice device, AudioEngineShareMode shareMode = AudioEngineShareMode.Shared, int latency = 200, bool useParallel = false)
            : this(device, DefaultWaveFormat, shareMode, latency, useParallel)
        {
        }

        /// <summary>
        /// Checks if the volumes are the same.
        /// </summary>
        /// <param name="volume1">Volume 1.</param>
        /// <param name="volume2">Volume 2.</param>
        /// <returns>true if the volumes are the same.</returns>
        internal static bool EqualsVolume(float volume1, float volume2)
        {
            float diff = Math.Abs(volume1 - volume2);

            return (diff < VOLUME_EPSILON);
        }

        /// <summary>
        /// Loads audio file for this player.
        /// The audio data will be resampled, if needed.
        /// </summary>
        /// <param name="filePath">Audio file path.</param>
        /// <param name="isCached">true if the audio file is cached on the memory.</param>
        /// <param name="volume">Audio volume.</param>
        /// <param name="resamplingQuality">Resampling quality from min(1) to max(60).</param>
        /// <returns>Audio data.</returns>
        public AudioData LoadAudio(string filePath, bool isCached = true, float volume = 1.0f, int resamplingQuality = 60)
        {
            return AudioData.LoadAudio(filePath, this.WaveFormat, isCached, volume, resamplingQuality);
        }

        /// <summary>
        /// Checks this AudioPlayer is disposed.
        /// </summary>
        public void CheckDisposed()
        {
            if (this.disposedValue)
            {
                throw new ObjectDisposedException(nameof(AudioPlayer));
            }
        }

        /// <summary>
        /// Inits player.
        /// </summary>
        public void Init()
        {
            CheckDisposed();

            using (this.rockLock.EnterWriteLock())
            {
                if (this.State == AudioEngineState.Created)
                {
                    IWavePlayer player;

                    var deviceType = this.device.DeviceType;

                    if (deviceType == AudioDeviceType.WASAPI)
                    {
                        var shareMode = AudioClientShareMode.Shared;

                        if (this.shareMode == AudioEngineShareMode.Exclusive)
                        {
                            shareMode = AudioClientShareMode.Exclusive;
                        }

                        player = new WasapiOut(this.device.MMDevice, shareMode, true, this.latency);
                    }
                    else if (deviceType == AudioDeviceType.Wave)
                    {
                        player = new WaveOutEvent()
                        {
                            DeviceNumber   = this.device.Number,
                            DesiredLatency = this.latency,
                        };
                    }
                    else if (deviceType == AudioDeviceType.DirectSound)
                    {
                        player = new DirectSoundOut(this.device.Guid, this.latency);
                    }
                    else if (deviceType == AudioDeviceType.ASIO)
                    {
                        player = new AsioOut(this.device.Name);
                    }
                    else
                    {
                        throw new InvalidOperationException("Unknown device type.");
                    }

                    try
                    {
                        player.Init(this.mixer);

                        this.player = player;
                        this.State  = AudioEngineState.Initialized;
                    }
                    catch (Exception)
                    {
                        player?.Dispose();

                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Starts player.
        /// </summary>
        public void Start()
        {
            CheckDisposed();

            if (this.State == AudioEngineState.Created)
            {
                Init();
            }

            using (this.rockLock.EnterWriteLock())
            {
                if (this.State == AudioEngineState.Initialized || this.State == AudioEngineState.Stopped || this.State == AudioEngineState.Paused)
                {
                    this.player.Play();
                    this.State = AudioEngineState.Started;
                }
            }
        }

        /// <summary>
        /// Stops player.
        /// </summary>
        public void Stop()
        {
            CheckDisposed();

            using (this.rockLock.EnterWriteLock())
            {
                if (this.State == AudioEngineState.Started || this.State == AudioEngineState.Paused)
                {
                    this.player.Stop();
                    this.State = AudioEngineState.Stopped;
                }
            }
        }

        /// <summary>
        /// Pauses player.
        /// </summary>
        public void Pause()
        {
            CheckDisposed();

            using (this.rockLock.EnterWriteLock())
            {
                if (this.State == AudioEngineState.Started)
                {
                    this.player.Pause();
                    this.State = AudioEngineState.Paused;
                }
            }
        }

        /// <summary>
        /// Converts to right channle count.
        /// </summary>
        /// <param name="input">Input.</param>
        /// <returns>Resutl sample provider.</returns>
        private ISampleProvider ConvertToRightChannelCount(ISampleProvider input)
        {
            if (input.WaveFormat.Channels == this.mixer.WaveFormat.Channels)
            {
                return input;
            }
            else if(input.WaveFormat.Channels == 1 && this.mixer.WaveFormat.Channels == 2)
            {
                return new MonoToStereoSampleProvider(input);
            }

            throw new NotImplementedException(String.Format($"Not implemeted - input channels = {input.WaveFormat.Channels}, mixer channels = {this.mixer.WaveFormat.Channels}"));
        }

        /// <summary>
        /// Adds to mixer.
        /// </summary>
        /// <param name="input">Input.</param>
        private void AddMixerInput(ISampleProvider input)
        {
            this.mixer.AddMixerInput(ConvertToRightChannelCount(input));
        }

        public PlaybackToken Play(AudioData audioData, PlaybackMode mode = PlaybackMode.Once)
        {
            CheckDisposed();

            if (audioData.WaveFormat.SampleRate != this.WaveFormat.SampleRate)
            {
                throw new InvalidOperationException("The sample rate of audio data does not much the sample rate of this player.");
            }

            using (this.rockLock.EnterReadLock())
            {
                if(this.State != AudioEngineState.Started && this.State != AudioEngineState.Paused)
                {
                    throw new InvalidAudioEngineStateException("The Audio Engine is not started or paused.");
                }

                var token = new PlaybackToken(mode == PlaybackMode.Loop);

                if (audioData.IsCached)
                {
                    AddMixerInput(new CachedAudioDataSampleProvider(audioData, token, this, this.useParallel));
                }

                return token;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.player?.Dispose();
                    this.State = AudioEngineState.Disposed;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~AudioPlayer()
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
