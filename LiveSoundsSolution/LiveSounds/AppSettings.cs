using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
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
        /// Is data directory portable.
        /// </summary>
        [JsonInclude]
        public bool IsPortable = true;

        /// <summary>
        /// Log level name.
        /// </summary>
        [JsonInclude]
        public string LogLevelName { get; private set; } = "None";

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
        /// Parses enum.
        /// </summary>
        /// <typeparam name="TEnum">Enum type.</typeparam>
        /// <param name="name">The enum value string.</param>
        /// <param name="defaultValue">The default value if failed to parse.</param>
        /// <returns>The parsed enum value, or default value.</returns>
        public static TEnum ParseEnum<TEnum>(string name, TEnum defaultValue)
            where TEnum : struct
        {
            if (Enum.TryParse<TEnum>(name, true, out TEnum result))
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
            using (var writer = new Utf8JsonWriter(fs, new JsonWriterOptions { Indented = true }))
            {
                JsonSerializer.Serialize<AppSettings>(writer, this, JsonSerializerOptionsForFileWrite);
            }
        }
    }
}
