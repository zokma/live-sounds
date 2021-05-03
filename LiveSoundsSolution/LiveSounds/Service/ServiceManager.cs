using LiveSounds.Api;
using LiveSounds.Audio;
using LiveSounds.Localization;
using LiveSounds.Ngrok;
using LiveSounds.Notification;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
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
        /// Audio renderings resouce.
        /// </summary>
        private const string AUDIO_RENDERINGS_RESOURCE = "/audio/renderings";

        /// <summary>
        /// Audio renderings path pattern.
        /// </summary>
        private const string AUDIO_RENDERINGS_PATH_PATTERN = "/{0}" + AUDIO_RENDERINGS_RESOURCE;

        /// <summary>
        /// Listen url pattern.
        /// </summary>
        private const string LISTEN_URL_PATTERN = "http://127.0.0.1:{0}/{1}/";

        /// <summary>
        /// Valid url pattern.
        /// </summary>
        private const string VALID_URL_PATTERN = "/{0}" + AUDIO_RENDERINGS_RESOURCE;

        /// <summary>
        /// Random generator.
        /// </summary>
        private static readonly RNGCryptoServiceProvider random = new RNGCryptoServiceProvider();

        /// <summary>
        /// UTF-8 Encoding.
        /// </summary>
        private static readonly Encoding UTF8 = new UTF8Encoding(false);

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
        /// Notification Manager.
        /// </summary>
        private NotificationManager notification;

        /// <summary>
        /// ZokmaApi.
        /// </summary>
        private ZokmaApi zokmaApi;

        /// <summary>
        /// Audio manager.
        /// </summary>
        private AudioManager audioManager;

        /// <summary>
        /// Sound id.
        /// </summary>
        private string soundId;

        /// <summary>
        /// Secret.
        /// </summary>
        private string secretString;

        /// <summary>
        /// HTTP Listener.
        /// </summary>
        private HttpListener listener;

        /// <summary>
        /// Creates ServiceManager.
        /// </summary>
        /// <param name="notification">Notification manager.</param>
        public ServiceManager(NotificationManager notification)
        {
            this.notification = notification;

            this.IsRunning = false;
        }

        /// <summary>
        /// Listens.
        /// </summary>
        /// <param name="listener">HTTP listener.</param>
        /// <param name="validPath">Valid path.</param>
        /// <returns>Task.</returns>
        private async Task Listen(HttpListener listener, string validPath)
        {
            Log.Information("Start HTTP Listen: ThreadId = {ThreadId}", Thread.CurrentThread.ManagedThreadId);

            try
            {
                while (listener.IsListening)
                {
                    var context = await listener.GetContextAsync();

                    try
                    {
                        var request  = context.Request;
                        var response = context.Response;

                        var status = HttpStatusCode.Forbidden;

                        try
                        {
                            if (request.RawUrl == validPath && 
                                request.HttpMethod == HttpMethod.Post.Method && 
                                request.ContentType.StartsWith("application/json") && 
                                request.ContentLength64 <= AppSettings.HTTP_LISTENER_LIMIT_REQUESTED_BYTES)
                            {
                                using var input  = request.InputStream;
                                using var reader = new StreamReader(input, UTF8);

                                string body = reader.ReadToEnd();

                                if(Log.IsVerboseEnabled)
                                {
                                    Log.Verbose("HTTP Listener received: body = {body}", body);
                                }

                                var audioRenerings = JsonSerializer.Deserialize<AudioRendering>(body, AppSettings.JsonSerializerOptionsForHttpRead);
                            }
                        }
                        finally
                        {
                            response.StatusCode = (int)status;
                            response.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error on processing requested content.");
                    }
                }
            }
            catch (HttpListenerException hlex)
            {
                if(hlex.ErrorCode != 995)
                {
                    Log.Error(hlex, "Unexpected Error on listening context.");
                }
            }

            Log.Information("Exit HTTP Listen: ThreadId = {ThreadId}", Thread.CurrentThread.ManagedThreadId);
        }

        /// <summary>
        /// Starts listening.
        /// </summary>
        /// <param name="port">Listening port.</param>
        /// <param name="guid">Listening Guid.</param>
        private void StartListener(int port, string guid)
        {
            this.listener = new HttpListener();

            string address   = String.Format(LISTEN_URL_PATTERN, port, guid);
            string validPath = String.Format(VALID_URL_PATTERN, guid);

            this.listener.Prefixes.Add(address);

            this.listener.Start();

            int threads = App.Settings.HttpListenerThreads;

            for (int i = 0; i < threads; i++)
            {
                Task.Run(
                    async () => {
                           await Listen(this.listener, validPath);
                        }
                    );
            }
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

            var settings = App.Settings;

            this.cancellationTokenSource = new CancellationTokenSource();
            this.cancellationToken       = this.cancellationTokenSource.Token;

            var tunnelInfo = await NgrokManager.FindTunnel(settings.NgrokApiPort, this.cancellationToken);

            int validitySeconds = 0;

            if(tunnelInfo == null)
            {
                this.notification.ShowNotification(LocalizedInfo.MessageNoNgrokDetected, Notification.NotificationLevel.Error);
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
                    this.zokmaApi = new ZokmaApi(settings.Token);

                    var secret = new byte[SECRET_LENGTH];
                    random.GetBytes(secret);

                    this.secretString = Convert.ToBase64String(secret, Base64FormattingOptions.None);

                    string guid = Guid.NewGuid().ToString("N");

                    try
                    {
                        var sound = await zokmaApi.CreateSound(
                                                (tunnelInfo.PublicUrl + String.Format(AUDIO_RENDERINGS_PATH_PATTERN, guid)),
                                                this.audioManager.AudioItems.ToArray(),
                                                this.secretString,
                                                settings.ServiceValiditySeconds,
                                                null, null,
                                                this.cancellationToken);

                        if (sound != null)
                        {
                            this.soundId    = sound.Id;
                            validitySeconds = sound.ValiditySeconds;

                            StartListener(tunnelInfo.ForwardingInfo.Port, guid);

                            this.IsRunning = true;
                        }
                    }
                    catch (AuthenticationException)
                    {
                        this.notification.ShowNotification(LocalizedInfo.MessageTokenInvalidError, Notification.NotificationLevel.Error);
                    }
                    catch (HttpRequestException hre)
                    {
                        Log.Warning(hre, "Error on creating sound.");
                    }
                    catch (HttpPostSizeTooLargeException hpstlex)
                    {
                        Log.Error(hpstlex, "HTTP POST size is too large.");

                        this.notification.ShowNotification(LocalizedInfo.MessageHttpPostSizeTooLarge, Notification.NotificationLevel.Error);
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
                }

                this.listener?.Close();

                if (this.soundId != null)
                {
                    try
                    {
                        await this.zokmaApi?.DeleteSound(this.soundId, CancellationToken.None);
                    }
                    catch(Exception ex)
                    {
                        Log.Warning(ex, "Error on deleting sound.");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Error on stopping service.");
            }
            finally
            {
                this.cancellationTokenSource = null;
                this.listener = null;
                this.zokmaApi = null;

                this.IsRunning = false;
            }
        }
    }
}
