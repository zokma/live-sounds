using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zokma.Libs.Logging
{
    /// <summary>
    /// Static Log class for logging to file.
    /// This class uses Serilog(https://serilog.net/).
    /// </summary>
    public static class Log
    {
        /// <summary>
        /// Min log file size.
        /// </summary>
        private const long MIN_FILE_SIZE = 1L * 1024;

        /// <summary>
        /// Max log file size.
        /// </summary>
        private const long MAX_FILE_SIZE = 1L * 1024 * 1024 * 1024;

        /// <summary>
        /// Min log file count.
        /// </summary>
        private const int MIN_FILE_COUNT = 1;

        /// <summary>
        /// Max log file count.
        /// </summary>
        private const int MAX_FILE_COUNT = 1024;

        /// <summary>
        /// Default log file size.
        /// </summary>
        private const long DEFAULT_FILE_SIZE = 14L * 1024 * 1024;

        /// <summary>
        /// Default log file count.
        /// </summary>
        private const int DEFAULT_FILE_COUNT = 4;


        /// <summary>
        /// Default logger.
        /// Expects SilentLogger(internal access), but may be updated somewhere.
        /// </summary>
        private static readonly Serilog.ILogger DefaultLogger = Serilog.Log.Logger;

        /// <summary>
        /// Internal logger.
        /// </summary>
        private static Serilog.ILogger logger = DefaultLogger;

        /// <summary>
        /// Log level.
        /// </summary>
        private static LogLevel level = LogLevel.Information;

        /// <summary>
        /// Log level switcher.
        /// </summary>
        private static readonly Serilog.Core.LoggingLevelSwitch levelSwitch = new Serilog.Core.LoggingLevelSwitch(Serilog.Events.LogEventLevel.Information);


        /// <summary>
        /// Log Level to be output.
        /// </summary>
        public static LogLevel LogLevel 
        {
            get
            {
                return level;
            }

            set
            {
                level = value;
                levelSwitch.MinimumLevel = GetLogEventLevel(value);
            }
        }

        /// <summary>
        /// Gets LogEventLevel from LogLevel.
        /// </summary>
        /// <param name="level">LogLevel.</param>
        /// <returns>LogEventLevel.</returns>
        private static Serilog.Events.LogEventLevel GetLogEventLevel(LogLevel level)
        {
            var result = level switch
            {
                LogLevel.Verbose     => Serilog.Events.LogEventLevel.Verbose,
                LogLevel.Debug       => Serilog.Events.LogEventLevel.Debug,
                LogLevel.Information => Serilog.Events.LogEventLevel.Information,
                LogLevel.Warning     => Serilog.Events.LogEventLevel.Warning,
                LogLevel.Error       => Serilog.Events.LogEventLevel.Error,
                LogLevel.Fatal       => Serilog.Events.LogEventLevel.Fatal,
                _                    => Serilog.Events.LogEventLevel.Information,
            };

            return result;
        }

        /// <summary>
        /// Initializes Logger.
        /// </summary>
        /// <param name="path">Log file path.</param>
        /// <param name="logLevel">Initila log level.</param>
        /// <param name="fileSizeBytesLimit">Log file size limit in bytes.</param>
        /// <param name="fileCountLimit">Log file count limit(rolling).</param>
        public static void Init(string path, LogLevel logLevel = LogLevel.Information, long fileSizeBytesLimit = DEFAULT_FILE_SIZE, int fileCountLimit = DEFAULT_FILE_COUNT)
        {
            var former = logger as IDisposable;

            logger = DefaultLogger;
            Serilog.Log.Logger = DefaultLogger;

            fileSizeBytesLimit = Math.Max(Math.Min(fileSizeBytesLimit, MAX_FILE_SIZE ), MIN_FILE_SIZE);
            fileCountLimit     = Math.Max(Math.Min(fileCountLimit,     MAX_FILE_COUNT), MIN_FILE_COUNT);

            if (former != DefaultLogger)
            {
                former?.Dispose();
            }

            var log = new LoggerConfiguration()
                .WriteTo.File(path, fileSizeLimitBytes: fileSizeBytesLimit, retainedFileCountLimit: fileCountLimit, rollOnFileSizeLimit: true, encoding: new UTF8Encoding(false))
                .MinimumLevel.ControlledBy(levelSwitch)
                .CreateLogger();

            LogLevel = logLevel;

            logger = log;
            Serilog.Log.Logger = log;
        }

        /// <summary>
        /// Closes this logger.
        /// </summary>
        public static void Close()
        {
            var former = logger as IDisposable;

            logger = DefaultLogger;
            Serilog.Log.Logger = DefaultLogger;

            if (former != DefaultLogger)
            {
                former?.Dispose();
            }
        }

        /// <summary>
        /// Checks if the log level is enabled.
        /// </summary>
        /// <param name="level">LogLevel to be checked.</param>
        /// <returns>The log level is enabled.</returns>
        public static bool IsEnabled(LogLevel level)
        {
            bool result = level switch
            {
                LogLevel.Verbose     => logger.IsEnabled(Serilog.Events.LogEventLevel.Verbose),
                LogLevel.Debug       => logger.IsEnabled(Serilog.Events.LogEventLevel.Debug),
                LogLevel.Information => logger.IsEnabled(Serilog.Events.LogEventLevel.Information),
                LogLevel.Warning     => logger.IsEnabled(Serilog.Events.LogEventLevel.Warning),
                LogLevel.Error       => logger.IsEnabled(Serilog.Events.LogEventLevel.Error),
                LogLevel.Fatal       => logger.IsEnabled(Serilog.Events.LogEventLevel.Fatal),
                _                    => false,
            };

            return result;
        }

        #region WriteVerbose

        /// <summary>
        /// Writes a Verbose level log.
        /// </summary>
        /// <param name="message">Log message.</param>
        public static void Verbose(string message)
        {
            logger.Verbose(message);
        }

        /// <summary>
        /// Writes a Verbose level log.
        /// </summary>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue">The property value for the message template.</param>
        public static void Verbose<T>(string messageTemplate, T propertyValue)
        {
            logger.Verbose(messageTemplate, propertyValue);
        }

        /// <summary>
        /// Writes a Verbose level log.
        /// </summary>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue0">The property value for the message template.</param>
        /// <param name="propertyValue1">The property value for the message template.</param>
        public static void Verbose<T0, T1>(string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
            logger.Verbose(messageTemplate, propertyValue0, propertyValue1);
        }

        /// <summary>
        /// Writes a Verbose level log.
        /// </summary>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue0">The property value for the message template.</param>
        /// <param name="propertyValue1">The property value for the message template.</param>
        /// <param name="propertyValue2">The property value for the message template.</param>
        public static void Verbose<T0, T1, T2>(string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        {
            logger.Verbose(messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        }

        /// <summary>
        /// Writes a Verbose level log.
        /// </summary>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValues">The property values for the message template.</param>
        public static void Verbose(string messageTemplate, params object[] propertyValues)
        {
            logger.Verbose(messageTemplate, propertyValues);
        }

        /// <summary>
        /// Writes a Verbose level log.
        /// </summary>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="message">Log message.</param>
        public static void Verbose(Exception exception, string message)
        {
            logger.Verbose(exception, message);
        }

        /// <summary>
        /// Writes a Verbose level log.
        /// </summary>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue">The property value for the message template.</param>
        public static void Verbose<T>(Exception exception, string messageTemplate, T propertyValue)
        {
            logger.Verbose(exception, messageTemplate, propertyValue);
        }

        /// <summary>
        /// Writes a Verbose level log.
        /// </summary>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue0">The property value for the message template.</param>
        /// <param name="propertyValue1">The property value for the message template.</param>
        public static void Verbose<T0, T1>(Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
            logger.Verbose(exception, messageTemplate, propertyValue0, propertyValue1);
        }

        /// <summary>
        /// Writes a Verbose level log.
        /// </summary>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue0">The property value for the message template.</param>
        /// <param name="propertyValue1">The property value for the message template.</param>
        /// <param name="propertyValue2">The property value for the message template.</param>
        public static void Verbose<T0, T1, T2>(Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        {
            logger.Verbose(exception, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        }

        /// <summary>
        /// Writes a Verbose level log.
        /// </summary>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValues">The property values for the message template.</param>
        public static void Verbose(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            logger.Verbose(exception, messageTemplate, propertyValues);
        }

        #endregion

        #region WriteDebug

        /// <summary>
        /// Writes a Debug level log.
        /// </summary>
        /// <param name="message">Log message.</param>
        public static void Debug(string message)
        {
            logger.Debug(message);
        }

        /// <summary>
        /// Writes a Debug level log.
        /// </summary>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue">The property value for the message template.</param>
        public static void Debug<T>(string messageTemplate, T propertyValue)
        {
            logger.Debug(messageTemplate, propertyValue);
        }

        /// <summary>
        /// Writes a Debug level log.
        /// </summary>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue0">The property value for the message template.</param>
        /// <param name="propertyValue1">The property value for the message template.</param>
        public static void Debug<T0, T1>(string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
            logger.Debug(messageTemplate, propertyValue0, propertyValue1);
        }

        /// <summary>
        /// Writes a Debug level log.
        /// </summary>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue0">The property value for the message template.</param>
        /// <param name="propertyValue1">The property value for the message template.</param>
        /// <param name="propertyValue2">The property value for the message template.</param>
        public static void Debug<T0, T1, T2>(string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        {
            logger.Debug(messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        }

        /// <summary>
        /// Writes a Debug level log.
        /// </summary>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValues">The property values for the message template.</param>
        public static void Debug(string messageTemplate, params object[] propertyValues)
        {
            logger.Debug(messageTemplate, propertyValues);
        }

        /// <summary>
        /// Writes a Debug level log.
        /// </summary>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="message">Log message.</param>
        public static void Debug(Exception exception, string message)
        {
            logger.Debug(exception, message);
        }

        /// <summary>
        /// Writes a Debug level log.
        /// </summary>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue">The property value for the message template.</param>
        public static void Debug<T>(Exception exception, string messageTemplate, T propertyValue)
        {
            logger.Debug(exception, messageTemplate, propertyValue);
        }

        /// <summary>
        /// Writes a Debug level log.
        /// </summary>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue0">The property value for the message template.</param>
        /// <param name="propertyValue1">The property value for the message template.</param>
        public static void Debug<T0, T1>(Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
            logger.Debug(exception, messageTemplate, propertyValue0, propertyValue1);
        }

        /// <summary>
        /// Writes a Debug level log.
        /// </summary>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue0">The property value for the message template.</param>
        /// <param name="propertyValue1">The property value for the message template.</param>
        /// <param name="propertyValue2">The property value for the message template.</param>
        public static void Debug<T0, T1, T2>(Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        {
            logger.Debug(exception, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        }

        /// <summary>
        /// Writes a Debug level log.
        /// </summary>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValues">The property values for the message template.</param>
        public static void Debug(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            logger.Debug(exception, messageTemplate, propertyValues);
        }

        #endregion

        #region WriteInformation

        /// <summary>
        /// Writes an Information level log.
        /// </summary>
        /// <param name="message">Log message.</param>
        public static void Information(string message)
        {
            logger.Information(message);
        }

        /// <summary>
        /// Writes an Information level log.
        /// </summary>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue">The property value for the message template.</param>
        public static void Information<T>(string messageTemplate, T propertyValue)
        {
            logger.Information(messageTemplate, propertyValue);
        }

        /// <summary>
        /// Writes an Information level log.
        /// </summary>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue0">The property value for the message template.</param>
        /// <param name="propertyValue1">The property value for the message template.</param>
        public static void Information<T0, T1>(string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
            logger.Information(messageTemplate, propertyValue0, propertyValue1);
        }

        /// <summary>
        /// Writes an Information level log.
        /// </summary>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue0">The property value for the message template.</param>
        /// <param name="propertyValue1">The property value for the message template.</param>
        /// <param name="propertyValue2">The property value for the message template.</param>
        public static void Information<T0, T1, T2>(string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        {
            logger.Information(messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        }

        /// <summary>
        /// Writes an Information level log.
        /// </summary>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValues">The property values for the message template.</param>
        public static void Information(string messageTemplate, params object[] propertyValues)
        {
            logger.Information(messageTemplate, propertyValues);
        }

        /// <summary>
        /// Writes an Information level log.
        /// </summary>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="message">Log message.</param>
        public static void Information(Exception exception, string message)
        {
            logger.Information(exception, message);
        }

        /// <summary>
        /// Writes an Information level log.
        /// </summary>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue">The property value for the message template.</param>
        public static void Information<T>(Exception exception, string messageTemplate, T propertyValue)
        {
            logger.Information(exception, messageTemplate, propertyValue);
        }

        /// <summary>
        /// Writes an Information level log.
        /// </summary>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue0">The property value for the message template.</param>
        /// <param name="propertyValue1">The property value for the message template.</param>
        public static void Information<T0, T1>(Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
            logger.Information(exception, messageTemplate, propertyValue0, propertyValue1);
        }

        /// <summary>
        /// Writes an Information level log.
        /// </summary>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue0">The property value for the message template.</param>
        /// <param name="propertyValue1">The property value for the message template.</param>
        /// <param name="propertyValue2">The property value for the message template.</param>
        public static void Information<T0, T1, T2>(Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        {
            logger.Information(exception, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        }

        /// <summary>
        /// Writes an Information level log.
        /// </summary>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValues">The property values for the message template.</param>
        public static void Information(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            logger.Information(exception, messageTemplate, propertyValues);
        }

        #endregion

        #region WriteWarning

        /// <summary>
        /// Writes a Warning level log.
        /// </summary>
        /// <param name="message">Log message.</param>
        public static void Warning(string message)
        {
            logger.Warning(message);
        }

        /// <summary>
        /// Writes a Warning level log.
        /// </summary>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue">The property value for the message template.</param>
        public static void Warning<T>(string messageTemplate, T propertyValue)
        {
            logger.Warning(messageTemplate, propertyValue);
        }

        /// <summary>
        /// Writes a Warning level log.
        /// </summary>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue0">The property value for the message template.</param>
        /// <param name="propertyValue1">The property value for the message template.</param>
        public static void Warning<T0, T1>(string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
            logger.Warning(messageTemplate, propertyValue0, propertyValue1);
        }

        /// <summary>
        /// Writes a Warning level log.
        /// </summary>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue0">The property value for the message template.</param>
        /// <param name="propertyValue1">The property value for the message template.</param>
        /// <param name="propertyValue2">The property value for the message template.</param>
        public static void Warning<T0, T1, T2>(string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        {
            logger.Warning(messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        }

        /// <summary>
        /// Writes a Warning level log.
        /// </summary>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValues">The property values for the message template.</param>
        public static void Warning(string messageTemplate, params object[] propertyValues)
        {
            logger.Warning(messageTemplate, propertyValues);
        }

        /// <summary>
        /// Writes a Warning level log.
        /// </summary>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="message">Log message.</param>
        public static void Warning(Exception exception, string message)
        {
            logger.Warning(exception, message);
        }

        /// <summary>
        /// Writes a Warning level log.
        /// </summary>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue">The property value for the message template.</param>
        public static void Warning<T>(Exception exception, string messageTemplate, T propertyValue)
        {
            logger.Warning(exception, messageTemplate, propertyValue);
        }

        /// <summary>
        /// Writes a Warning level log.
        /// </summary>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue0">The property value for the message template.</param>
        /// <param name="propertyValue1">The property value for the message template.</param>
        public static void Warning<T0, T1>(Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
            logger.Warning(exception, messageTemplate, propertyValue0, propertyValue1);
        }

        /// <summary>
        /// Writes a Warning level log.
        /// </summary>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue0">The property value for the message template.</param>
        /// <param name="propertyValue1">The property value for the message template.</param>
        /// <param name="propertyValue2">The property value for the message template.</param>
        public static void Warning<T0, T1, T2>(Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        {
            logger.Warning(exception, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        }

        /// <summary>
        /// Writes a Warning level log.
        /// </summary>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValues">The property values for the message template.</param>
        public static void Warning(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            logger.Warning(exception, messageTemplate, propertyValues);
        }

        #endregion

        #region WriteError

        /// <summary>
        /// Writes an Error level log.
        /// </summary>
        /// <param name="message">Log message.</param>
        public static void Error(string message)
        {
            logger.Error(message);
        }

        /// <summary>
        /// Writes an Error level log.
        /// </summary>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue">The property value for the message template.</param>
        public static void Error<T>(string messageTemplate, T propertyValue)
        {
            logger.Error(messageTemplate, propertyValue);
        }

        /// <summary>
        /// Writes an Error level log.
        /// </summary>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue0">The property value for the message template.</param>
        /// <param name="propertyValue1">The property value for the message template.</param>
        public static void Error<T0, T1>(string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
            logger.Error(messageTemplate, propertyValue0, propertyValue1);
        }

        /// <summary>
        /// Writes an Error level log.
        /// </summary>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue0">The property value for the message template.</param>
        /// <param name="propertyValue1">The property value for the message template.</param>
        /// <param name="propertyValue2">The property value for the message template.</param>
        public static void Error<T0, T1, T2>(string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        {
            logger.Error(messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        }

        /// <summary>
        /// Writes an Error level log.
        /// </summary>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValues">The property values for the message template.</param>
        public static void Error(string messageTemplate, params object[] propertyValues)
        {
            logger.Error(messageTemplate, propertyValues);
        }

        /// <summary>
        /// Writes an Error level log.
        /// </summary>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="message">Log message.</param>
        public static void Error(Exception exception, string message)
        {
            logger.Error(exception, message);
        }

        /// <summary>
        /// Writes an Error level log.
        /// </summary>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue">The property value for the message template.</param>
        public static void Error<T>(Exception exception, string messageTemplate, T propertyValue)
        {
            logger.Error(exception, messageTemplate, propertyValue);
        }

        /// <summary>
        /// Writes an Error level log.
        /// </summary>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue0">The property value for the message template.</param>
        /// <param name="propertyValue1">The property value for the message template.</param>
        public static void Error<T0, T1>(Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
            logger.Error(exception, messageTemplate, propertyValue0, propertyValue1);
        }

        /// <summary>
        /// Writes an Error level log.
        /// </summary>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue0">The property value for the message template.</param>
        /// <param name="propertyValue1">The property value for the message template.</param>
        /// <param name="propertyValue2">The property value for the message template.</param>
        public static void Error<T0, T1, T2>(Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        {
            logger.Error(exception, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        }

        /// <summary>
        /// Writes an Error level log.
        /// </summary>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValues">The property values for the message template.</param>
        public static void Error(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            logger.Error(exception, messageTemplate, propertyValues);
        }

        #endregion

        #region WriteFatal

        /// <summary>
        /// Writes a Fatal level log.
        /// </summary>
        /// <param name="message">Log message.</param>
        public static void Fatal(string message)
        {
            logger.Fatal(message);
        }

        /// <summary>
        /// Writes a Fatal level log.
        /// </summary>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue">The property value for the message template.</param>
        public static void Fatal<T>(string messageTemplate, T propertyValue)
        {
            logger.Fatal(messageTemplate, propertyValue);
        }

        /// <summary>
        /// Writes a Fatal level log.
        /// </summary>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue0">The property value for the message template.</param>
        /// <param name="propertyValue1">The property value for the message template.</param>
        public static void Fatal<T0, T1>(string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
            logger.Fatal(messageTemplate, propertyValue0, propertyValue1);
        }

        /// <summary>
        /// Writes a Fatal level log.
        /// </summary>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue0">The property value for the message template.</param>
        /// <param name="propertyValue1">The property value for the message template.</param>
        /// <param name="propertyValue2">The property value for the message template.</param>
        public static void Fatal<T0, T1, T2>(string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        {
            logger.Fatal(messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        }

        /// <summary>
        /// Writes a Fatal level log.
        /// </summary>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValues">The property values for the message template.</param>
        public static void Fatal(string messageTemplate, params object[] propertyValues)
        {
            logger.Fatal(messageTemplate, propertyValues);
        }

        /// <summary>
        /// Writes a Fatal level log.
        /// </summary>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="message">Log message.</param>
        public static void Fatal(Exception exception, string message)
        {
            logger.Fatal(exception, message);
        }

        /// <summary>
        /// Writes a Fatal level log.
        /// </summary>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue">The property value for the message template.</param>
        public static void Fatal<T>(Exception exception, string messageTemplate, T propertyValue)
        {
            logger.Fatal(exception, messageTemplate, propertyValue);
        }

        /// <summary>
        /// Writes a Fatal level log.
        /// </summary>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue0">The property value for the message template.</param>
        /// <param name="propertyValue1">The property value for the message template.</param>
        public static void Fatal<T0, T1>(Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
            logger.Fatal(exception, messageTemplate, propertyValue0, propertyValue1);
        }

        /// <summary>
        /// Writes a Fatal level log.
        /// </summary>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue0">The property value for the message template.</param>
        /// <param name="propertyValue1">The property value for the message template.</param>
        /// <param name="propertyValue2">The property value for the message template.</param>
        public static void Fatal<T0, T1, T2>(Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        {
            logger.Fatal(exception, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        }

        /// <summary>
        /// Writes a Fatal level log.
        /// </summary>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValues">The property values for the message template.</param>
        public static void Fatal(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            logger.Fatal(exception, messageTemplate, propertyValues);
        }

        #endregion
    }
}
