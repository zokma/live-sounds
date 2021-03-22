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
        /// </summary>
        private static readonly Serilog.ILogger DefaultLogger = EmptyLogger.Instance;

        /// <summary>
        /// Internal logger.
        /// </summary>
        private static Serilog.ILogger logger = DefaultLogger;

        /// <summary>
        /// Log level.
        /// </summary>
        private static LogLevel level = LogLevel.None;

        /// <summary>
        /// Log level switcher.
        /// </summary>
        private static readonly Serilog.Core.LoggingLevelSwitch levelSwitch = new Serilog.Core.LoggingLevelSwitch(Serilog.Events.LogEventLevel.Warning);

        /// <summary>
        /// Whether the logger ignores exception or not.
        /// </summary>
        private static bool isExceptionIgnored = true;

        /// <summary>
        /// Whether the logger is closed after exception thrown.
        /// </summary>
        private static bool isClosedAfterException = true;

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
                levelSwitch.MinimumLevel = GetLogEventLevel(value);
                level = value;
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
                _                    => Serilog.Events.LogEventLevel.Fatal,
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
        /// <param name="isBuffered">Whether the logger output is buffered or not.</param>
        /// <param name="isExceptionIgnored">Whether the logger ignores exception or not.</param>
        /// <param name="isClosedAfterException">Whether the logger is closed after exception thrown.</param>
        public static void Init(string path, LogLevel logLevel = LogLevel.Warning, long fileSizeBytesLimit = DEFAULT_FILE_SIZE, int fileCountLimit = DEFAULT_FILE_COUNT, bool isBuffered = false, bool isExceptionIgnored = true, bool isClosedAfterException = true)
        {
            try
            {
                Log.isExceptionIgnored     = isExceptionIgnored;
                Log.isClosedAfterException = isClosedAfterException;

                var former = Log.logger;

                Log.logger         = DefaultLogger;
                Serilog.Log.Logger = DefaultLogger;

                Log.LogLevel = logLevel;

                if (former != DefaultLogger)
                {
                    (former as IDisposable)?.Dispose();
                }

                if(logLevel == LogLevel.None || fileSizeBytesLimit <= 0 || fileCountLimit <= 0)
                {
                    return;
                }

                fileSizeBytesLimit = Math.Max(Math.Min(fileSizeBytesLimit, MAX_FILE_SIZE), MIN_FILE_SIZE);
                fileCountLimit     = Math.Max(Math.Min(fileCountLimit, MAX_FILE_COUNT), MIN_FILE_COUNT);

                var log = new LoggerConfiguration()
                    .WriteTo.File(path, fileSizeLimitBytes: fileSizeBytesLimit, retainedFileCountLimit: fileCountLimit, rollOnFileSizeLimit: true, buffered: isBuffered, encoding: new UTF8Encoding(false))
                    .MinimumLevel.ControlledBy(levelSwitch)
                    .CreateLogger();

                Log.logger         = log;
                Serilog.Log.Logger = log;
            }
            catch(Exception)
            {
                if (!isExceptionIgnored)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Closes this logger.
        /// </summary>
        public static void Close()
        {
            try
            {
                LogLevel = LogLevel.None;

                var former = logger;

                logger = DefaultLogger;
                Serilog.Log.Logger = DefaultLogger;

                if (former != DefaultLogger)
                {
                    (former as IDisposable)?.Dispose();
                }
            }
            catch(Exception)
            {
                if (!isExceptionIgnored)
                {
                    throw;
                }
            }
        }

        #region IsEnabled

        /// <summary>
        /// true if Verbose level log enabled to output.
        /// </summary>
        public static bool IsVerboseEnabled => IsEnabled(LogLevel.Verbose);

        /// <summary>
        /// true if Debug level log enabled to output.
        /// </summary>
        public static bool IsDebugEnabled => IsEnabled(LogLevel.Debug);

        /// <summary>
        /// true if Information level log enabled to output.
        /// </summary>
        public static bool IsInformationEnabled => IsEnabled(LogLevel.Information);

        /// <summary>
        /// true if Warning level log enabled to output.
        /// </summary>
        public static bool IsWarningEnabled => IsEnabled(LogLevel.Warning);

        /// <summary>
        /// true if Error level log enabled to output.
        /// </summary>
        public static bool IsErrorEnabled => IsEnabled(LogLevel.Error);

        /// <summary>
        /// true if Fatal level log enabled to output.
        /// </summary>
        public static bool IsFatalEnabled => IsEnabled(LogLevel.Fatal);


        /// <summary>
        /// Checks if the log level is enabled.
        /// </summary>
        /// <param name="level">LogLevel to be checked.</param>
        /// <returns>The log level is enabled.</returns>
        public static bool IsEnabled(LogLevel level)
        {
            return ((level < LogLevel.Silent) && (level >= Log.level));
        }

        #endregion

        #region WriteLog

        /// <summary>
        /// Writes a specified level log.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="message">Log message.</param>
        private static void Write(Serilog.Events.LogEventLevel level, string message)
        {
            try
            {
                if (Log.level < LogLevel.Silent)
                {
                    logger.Write(level, message);
                }
            }
            catch (Exception)
            {
                if(isClosedAfterException)
                {
                    Close();
                }

                if (!isExceptionIgnored)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Writes a specified level log.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue">The property value for the message template.</param>
        private static void Write<T>(Serilog.Events.LogEventLevel level, string messageTemplate, T propertyValue)
        {
            try
            {
                if (Log.level < LogLevel.Silent)
                {
                    logger.Write(level, messageTemplate, propertyValue);
                }
            }
            catch (Exception)
            {
                if (isClosedAfterException)
                {
                    Close();
                }

                if (!isExceptionIgnored)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Writes a specified level log.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue0">The property value for the message template.</param>
        /// <param name="propertyValue1">The property value for the message template.</param>
        private static void Write<T0, T1>(Serilog.Events.LogEventLevel level, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
            try
            {
                if (Log.level < LogLevel.Silent)
                {
                    logger.Write(level, messageTemplate, propertyValue0, propertyValue1);
                }
            }
            catch (Exception)
            {
                if (isClosedAfterException)
                {
                    Close();
                }

                if (!isExceptionIgnored)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Writes a specified level log.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue0">The property value for the message template.</param>
        /// <param name="propertyValue1">The property value for the message template.</param>
        /// <param name="propertyValue2">The property value for the message template.</param>
        private static void Write<T0, T1, T2>(Serilog.Events.LogEventLevel level, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        {
            try
            {
                if (Log.level < LogLevel.Silent)
                {
                    logger.Write(level, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
                }
            }
            catch (Exception)
            {
                if (isClosedAfterException)
                {
                    Close();
                }

                if (!isExceptionIgnored)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Writes a specified level log.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValues">The property values for the message template.</param>
        private static void Write(Serilog.Events.LogEventLevel level, string messageTemplate, params object[] propertyValues)
        {
            try
            {
                if (Log.level < LogLevel.Silent)
                {
                    logger.Write(level, messageTemplate, propertyValues);
                }
            }
            catch (Exception)
            {
                if (isClosedAfterException)
                {
                    Close();
                }

                if (!isExceptionIgnored)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Writes a specified level log.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="message">Log message.</param>
        private static void Write(Serilog.Events.LogEventLevel level, Exception exception, string message)
        {
            try
            {
                if (Log.level < LogLevel.Silent)
                {
                    logger.Write(level, exception, message);
                }
            }
            catch (Exception)
            {
                if (isClosedAfterException)
                {
                    Close();
                }

                if (!isExceptionIgnored)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Writes a specified level log.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue">The property value for the message template.</param>
        private static void Write<T>(Serilog.Events.LogEventLevel level, Exception exception, string messageTemplate, T propertyValue)
        {
            try
            {
                if (Log.level < LogLevel.Silent)
                {
                    logger.Write(level, exception, messageTemplate, propertyValue);
                }
            }
            catch (Exception)
            {
                if (isClosedAfterException)
                {
                    Close();
                }

                if (!isExceptionIgnored)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Writes a specified level log.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue0">The property value for the message template.</param>
        /// <param name="propertyValue1">The property value for the message template.</param>
        private static void Write<T0, T1>(Serilog.Events.LogEventLevel level, Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
            try
            {
                if (Log.level < LogLevel.Silent)
                {
                    logger.Write(level, exception, messageTemplate, propertyValue0, propertyValue1);
                }
            }
            catch (Exception)
            {
                if (isClosedAfterException)
                {
                    Close();
                }

                if (!isExceptionIgnored)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Writes a specified level log.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue0">The property value for the message template.</param>
        /// <param name="propertyValue1">The property value for the message template.</param>
        /// <param name="propertyValue2">The property value for the message template.</param>
        private static void Write<T0, T1, T2>(Serilog.Events.LogEventLevel level, Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        {
            try
            {
                if (Log.level < LogLevel.Silent)
                {
                    logger.Write(level, exception, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
                }
            }
            catch (Exception)
            {
                if (isClosedAfterException)
                {
                    Close();
                }

                if (!isExceptionIgnored)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Writes a specified level log.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValues">The property values for the message template.</param>
        private static void Write(Serilog.Events.LogEventLevel level, Exception exception, string messageTemplate, params object[] propertyValues)
        {
            try
            {
                if (Log.level < LogLevel.Silent)
                {
                    logger.Write(level, exception, messageTemplate, propertyValues);
                }
            }
            catch (Exception)
            {
                if (isClosedAfterException)
                {
                    Close();
                }

                if (!isExceptionIgnored)
                {
                    throw;
                }
            }
        }

        #endregion

        #region WriteVerbose

        /// <summary>
        /// Writes a Verbose level log.
        /// </summary>
        /// <param name="message">Log message.</param>
        public static void Verbose(string message)
        {
            Write(Serilog.Events.LogEventLevel.Verbose, message);
        }

        /// <summary>
        /// Writes a Verbose level log.
        /// </summary>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue">The property value for the message template.</param>
        public static void Verbose<T>(string messageTemplate, T propertyValue)
        {
            Write(Serilog.Events.LogEventLevel.Verbose, messageTemplate, propertyValue);
        }

        /// <summary>
        /// Writes a Verbose level log.
        /// </summary>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue0">The property value for the message template.</param>
        /// <param name="propertyValue1">The property value for the message template.</param>
        public static void Verbose<T0, T1>(string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
            Write(Serilog.Events.LogEventLevel.Verbose, messageTemplate, propertyValue0, propertyValue1);
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
            Write(Serilog.Events.LogEventLevel.Verbose, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        }

        /// <summary>
        /// Writes a Verbose level log.
        /// </summary>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValues">The property values for the message template.</param>
        public static void Verbose(string messageTemplate, params object[] propertyValues)
        {
            Write(Serilog.Events.LogEventLevel.Verbose, messageTemplate, propertyValues);
        }

        /// <summary>
        /// Writes a Verbose level log.
        /// </summary>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="message">Log message.</param>
        public static void Verbose(Exception exception, string message)
        {
            Write(Serilog.Events.LogEventLevel.Verbose, exception, message);
        }

        /// <summary>
        /// Writes a Verbose level log.
        /// </summary>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue">The property value for the message template.</param>
        public static void Verbose<T>(Exception exception, string messageTemplate, T propertyValue)
        {
            Write(Serilog.Events.LogEventLevel.Verbose, exception, messageTemplate, propertyValue);
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
            Write(Serilog.Events.LogEventLevel.Verbose, exception, messageTemplate, propertyValue0, propertyValue1);
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
            Write(Serilog.Events.LogEventLevel.Verbose, exception, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        }

        /// <summary>
        /// Writes a Verbose level log.
        /// </summary>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValues">The property values for the message template.</param>
        public static void Verbose(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Write(Serilog.Events.LogEventLevel.Verbose, exception, messageTemplate, propertyValues);
        }

        #endregion

        #region WriteDebug

        /// <summary>
        /// Writes a Debug level log.
        /// </summary>
        /// <param name="message">Log message.</param>
        public static void Debug(string message)
        {
            Write(Serilog.Events.LogEventLevel.Debug, message);
        }

        /// <summary>
        /// Writes a Debug level log.
        /// </summary>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue">The property value for the message template.</param>
        public static void Debug<T>(string messageTemplate, T propertyValue)
        {
            Write(Serilog.Events.LogEventLevel.Debug, messageTemplate, propertyValue);
        }

        /// <summary>
        /// Writes a Debug level log.
        /// </summary>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue0">The property value for the message template.</param>
        /// <param name="propertyValue1">The property value for the message template.</param>
        public static void Debug<T0, T1>(string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
            Write(Serilog.Events.LogEventLevel.Debug, messageTemplate, propertyValue0, propertyValue1);
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
            Write(Serilog.Events.LogEventLevel.Debug, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        }

        /// <summary>
        /// Writes a Debug level log.
        /// </summary>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValues">The property values for the message template.</param>
        public static void Debug(string messageTemplate, params object[] propertyValues)
        {
            Write(Serilog.Events.LogEventLevel.Debug, messageTemplate, propertyValues);
        }

        /// <summary>
        /// Writes a Debug level log.
        /// </summary>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="message">Log message.</param>
        public static void Debug(Exception exception, string message)
        {
            Write(Serilog.Events.LogEventLevel.Debug, exception, message);
        }

        /// <summary>
        /// Writes a Debug level log.
        /// </summary>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue">The property value for the message template.</param>
        public static void Debug<T>(Exception exception, string messageTemplate, T propertyValue)
        {
            Write(Serilog.Events.LogEventLevel.Debug, exception, messageTemplate, propertyValue);
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
            Write(Serilog.Events.LogEventLevel.Debug, exception, messageTemplate, propertyValue0, propertyValue1);
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
            Write(Serilog.Events.LogEventLevel.Debug, exception, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        }

        /// <summary>
        /// Writes a Debug level log.
        /// </summary>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValues">The property values for the message template.</param>
        public static void Debug(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Write(Serilog.Events.LogEventLevel.Debug, exception, messageTemplate, propertyValues);
        }

        #endregion

        #region WriteInformation

        /// <summary>
        /// Writes an Information level log.
        /// </summary>
        /// <param name="message">Log message.</param>
        public static void Information(string message)
        {
            Write(Serilog.Events.LogEventLevel.Information, message);
        }

        /// <summary>
        /// Writes an Information level log.
        /// </summary>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue">The property value for the message template.</param>
        public static void Information<T>(string messageTemplate, T propertyValue)
        {
            Write(Serilog.Events.LogEventLevel.Information, messageTemplate, propertyValue);
        }

        /// <summary>
        /// Writes an Information level log.
        /// </summary>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue0">The property value for the message template.</param>
        /// <param name="propertyValue1">The property value for the message template.</param>
        public static void Information<T0, T1>(string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
            Write(Serilog.Events.LogEventLevel.Information, messageTemplate, propertyValue0, propertyValue1);
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
            Write(Serilog.Events.LogEventLevel.Information, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        }

        /// <summary>
        /// Writes an Information level log.
        /// </summary>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValues">The property values for the message template.</param>
        public static void Information(string messageTemplate, params object[] propertyValues)
        {
            Write(Serilog.Events.LogEventLevel.Information, messageTemplate, propertyValues);
        }

        /// <summary>
        /// Writes an Information level log.
        /// </summary>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="message">Log message.</param>
        public static void Information(Exception exception, string message)
        {
            Write(Serilog.Events.LogEventLevel.Information, exception, message);
        }

        /// <summary>
        /// Writes an Information level log.
        /// </summary>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue">The property value for the message template.</param>
        public static void Information<T>(Exception exception, string messageTemplate, T propertyValue)
        {
            Write(Serilog.Events.LogEventLevel.Information, exception, messageTemplate, propertyValue);
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
            Write(Serilog.Events.LogEventLevel.Information, exception, messageTemplate, propertyValue0, propertyValue1);
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
            Write(Serilog.Events.LogEventLevel.Information, exception, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        }

        /// <summary>
        /// Writes an Information level log.
        /// </summary>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValues">The property values for the message template.</param>
        public static void Information(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Write(Serilog.Events.LogEventLevel.Information, exception, messageTemplate, propertyValues);
        }

        #endregion

        #region WriteWarning

        /// <summary>
        /// Writes a Warning level log.
        /// </summary>
        /// <param name="message">Log message.</param>
        public static void Warning(string message)
        {
            Write(Serilog.Events.LogEventLevel.Warning, message);
        }

        /// <summary>
        /// Writes a Warning level log.
        /// </summary>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue">The property value for the message template.</param>
        public static void Warning<T>(string messageTemplate, T propertyValue)
        {
            Write(Serilog.Events.LogEventLevel.Warning, messageTemplate, propertyValue);
        }

        /// <summary>
        /// Writes a Warning level log.
        /// </summary>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue0">The property value for the message template.</param>
        /// <param name="propertyValue1">The property value for the message template.</param>
        public static void Warning<T0, T1>(string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
            Write(Serilog.Events.LogEventLevel.Warning, messageTemplate, propertyValue0, propertyValue1);
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
            Write(Serilog.Events.LogEventLevel.Warning, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        }

        /// <summary>
        /// Writes a Warning level log.
        /// </summary>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValues">The property values for the message template.</param>
        public static void Warning(string messageTemplate, params object[] propertyValues)
        {
            Write(Serilog.Events.LogEventLevel.Warning, messageTemplate, propertyValues);
        }

        /// <summary>
        /// Writes a Warning level log.
        /// </summary>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="message">Log message.</param>
        public static void Warning(Exception exception, string message)
        {
            Write(Serilog.Events.LogEventLevel.Warning, exception, message);
        }

        /// <summary>
        /// Writes a Warning level log.
        /// </summary>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue">The property value for the message template.</param>
        public static void Warning<T>(Exception exception, string messageTemplate, T propertyValue)
        {
            Write(Serilog.Events.LogEventLevel.Warning, exception, messageTemplate, propertyValue);
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
            Write(Serilog.Events.LogEventLevel.Warning, exception, messageTemplate, propertyValue0, propertyValue1);
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
            Write(Serilog.Events.LogEventLevel.Warning, exception, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        }

        /// <summary>
        /// Writes a Warning level log.
        /// </summary>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValues">The property values for the message template.</param>
        public static void Warning(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Write(Serilog.Events.LogEventLevel.Warning, exception, messageTemplate, propertyValues);
        }

        #endregion

        #region WriteError

        /// <summary>
        /// Writes an Error level log.
        /// </summary>
        /// <param name="message">Log message.</param>
        public static void Error(string message)
        {
            Write(Serilog.Events.LogEventLevel.Error, message);
        }

        /// <summary>
        /// Writes an Error level log.
        /// </summary>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue">The property value for the message template.</param>
        public static void Error<T>(string messageTemplate, T propertyValue)
        {
            Write(Serilog.Events.LogEventLevel.Error, messageTemplate, propertyValue);
        }

        /// <summary>
        /// Writes an Error level log.
        /// </summary>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue0">The property value for the message template.</param>
        /// <param name="propertyValue1">The property value for the message template.</param>
        public static void Error<T0, T1>(string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
            Write(Serilog.Events.LogEventLevel.Error, messageTemplate, propertyValue0, propertyValue1);
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
            Write(Serilog.Events.LogEventLevel.Error, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        }

        /// <summary>
        /// Writes an Error level log.
        /// </summary>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValues">The property values for the message template.</param>
        public static void Error(string messageTemplate, params object[] propertyValues)
        {
            Write(Serilog.Events.LogEventLevel.Error, messageTemplate, propertyValues);
        }

        /// <summary>
        /// Writes an Error level log.
        /// </summary>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="message">Log message.</param>
        public static void Error(Exception exception, string message)
        {
            Write(Serilog.Events.LogEventLevel.Error, exception, message);
        }

        /// <summary>
        /// Writes an Error level log.
        /// </summary>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue">The property value for the message template.</param>
        public static void Error<T>(Exception exception, string messageTemplate, T propertyValue)
        {
            Write(Serilog.Events.LogEventLevel.Error, exception, messageTemplate, propertyValue);
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
            Write(Serilog.Events.LogEventLevel.Error, exception, messageTemplate, propertyValue0, propertyValue1);
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
            Write(Serilog.Events.LogEventLevel.Error, exception, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        }

        /// <summary>
        /// Writes an Error level log.
        /// </summary>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValues">The property values for the message template.</param>
        public static void Error(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Write(Serilog.Events.LogEventLevel.Error, exception, messageTemplate, propertyValues);
        }

        #endregion

        #region WriteFatal

        /// <summary>
        /// Writes a Fatal level log.
        /// </summary>
        /// <param name="message">Log message.</param>
        public static void Fatal(string message)
        {
            Write(Serilog.Events.LogEventLevel.Fatal, message);
        }

        /// <summary>
        /// Writes a Fatal level log.
        /// </summary>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue">The property value for the message template.</param>
        public static void Fatal<T>(string messageTemplate, T propertyValue)
        {
            Write(Serilog.Events.LogEventLevel.Fatal, messageTemplate, propertyValue);
        }

        /// <summary>
        /// Writes a Fatal level log.
        /// </summary>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue0">The property value for the message template.</param>
        /// <param name="propertyValue1">The property value for the message template.</param>
        public static void Fatal<T0, T1>(string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
            Write(Serilog.Events.LogEventLevel.Fatal, messageTemplate, propertyValue0, propertyValue1);
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
            Write(Serilog.Events.LogEventLevel.Fatal, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        }

        /// <summary>
        /// Writes a Fatal level log.
        /// </summary>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValues">The property values for the message template.</param>
        public static void Fatal(string messageTemplate, params object[] propertyValues)
        {
            Write(Serilog.Events.LogEventLevel.Fatal, messageTemplate, propertyValues);
        }

        /// <summary>
        /// Writes a Fatal level log.
        /// </summary>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="message">Log message.</param>
        public static void Fatal(Exception exception, string message)
        {
            Write(Serilog.Events.LogEventLevel.Fatal, exception, message);
        }

        /// <summary>
        /// Writes a Fatal level log.
        /// </summary>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue">The property value for the message template.</param>
        public static void Fatal<T>(Exception exception, string messageTemplate, T propertyValue)
        {
            Write(Serilog.Events.LogEventLevel.Fatal, exception, messageTemplate, propertyValue);
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
            Write(Serilog.Events.LogEventLevel.Fatal, exception, messageTemplate, propertyValue0, propertyValue1);
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
            Write(Serilog.Events.LogEventLevel.Fatal, exception, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        }

        /// <summary>
        /// Writes a Fatal level log.
        /// </summary>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValues">The property values for the message template.</param>
        public static void Fatal(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Write(Serilog.Events.LogEventLevel.Fatal, exception, messageTemplate, propertyValues);
        }

        #endregion

    }
}
