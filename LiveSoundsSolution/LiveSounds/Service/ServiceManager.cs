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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Zokma.Libs.Audio;
using Zokma.Libs.Logging;

namespace LiveSounds.Service
{
    /// <summary>
    /// Service Manager.
    /// </summary>
    internal class ServiceManager : IDisposable
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
        /// Regex to capture stream id.
        /// For now, only YouTube is supported.
        /// </summary>
        private const string REGEX_TO_CAPTURE_STREAM_ID = @"^https://(youtu\.be/|www\.youtube\.com/(embed/|watch\?(v=|.*&v=)))(?<STREAMID>[0-9a-zA-Z_-]{10,24})($|&|\?)";

        /// <summary>
        /// Regex to capture stream id.
        /// For now, only YouTube is supported.
        /// This regex is compiled and cached as static field for reuse.
        /// However, it may be called a few times and may be better to instanciate before using.
        /// For now, compiled and cached are adopted.
        /// </summary>
        public static readonly Regex RegexToCaptureStreamId = new Regex(REGEX_TO_CAPTURE_STREAM_ID, RegexOptions.Compiled, AppSettings.REGEX_TIMEOUT_NORMAL);

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
        /// Audio Player.
        /// </summary>
        private AudioPlayer audioPlayer;

        /// <summary>
        /// RateLimit manager.
        /// </summary>
        private RateLimitManager rateLimitManager;

        /// <summary>
        /// Ngrok manager.
        /// </summary>
        private NgrokManager ngrokManager;

        /// <summary>
        /// Play Audio Limit per application.
        /// </summary>
        public int PlayAudioLimitsPerApp 
        {
            get
            {
                return this.rateLimitManager.GlobalLimit;
            }

            set
            {
                this.rateLimitManager.GlobalLimit = value;
            }
        }

        /// <summary>
        /// Play Audio Limit per user.
        /// </summary>
        public int PlayAudioLimitsPerUser
        {
            get
            {
                return this.rateLimitManager.UserLimit;
            }

            set
            {
                this.rateLimitManager.UserLimit = value;
            }
        }

        /// <summary>
        /// Sound id.
        /// </summary>
        public string SoundId { get; private set; }

        /// <summary>
        /// Secret.
        /// </summary>
        private string secretString;

        /// <summary>
        /// Dispatcher for events.
        /// </summary>
        private Dispatcher dispatcher;

        /// <summary>
        /// Auto close timer.
        /// </summary>
        private DispatcherTimer autoCloseTimer;

        /// <summary>
        /// Auto close action.
        /// </summary>
        private Action autoCloseAction;

        /// <summary>
        /// HTTP Listener.
        /// </summary>
        private HttpListener listener;
        private bool disposedValue;

        /// <summary>
        /// Creates ServiceManager.
        /// </summary>
        /// <param name="notification">Notification manager.</param>
        /// <param name="ngrokApiPort">Ngrok Api port.</param>
        /// <param name="playAudioLimitsPerApp">Play Audio Limit per application.</param>
        /// <param name="playAudioLimitsPerUser">Play Audio Limit per user.</param>
        /// <param name="dispatcher">Dispather.</param>
        /// <param name="autoCloseAction">Action for auto close.</param>
        public ServiceManager(NotificationManager notification, int ngrokApiPort, int playAudioLimitsPerApp, int playAudioLimitsPerUser, Dispatcher dispatcher, Action autoCloseAction)
        {
            this.notification     = notification;
            this.ngrokManager     = new NgrokManager(ngrokApiPort);
            this.rateLimitManager = new RateLimitManager()
            {
                GlobalLimit = playAudioLimitsPerApp,
                UserLimit   = playAudioLimitsPerUser,
            };

            this.dispatcher      = dispatcher;
            this.autoCloseAction = autoCloseAction;

            this.IsRunning = false;
        }

