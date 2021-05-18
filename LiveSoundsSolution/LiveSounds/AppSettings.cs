using LiveSounds.Localization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Zokma.Libs.Audio;
using Zokma.Libs.Logging;

namespace LiveSounds
{
    internal class AppSettings
    {

#if   LOCAL_TEST_ENV

        /// <summary>
        /// URI for API service.
        /// </summary>
        public const string ZOKMA_API_URI = "http://localhost";

        /// <summary>
        /// Web app base URI.
        /// </summary>
        private const string ZOKMA_WEB_APP_BASE_URI = "http://localhost";

        /// <summary>
        /// URI Scheme.
        /// </summary>
        public const string ZOKMA_URI_STARTS_WITH = "http://";

#elif SANDBOX_ENV

        /// <summary>
        /// URI for API service.
        /// </summary>
        public const string ZOKMA_API_URI = "https://api-sandbox.zokma.net";

        /// <summary>
        /// Web app base URI.
        /// </summary>
        private const string ZOKMA_WEB_APP_BASE_URI = "https://sandbox.zokma.net";

        /// <summary>
        /// URI Scheme.
        /// </summary>
        public const string ZOKMA_URI_STARTS_WITH = "https://";

#else

        /// <summary>
        /// URI for API service.
        /// </summary>
        public const string ZOKMA_API_URI = "https://api.zokma.net";

        /// <summary>
        /// Web app base URI.
        /// </summary>
        private const string ZOKMA_WEB_APP_BASE_URI = "https://karakuri.zokma.net";

        /// <summary>
        /// URI Scheme.
        /// </summary>
        public const string ZOKMA_URI_STARTS_WITH = "https://";

#endif

        /// <summary>
        /// Web app uri pattern.
        /// </summary>
        public const string ZOKMA_WEB_APP_URI_PATTERN = ZOKMA_WEB_APP_BASE_URI + "/livesounds/{0}";

        /// <summary>
        /// Audio sample rate min.
        /// </summary>
        public const int AUDIO_SAMPLE_RATE_MIN = 8000;

        /// <summary>
        /// Audio sample rate max.
        /// </summary>
        public const int AUDIO_SAMPLE_RATE_MAX = 5644800;

        /// <summary>
        /// Audio sample rate default.
        /// </summary>
        public const int AUDIO_SAMPLE_RATE_DEFAULT = 44100;

        /// <summary>
        /// Audio channels min.
        /// </summary>
        public const int AUDIO_CHANNELS_MIN = 1;

        /// <summary>
        /// Audio channels max.
        /// </summary>
        //public const int AUDIO_CHANNELS_MAX = 12;
        public const int AUDIO_CHANNELS_MAX = 2;

        /// <summary>
        /// Audio channels default.
        /// </summary>
        public const int AUDIO_CHANNELS_DEFAULT = 2;

        /// <summary>
        /// Play audio limits min.
        /// </summary>
        public const int PLAY_AUDIO_LIMITS_MIN = 0;

        /// <summary>
        /// Play audio limits max.
        /// </summary>
        public const int PLAY_AUDIO_LIMITS_MAX = 256;

        /// <summary>
        /// Play audio limits per app default.
        /// </summary>
        public const int PLAY_AUDIO_LIMITS_PER_APP_DEFAULT = 30;

        /// <summary>
        /// Play audio limits per user default.
        /// </summary>
        public const int PLAY_AUDIO_LIMITS_PER_USER_DEFAULT = 5;

        /// <summary>
        /// Network port min.
        /// </summary>
        public const int NETWORK_PORT_MIN = 1024;

        /// <summary>
        /// Network port max.
        /// </summary>
        public const int NETWORK_PORT_MAX = UInt16.MaxValue;

        /// <summary>
        /// Network port default.
        /// </summary>
        public const int LOCAL_PORT_DEFAULT = 8780;

        /// <summary>
        /// Ngrok api port default.
        /// </summary>
        public const int NGROK_API_PORT_DEFAULT = 4040;

        /// <summary>
        /// Items limit min.
        /// </summary>
        public const int ITEMS_LIMIT_MIN = 1;

        /// <summary>
        /// Items limit max.
        /// </summary>
        public const int ITEMS_LIMIT_MAX = 256;

        /// <summary>
        /// Items limit max.
        /// </summary>
        public const int ITEMS_LIMIT_DEFAULT = 64;

        /// <summary>
        /// Http Threads min.
        /// </summary>
        public const int HTTP_LISTENER_THREDS_MIN = 1;

