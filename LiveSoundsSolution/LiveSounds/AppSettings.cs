using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        /// <summary>
        /// Play audio limits min.
        /// </summary>
        public const int PLAY_AUDIO_LIMITS_MIN = 0;

        /// <summary>
        /// Play audio limits max.
        /// </summary>
        public const int PLAY_AUDIO_LIMITS_MAX = 999;

        /// <summary>
        /// Play audio limits per app default.
        /// </summary>
        public const int PLAY_AUDIO_LIMITS_PER_APP_DEFAULT = 30;

        /// <summary>
        /// Play audio limits per user default.
        /// </summary>
        public const int PLAY_AUDIO_LIMITS_PER_USER_DEFAULT = 5;

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
            PropertyNameCaseInsensitive = false,
            Encoder = JSON_ENCODER,
        };

        /// <summary>
        /// JsonSerializerOptions for http write.
        /// </summary>
        public static readonly JsonSerializerOptions JsonSerializerOptionsForHttpWrite = new JsonSerializerOptions
        {
            WriteIndented = false,
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
        public int PlayAudioLimitsPerMinutePerAppNumber = 20;

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
                this.PlayAudioLimitsPerMinutePerAppNumber = GetPlayAudioLimits(value);
            }
        }

        /// <summary>
        /// Play Audio limits per minute per user.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("PlayAudioLimitsPerMinutePerUser")]
        public int PlayAudioLimitsPerMinutePerUserNumber = 5;

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
                this.PlayAudioLimitsPerMinutePerUserNumber = GetPlayAudioLimits(value);
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

            if (!String.IsNullOrWhiteSpace(this.LogLevelName))
            {
                this.LogLevelName = this.LogLevel.ToString();
            }

            using (var fs     = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read))
            using (var writer = new Utf8JsonWriter(fs, JsonWriterOptionForFile))
            {
                JsonSerializer.Serialize<AppSettings>(writer, this, JsonSerializerOptionsForFileWrite);
            }
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