        /// <summary>
        /// Listens.
        /// </summary>
        /// <param name="listener">HTTP listener.</param>
        /// <param name="validPath">Valid path.</param>
        /// <param name="counter">Counter for HttpListener.</param>
        /// <returns>Task.</returns>
        private async Task Listen(HttpListener listener, string validPath, HttpListenerCounter counter)
        {
            Log.Information("Start HTTP Listen: ThreadId = {ThreadId}", Thread.CurrentThread.ManagedThreadId);

            try
            {
                counter.ReportListenerStarted();

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
                            int retryAfter = this.rateLimitManager.CheckGlobalRetryAfterSecounds();

                            if (retryAfter <= 0 &&
                                request.RawUrl == validPath && 
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

                                if(audioRenerings.SoundId == this.SoundId && 
                                   audioRenerings.Secret  == this.secretString &&
                                   !String.IsNullOrWhiteSpace(audioRenerings.Id) &&
                                   !String.IsNullOrWhiteSpace(audioRenerings.UserHash))
                                {
                                    retryAfter = this.rateLimitManager.CheckUserRetryAfterSecounds(audioRenerings.UserHash);

                                    if (retryAfter <= 0)
                                    {
                                        AudioData audioData;

                                        if(this.audioManager.TryGetAudioData(audioRenerings.Id, out audioData))
                                        {
                                            this.audioPlayer?.Play(audioData, PlaybackMode.Once, audioRenerings.Volume);

                                            status = HttpStatusCode.NoContent;
                                        }
                                        else
                                        {
                                            status = HttpStatusCode.NotFound;
                                        }
                                    }
                                }
                            }

                            if (retryAfter > 0)
                            {
                                status = HttpStatusCode.TooManyRequests;
                                response.Headers.Add("Retry-After", retryAfter.ToString());
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "Error on processing requested content.");

                            status = HttpStatusCode.InternalServerError;
                        }
                        finally
                        {
                            response.StatusCode = (int)status;
                            response.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Unexpected Error on receiving request.");
                    }
                }
            }
            catch (ObjectDisposedException ode)
            {
                Log.Error(ode, "Error on closing listener.");
            }
            catch (HttpListenerException hlex)
            {
                if(hlex.ErrorCode != 995)
                {
                    Log.Error(hlex, "Unexpected Error on listening context.");
                }
            }
            finally
            {
                if (listener.IsListening)
                {
                    counter.ReportListenerTerminatedUnexpectedly();
                }
                else
                {
                    counter.ReportListenerStopped();
                }

                if(!counter.IsAlive)
                {
                    Log.Error("Service will be stopped automatically.");

                    this.notification.ShowNotification(LocalizedInfo.MessageStopServiceWithUnexpectedError, NotificationLevel.Error);

                    this.autoCloseAction();
                }

                Log.Information("Exit HTTP Listen: ThreadId = {ThreadId}", Thread.CurrentThread.ManagedThreadId);
            }
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

            this.rateLimitManager.Reset();


            var counter = new HttpListenerCounter(threads);

            for (int i = 0; i < threads; i++)
            {
                Task.Run(
                    async () => {
                           await Listen(this.listener, validPath, counter);
                        }
                    );
            }
        }


        /// <summary>
        /// Gets StreamingId from Live Url.
        /// </summary>
        /// <param name="liveUrl">Stream Url.</param>
        /// <returns>StreamingId.</returns>
        private static string GetStreamingId(string liveUrl)
        {
            if (String.IsNullOrWhiteSpace(liveUrl))
            {
                return null;
            }

            string result = null;

            try
            {
                var match = RegexToCaptureStreamId.Match(liveUrl);

                if (match.Success)
                {
                    result = match.Groups["STREAMID"].Value;
                }
            }
            catch (RegexMatchTimeoutException rmte)
            {
                Log.Error(rmte, "Error on parsing Live Stream Url.");
            }

            return result;
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

            string streamingId = null;

            if(!String.IsNullOrWhiteSpace(config.LiveUrl))
            {
                streamingId = GetStreamingId(config.LiveUrl.Trim());

                if(streamingId == null)
                {
                    this.notification.ShowNotification(LocalizedInfo.MessageLiveUrlNotSupportedWarning, Notification.NotificationLevel.Warn);
                }
            }

            var settings = App.Settings;

            this.cancellationTokenSource = new CancellationTokenSource();
            this.cancellationToken       = this.cancellationTokenSource.Token;

            var tunnelInfo = await this.ngrokManager.FindTunnel(config.ForwardingPort.Value, settings.NgrokRegion, this.cancellationToken);

            TimeSpan validityPeriod = TimeSpan.Zero;

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
                                                null, 
                                                streamingId,
                                                this.cancellationToken);

                        if (sound != null)
                        {
                            this.audioPlayer = config.AudioPlayer;
                            this.SoundId     = sound.Id;
                            validityPeriod   = TimeSpan.FromSeconds(sound.ValiditySeconds);

                            this.autoCloseTimer = new DispatcherTimer(DispatcherPriority.Background, this.dispatcher)
                            {
                                Interval = validityPeriod,
                            };
                            this.autoCloseTimer.Tick += AutoCloseTimer_Tick;
                            this.autoCloseTimer.Start();

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

                        this.notification.ShowNotification(LocalizedInfo.MessageHttpRequestFailed, Notification.NotificationLevel.Error);
                    }
                    catch(JsonException jex)
                    {
                        Log.Warning(jex, "Error on creating sound.");
                    }
                    catch (HttpPostSizeTooLargeException hpstlex)
                    {
                        Log.Error(hpstlex, "HTTP POST size is too large.");

                        this.notification.ShowNotification(LocalizedInfo.MessageHttpPostSizeTooLarge, Notification.NotificationLevel.Error);
                    }
                }
            }

            return new ServiceInfo(this, tunnelInfo, validityPeriod);
        }

        private void AutoCloseTimer_Tick(object sender, EventArgs e)
        {
            this.autoCloseTimer?.Stop();

            if (this.autoCloseAction != null)
            {
                this.autoCloseAction();
            }
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

                this.audioManager?.Dispose();

                this.autoCloseTimer?.Stop();

                if (this.zokmaApi != null && this.SoundId != null)
                {
                    try
                    {
                        await this.zokmaApi?.DeleteSound(this.SoundId, CancellationToken.None);
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
                this.listener       = null;
                this.audioManager   = null;
                this.autoCloseTimer = null;
                this.zokmaApi       = null;

                this.IsRunning = false;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.ngrokManager?.Dispose();
                    this.ngrokManager = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~ServiceManager()
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
