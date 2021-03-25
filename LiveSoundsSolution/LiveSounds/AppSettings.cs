using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Zokma.Libs.Logging;

namespace LiveSounds
{
    internal class AppSettings
    {

        /// <summary>
        /// JsonSerializerOptions for file read.
        /// </summary>
        public static readonly JsonSerializerOptions JsonSerializerOptionsForFileRead = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            PropertyNameCaseInsensitive = true,
        };

        /// <summary>
        /// JsonSerializerOptions for file write.
        /// </summary>
        public static readonly JsonSerializerOptions JsonSerializerOptionsForFileWrite = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        /// <summary>
        /// JsonSerializerOptions for http read.
        /// </summary>
        public static readonly JsonSerializerOptions JsonSerializerOptionsForHttpRead = new JsonSerializerOptions
        {
            AllowTrailingCommas = false,
            PropertyNameCaseInsensitive = false,
        };

        /// <summary>
        /// JsonSerializerOptions for http write.
        /// </summary>
        public static readonly JsonSerializerOptions JsonSerializerOptionsForHttpWrite = new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };


        /// <summary>
        /// JsonWriterOptions for file write.
        /// </summary>
        public static readonly JsonWriterOptions JsonWriterOptionForFile = new JsonWriterOptions
        {
            Indented = true,
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
        public string RenderModeName { get; internal set; } = null;

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
