using LiveSounds.Api;
using LiveSounds.Audio;
using LiveSounds.Localization;
using LiveSounds.Ngrok;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Zokma.Libs.Logging;

namespace LiveSounds.Service
{
    /// <summary>
    /// Service Manager.
    /// </summary>
    internal class ServiceManager
    {
        /// <summary>
        /// Secret length;
        /// </summary>
        private const int SECRET_LENGTH = 40;

        /// <summary>
        /// Audio renderings path pattern.
        /// </summary>
        private const string AUDIO_RENDERINGS_PATH_PATTERN = "/{0}/audio/renderings";

        /// <summary>
        /// Random generator.
        /// </summary>
        private static readonly RNGCryptoServiceProvider random = new RNGCryptoServiceProvider();

        /// <summary>
        /// TokenSource to cancel.
        /// </summary>
        private CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// Token to cancel.
        /// </summary>
        private CancellationToken cancellationToken;

        /// <summary>
        /// true if the service is running.
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// ZokmaApi.
        /// </summary>
        private ZokmaApi zokmaApi;

        /// <summary>
        /// Audio manager.
        /// </summary>
        private AudioManager audioManager;

        /// <summary>
        /// Resouce path.
        /// </summary>
        private string resourcePath;

        /// <summary>
        /// Sound id.
        /// </summary>
        private string soundId;

        /// <summary>
        /// Creates ServiceManager.
        /// </summary>
        public ServiceManager()
        {
            this.IsRunning = false;
        }

        /// <summary>
        /// Starts service.
        /// </summary>
        /// <param name="config">Service config.</param>
        /// <returns>Service info.</returns>
        public async Task<ServiceInfo> StartService(ServiceConfig config)
        {
            if(this.IsRunning)
            {
                await this.Stop();
            }

            this.cancellationTokenSource = new CancellationTokenSource();
            this.cancellationToken       = this.cancellationTokenSource.Token;

            var notification = config.NotificationManager;

            var tunnelInfo = await NgrokManager.FindTunnel(config.NgrokApiPort, this.cancellationToken);

            int validitySeconds = 0;

            if(tunnelInfo == null)
            {
                notification.ShowNotification(LocalizedInfo.MessageNoNgrokDetected, Notification.NotificationLevel.Error);
            }
            else
            {
                this.audioManager = AudioManager.CreateAudioManager(config.AudioItems, config.AudioDataDirectory, notification);

                if (this.audioManager.AudioItems == null || this.audioManager.AudioItems.Count <= 0)
                {
                    notification.ShowNotification(LocalizedInfo.MessageValidAudioFileNotFound, Notification.NotificationLevel.Error);
                }
                else
                {
                    var settings = App.Settings;

                    this.zokmaApi = new ZokmaApi(settings.Token);

                    var secret = new byte[SECRET_LENGTH];
                    random.GetBytes(secret);

                    this.resourcePath = String.Format(AUDIO_RENDERINGS_PATH_PATTERN, Guid.NewGuid().ToString("N"));

                    try
                    {
                        var sound = await zokmaApi.CreateSound(
                                                (tunnelInfo.PublicUrl + this.resourcePath),
                                                this.audioManager.AudioItems.ToArray(),
                                                Convert.ToBase64String(secret, Base64FormattingOptions.None),
                                                settings.ServiceValiditySeconds,
                                                null, null,
                                                this.cancellationToken);

                        if (sound != null)
                        {
                            this.soundId    = sound.Id;
                            validitySeconds = sound.ValiditySeconds;

                            this.IsRunning = true;
                        }
                    }
                    catch (HttpPostSizeTooLargeException hpstlex)
                    {
                        Log.Error(hpstlex, "HTTP POST size is too large.");

                        notification.ShowNotification(LocalizedInfo.MessageHttpPostSizeTooLarge, Notification.NotificationLevel.Error);
                    }
                }
            }

            return new ServiceInfo(this, tunnelInfo, validitySeconds);
        }

        /// <summary>
        /// Stops the service.
        /// </summary>
        public async Task Stop()
        {
            try
            {
                this.cancellationTokenSource?.Cancel();
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Error on cancel.");
            }
            finally
            {
                this.cancellationTokenSource?.Dispose();
                this.cancellationTokenSource = null;
            }

            if (this.soundId != null)
            {
                await this.zokmaApi?.DeleteSound(this.soundId, CancellationToken.None);
                this.zokmaApi = null;
            }

            if (this.IsRunning)
            {
                this.IsRunning = false;

            }
        }
    }
}