        /// <summary>
        /// Http Threads max.
        /// </summary>
        public const int HTTP_LISTENER_THREDS_MAX = 64;

        /// <summary>
        /// Http Threads default.
        /// </summary>
        public const int HTTP_LISTENER_THREDS_DEFAULT = 4;

        /// <summary>
        /// Http post limits bytes min.
        /// </summary>
        public const int HTTP_POST_LIMIT_BYTES_MIN = 64 * 1024;

        /// <summary>
        /// Http post limits bytes max.
        /// </summary>
        public const int HTTP_POST_LIMIT_BYTES_MAX = 10 * 1024 * 1024;

        /// <summary>
        /// Http post limits bytes default.
        /// </summary>
        public const int HTTP_POST_LIMIT_BYTES_DEFAULT = 1 * 1024 * 1024;

        /// <summary>
        /// Http Listener requested bytes limit.
        /// </summary>
        public const int HTTP_LISTENER_LIMIT_REQUESTED_BYTES = 64 * 1024;

        /// <summary>
        /// Service Validity seconds min.
        /// </summary>
        public const int SERVICE_VALIDITY_SECONDS_MIN = 1 * 60 * 60;

        /// <summary>
        /// Service Validity seconds max.
        /// </summary>
        public const int SERVICE_VALIDITY_SECONDS_MAX = 4 * 24 * 60 * 60;

        /// <summary>
        /// Service Validity seconds default.
        /// </summary>
        public const int SERVICE_VALIDITY_SECONDS_DEFAULT = 1 * 24 * 60 * 60;

        /// <summary>
        /// Regex timeout normal in millisecounds.
        /// </summary>
        public const int REGEX_TIMEOUT_NORMAL_IN_MILLISECONDS = 1500;

        /// <summary>
        /// Json encoder.
        /// </summary>
        private static readonly JavaScriptEncoder JSON_ENCODER = JavaScriptEncoder.Create(UnicodeRanges.All);

        /// <summary>
        /// JsonSerializerOptions for file read.
        /// </summary>
        public static readonly JsonSerializerOptions JsonSerializerOptionsForFileRead = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            PropertyNameCaseInsensitive = true,
            Encoder = JSON_ENCODER,
        };

        /// <summary>
        /// JsonSerializerOptions for file write.
        /// </summary>
        public static readonly JsonSerializerOptions JsonSerializerOptionsForFileWrite = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = JSON_ENCODER,
        };

        /// <summary>
        /// JsonSerializerOptions for http read.
        /// </summary>
        public static readonly JsonSerializerOptions JsonSerializerOptionsForHttpRead = new JsonSerializerOptions
        {
            AllowTrailingCommas = false,
            PropertyNamingPolicy        = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = false,
            Encoder = JSON_ENCODER,
        };

        /// <summary>
        /// JsonSerializerOptions for http write.
        /// </summary>
        public static readonly JsonSerializerOptions JsonSerializerOptionsForHttpWrite = new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy   = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = JSON_ENCODER,
        };


        /// <summary>
        /// JsonWriterOptions for file write.
        /// </summary>
        public static readonly JsonWriterOptions JsonWriterOptionForFile = new JsonWriterOptions
        {
            Indented = true,
            Encoder  = JSON_ENCODER,
        };

        /// <summary>
        /// HTTP Connection timeout.
        /// </summary>
        public static readonly TimeSpan HTTP_CONNECTION_TIMEOUT = TimeSpan.FromSeconds(15.0f);

        /// <summary>
        /// HTTP Client timeout.
        /// </summary>
        public static readonly TimeSpan HTTP_CLIENT_TIMEOUT = TimeSpan.FromSeconds(90.0f);


        /// <summary>
        /// Internal ETP.
        /// It will be better that the value is generated conditionally and located on far location.
        /// But, in this time, it will be reasonable against purpose.
        /// </summary>
        private static readonly byte[] ETP = { 0x4d, 0xa4, 0x10, 0xa9, 0x2c, 0xe2, 0x8d, 0x7c, 0xe4, 0xfb, 0x44, 0xaf, 0x09, 0x52, 0x68, 0x50, };


        /// <summary>
        /// Audio Render Volume.
        /// </summary>
        [JsonInclude]
        public int AudioRenderVolume = 100;

        /// <summary>
        /// true if the audio render is muted.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("AudioRenderMuted")]
        public bool IsAudioRenderMuted = false;

        /// <summary>
        /// Play Audio limits per minute per app.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("PlayAudioLimitsPerMinutePerApp")]
        public int PlayAudioLimitsPerMinutePerAppNumber = PLAY_AUDIO_LIMITS_PER_APP_DEFAULT;

        /// <summary>
        /// Play Audio limits per minute per app.
        /// </summary>
        [JsonIgnore]
        public int PlayAudioLimitsPerMinutePerApp 
        {
            get
            {
                return GetPlayAudioLimits(this.PlayAudioLimitsPerMinutePerAppNumber);
            }
            set
            {
                this.PlayAudioLimitsPerMinutePerAppNumber = value;
            }
        }

        /// <summary>
        /// Play Audio limits per minute per user.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("PlayAudioLimitsPerMinutePerUser")]
        public int PlayAudioLimitsPerMinutePerUserNumber = PLAY_AUDIO_LIMITS_PER_USER_DEFAULT;

        /// <summary>
        /// Play Audio limits per minute per user.
        /// </summary>
        [JsonIgnore]
        public int PlayAudioLimitsPerMinutePerUser
        {
            get
            {
                return GetPlayAudioLimits(this.PlayAudioLimitsPerMinutePerUserNumber);
            }
            set
            {
                this.PlayAudioLimitsPerMinutePerUserNumber = value;
            }
        }

        /// <summary>
        /// Audio sample rate.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("AudioSampleRate")]
        public int AudioSampleRateNumber { get; private set; } = AUDIO_SAMPLE_RATE_DEFAULT;

        /// <summary>
        /// Audio sample rate.
        /// </summary>
        [JsonIgnore]
        public int AudioSampleRate
        {
            get
            {
                return Math.Max(Math.Min(this.AudioSampleRateNumber, AUDIO_SAMPLE_RATE_MAX), AUDIO_SAMPLE_RATE_MIN);
            }
        }

        /// <summary>
        /// Audio channels.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("AudioChannels")]
        public int AudioChannelsNumber { get; private set; } = AUDIO_CHANNELS_DEFAULT;

        /// <summary>
        /// Audio channels.
        /// </summary>
        [JsonIgnore]
        public int AudioChannels
        {
            get
            {
                return Math.Max(Math.Min(this.AudioChannelsNumber, AUDIO_CHANNELS_MAX), AUDIO_CHANNELS_MIN);
            }
        }

        /// <summary>
        /// Audio Wave Format.
        /// </summary>
        [JsonIgnore]
        public WaveFormat AudioWaveFormat
        {
            get
            {
                return new WaveFormat(this.AudioSampleRate, this.AudioChannels);
            }
        }

        /// <summary>
        /// Audio render device type name.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("AudioRenderDeviceType")]
        public string AudioRenderDeviceTypeName { get; private set; } = null;

        /// <summary>
        /// Audio render device type.
        /// </summary>
        [JsonIgnore]
        public AudioDeviceType AudioRenderDeviceType
        {
            get
            {
                return ParseEnum(this.AudioRenderDeviceTypeName, AudioDeviceType.WASAPI);
            }

            set
            {
                this.AudioRenderDeviceTypeName = value.ToString();
            }
        }

        /// <summary>
        /// Audio render device role name.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("AudioRenderDeviceRole")]
        public string AudioRenderDeviceRoleName { get; private set; } = null;

        /// <summary>
        /// Audio render device role.
        /// </summary>
        [JsonIgnore]
        public AudioDeviceRole AudioRenderDeviceRole
        {
            get
            {
                return ParseEnum(this.AudioRenderDeviceRoleName, AudioDeviceRole.Multimedia);
            }

            set
            {
                this.AudioRenderDeviceRoleName = value.ToString();
            }
        }

        /// <summary>
        /// Share mode for Audio Render Engine.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("AudioRenderEngineShareMode")]
        public string AudioRenderEngineShareModeName { get; private set; } = null;

        /// <summary>
        /// Share mode for Audio Render Engine.
        /// </summary>
        [JsonIgnore]
        public AudioEngineShareMode AudioRenderEngineShareMode
        {
            get
            {
                return ParseEnum(this.AudioRenderEngineShareModeName, AudioEngineShareMode.Shared);
            }
            set
            {
                this.AudioRenderEngineShareModeName = value.ToString();
            }
        }


        /// <summary>
        /// Audio latency.
        /// </summary>
        [JsonInclude]
        public int AudioRenderLatency = 200;

        /// <summary>
        /// Audio resampling quality;
        /// </summary>
        [JsonInclude]
        public int AudioResamplingQuality = 60;

        /// <summary>
        /// Audio Render device id.
        /// </summary>
        [JsonInclude]
        public string AudioRenderDeviceId;

        /// <summary>
        /// Data Preset id.
        /// </summary>
        [JsonInclude]
        public string DataPresetId;


        /// <summary>
        /// Local port number.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("LocalPort")]
        public int LocalPortNumber = LOCAL_PORT_DEFAULT;

        /// <summary>
        /// Local port number.
        /// </summary>
        [JsonIgnore]
        public int LocalPort
        {
            get
            {
                return GetNetworkPort(this.LocalPortNumber);
            }
            set
            {
                this.LocalPortNumber = value;
            }
        }

        /// <summary>
        /// Items limit.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("ItemsLimit")]
        public int ItemsLimitNumber { get; private set; } = ITEMS_LIMIT_DEFAULT;

        /// <summary>
        /// Items limit.
        /// </summary>
        [JsonIgnore]
        public int ItemsLimit
        {
            get
            {
                return Math.Max(Math.Min(this.ItemsLimitNumber, ITEMS_LIMIT_MAX), ITEMS_LIMIT_MIN);
            }
        }

        /// <summary>
        /// HTTP Listener threads.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("HttpListenerThreads")]
        public int HttpListenerThreadsNumber { get; private set; } = HTTP_LISTENER_THREDS_DEFAULT;

        /// <summary>
        /// HTTP Listener threads.
        /// </summary>
        [JsonIgnore]
        public int HttpListenerThreads
        {
            get
            {
                return Math.Max(Math.Min(this.HttpListenerThreadsNumber, HTTP_LISTENER_THREDS_MAX), HTTP_LISTENER_THREDS_MIN);
            }
        }

        /// <summary>
        /// HTTP POST size limit in bytes.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("HttpPostSizeLimitBytes")]
        public int HttpPostSizeLimitBytesNumber { get; private set; } = HTTP_POST_LIMIT_BYTES_DEFAULT;

        /// <summary>
        /// HTTP POST size limit in bytes.
        /// </summary>
        [JsonIgnore]
        public int HttpPostSizeLimitBytes
        {
            get
            {
                return Math.Max(Math.Min(this.HttpPostSizeLimitBytesNumber, HTTP_POST_LIMIT_BYTES_MAX), HTTP_POST_LIMIT_BYTES_MIN);
            }
        }

        /// <summary>
        /// Validity seconds.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("ServiceValiditySeconds")]
        public int ServiceValiditySecondsNumber { get; private set; } = SERVICE_VALIDITY_SECONDS_DEFAULT;

        /// <summary>
        /// Validity seconds.
        /// </summary>
        [JsonIgnore]
        public int ServiceValiditySeconds
        {
            get
            {
                return Math.Max(Math.Min(this.ServiceValiditySecondsNumber, SERVICE_VALIDITY_SECONDS_MAX), SERVICE_VALIDITY_SECONDS_MIN);
            }
        }

        /// <summary>
        /// Encrypted Token.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("Token")]
        public string EncryptedToken { get; set; }

        /// <summary>
        /// Checks if Encrypted token info is set.
        /// </summary>
        [JsonIgnore]
        public bool HasEncryptedToken
        {
            get
            {
                return !(String.IsNullOrWhiteSpace(this.EncryptedToken));
            }
        }

        /// <summary>
        /// Token.
        /// </summary>
        [JsonIgnore]
        public string Token
        {
            get
            {
                string result = String.Empty;

                if (!String.IsNullOrWhiteSpace(this.EncryptedToken))
                {
                    try
                    {
                        var data = Convert.FromBase64String(this.EncryptedToken);

                        data = ProtectedData.Unprotect(data, ETP, this.DataProtectionScope);

                        var utf8 = new UTF8Encoding(false);

                        result = utf8.GetString(data);
                    }
                    catch (FormatException ex)
                    {
                        Log.Error(ex, "Base64 string has invalid format.");
                    }
                    catch (CryptographicException ex)
                    {
                        Log.Error(ex, "Unprotect failed.");
                    }
                    catch (ArgumentException ex)
                    {
                        Log.Error(ex, "UTF8 Decode failed.");
                    }
                }

                return result;
            }

            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    var utf8 = new UTF8Encoding(false);

                    var data = utf8.GetBytes(value);

                    data = ProtectedData.Protect(data, ETP, this.DataProtectionScope);

                    this.EncryptedToken = Convert.ToBase64String(data, Base64FormattingOptions.None);
                }
                else
                {
                    this.EncryptedToken = null;
                }
            }
        }

        /// <summary>
        /// DataProtectionScope.
        /// </summary>
        [JsonIgnore]
        private DataProtectionScope DataProtectionScope
        {
            get
            {
                return (this.IsPortable ? DataProtectionScope.LocalMachine : DataProtectionScope.CurrentUser); 
            }
        }

        /// <summary>
        /// Is data directory portable.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("Portable")]
        public bool IsPortable = true;

        /// <summary>
        /// Log level name.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("LogLevel")]
#if DEBUG
        public string LogLevelName { get; private set; } = "Debug";
#else
        public string LogLevelName { get; private set; } = "None";
#endif

        /// <summary>
        /// Log file size limit in bytes.
        /// </summary>
        [JsonInclude]
        public long LogFileSizeLimitBytes = 14L * 1024 * 1024;

        /// <summary>
        /// Log file count limit.
        /// </summary>
        [JsonInclude]
        public int LogFileCountLimit = 4;

        /// <summary>
        /// Log output buffered.
        /// </summary>
        [JsonInclude]
        public bool LogBuffered = false;

        /// <summary>
        /// Log level.
        /// </summary>
        [JsonIgnore]
        public LogLevel LogLevel
        {
            get
            {
                return ParseEnum(LogLevelName, Zokma.Libs.Logging.LogLevel.None);
            }

            set
            {
                LogLevelName = value.ToString();
            }
        }

        /// <summary>
        /// Notification max.
        /// </summary>
        [JsonInclude]
        public int NotificationMax = 5;

        /// <summary>
        /// History max.
        /// </summary>
        [JsonInclude]
#if DEBUG
        public int HistoryMax = 100;
#else
        public int HistoryMax = 10;
#endif
        /// <summary>
        /// Window Width.
        /// </summary>
        [JsonInclude]
        public int WindowWidth = 0;

        /// <summary>
        /// Window Height.
        /// </summary>
        [JsonInclude]
        public int WindowHeight = 0;

        /// <summary>
        /// Window Startup location name.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("WindowStartupLocation")]
        public string WindowStartupLocationName { get; private set; } = null;

        /// <summary>
        /// Windows Startup location.
        /// </summary>
        [JsonIgnore]
        public WindowStartupLocation WindowStartupLocation
        {
            get
            {
                return ParseEnum(this.WindowStartupLocationName, WindowStartupLocation.Manual);
            }

            set
            {
                this.WindowStartupLocationName = value.ToString();
            }
        }

        /// <summary>
        /// Window Style name.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("WindowStyle")]
        public string WindowStyleName { get; internal set; } = null;

        /// <summary>
        /// Window Style.
        /// </summary>
        [JsonIgnore]
        public WindowStyle WindowStyle
        {
            get
            {
                return ParseEnum(this.WindowStyleName, WindowStyle.None);
            }

            set
            {
                this.WindowStyleName = value.ToString();
            }
        }

        /// <summary>
        /// Render mode name.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("RenderMode")]
        public string RenderModeName { get; private set; } = null;

        /// <summary>
        /// Render mode name.
        /// </summary>
        [JsonIgnore]
        public RenderMode RenderMode
        {
            get
            {
                return ParseEnum(this.RenderModeName, RenderMode.Default);
            }

            set
            {
                this.RenderModeName = value.ToString();
            }
        }

        /// <summary>
        /// Ngrok region name.
        /// </summary>
        [JsonInclude]
        public string NgrokRegion = LocalizedInfo.NgrokRegion;

        /// <summary>
        /// Port for Ngrok api.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("NgrokApiPort")]
        public int? NgrokApiPortNumber = null;

        /// <summary>
        /// Port for Ngrok api.
        /// </summary>
        [JsonIgnore]
        public int NgrokApiPort {
            get
            {
                int? portNumber = this.NgrokApiPortNumber;

                int port = NGROK_API_PORT_DEFAULT;

                if(portNumber.HasValue)
                {
                    port = GetNetworkPort(portNumber.Value);
                }

                return port;
            }
        }

        /// <summary>
        /// Settings file path.
        /// </summary>
        [JsonIgnore]
        public string FilePath;

        /// <summary>
        /// true if the settings has file.
        /// </summary>
        [JsonIgnore]
        public bool HasFile 
        {
            get
            {
                return !String.IsNullOrWhiteSpace(this.FilePath);
            }
        }

        /// <summary>
        /// Parses enum.
        /// </summary>
        /// <typeparam name="TEnum">Enum type.</typeparam>
        /// <param name="name">The enum value string.</param>
        /// <param name="defaultValue">The default value if failed to parse.</param>
        /// <returns>The parsed enum value, or default value.</returns>
        public static TEnum ParseEnum<TEnum>(string name, TEnum defaultValue)
            where TEnum : struct
        {
            if (!String.IsNullOrWhiteSpace(name) && Enum.TryParse<TEnum>(name, true, out TEnum result))
            {
                return result;
            }

            return defaultValue;
        }

        /// <summary>
        /// Gets play audio limits.
        /// </summary>
        /// <param name="times">Play audio limits.</param>
        /// <returns>Adjusted play audio limits.</returns>
        public static int GetPlayAudioLimits(int times)
        {
            return Math.Max(Math.Min(times, PLAY_AUDIO_LIMITS_MAX), PLAY_AUDIO_LIMITS_MIN);
        }

        /// <summary>
        /// Gets play audio limits from string.
        /// </summary>
        /// <param name="times">Play audio limits string.</param>
        /// <param name="defaultLimits">Default limits.</param>
        /// <returns>Adjusted play audio limits.</returns>
        public static int GetPlayAudioLimits(string times, int defaultLimits)
        {
            int limits;

            if(!Int32.TryParse(times, out limits))
            {
                limits = defaultLimits;
            }

            return GetPlayAudioLimits(limits);
        }

        /// <summary>
        /// Gets Network port number.
        /// </summary>
        /// <param name="port">Network port.</param>
        /// <returns>Adjusted Network port.</returns>
        public static int GetNetworkPort(int port)
        {
            return Math.Max(Math.Min(port, NETWORK_PORT_MAX), NETWORK_PORT_MIN);
        }

        /// <summary>
        /// Gets Network port number from string.
        /// </summary>
        /// <param name="port">Network port.</param>
        /// <param name="defaultPort">Default port.</param>
        /// <returns>Adjusted Network port.</returns>
        public static int GetNetworkPort(string port, int defaultPort)
        {
            int portNumber;

            if (!Int32.TryParse(port, out portNumber))
            {
                portNumber = defaultPort;
            }

            return GetNetworkPort(portNumber);
        }

        /// <summary>
        /// Loads the app settings.
        /// </summary>
        /// <param name="filePath">Setting file path.</param>
        /// <returns>The app settings loaded.If failed to load, defautl settings are returned.</returns>
        public static AppSettings LoadAppSettings(string filePath)
        {
            AppSettings result;

            try
            {
                using (var fs     = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var reader = new StreamReader(fs, new UTF8Encoding(false), true))
                {
                    result = JsonSerializer.Deserialize<AppSettings>(reader.ReadToEnd(), JsonSerializerOptionsForFileRead);
                    
                    result.FilePath = fs.Name;
                }
            }
            catch
            {
                result = new AppSettings();
            }

            return result;
        }

        /// <summary>
        /// Saves the settings.
        /// </summary>
        /// <param name="filePath">The setting file path to be saved.</param>
        public void Save(string filePath)
        {
            if (!String.IsNullOrWhiteSpace(this.AudioRenderDeviceTypeName))
            {
                this.AudioRenderDeviceTypeName = this.AudioRenderDeviceType.ToString();
            }
            if (!String.IsNullOrWhiteSpace(this.AudioRenderDeviceRoleName))
            {
                this.AudioRenderDeviceRoleName = this.AudioRenderDeviceRole.ToString();
            }
            if (!String.IsNullOrWhiteSpace(this.AudioRenderEngineShareModeName))
            {
                this.AudioRenderEngineShareModeName = this.AudioRenderEngineShareMode.ToString();
            }

            if (!String.IsNullOrWhiteSpace(this.LogLevelName))
            {
                this.LogLevelName = this.LogLevel.ToString();
            }

            using (var fs     = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read))
            using (var writer = new Utf8JsonWriter(fs, JsonWriterOptionForFile))
            {
                JsonSerializer.Serialize<AppSettings>(writer, this, JsonSerializerOptionsForFileWrite);
            }

            Log.Information("AppSettings are saved to file.");
        }

        /// <summary>
        /// Saves the settings.
        /// </summary>
        public void Save()
        {
            if(String.IsNullOrWhiteSpace(this.FilePath))
            {
                throw new InvalidOperationException("Settings file path to be saved is undefined.");
            }

            this.Save(this.FilePath);
        }
    }
}
