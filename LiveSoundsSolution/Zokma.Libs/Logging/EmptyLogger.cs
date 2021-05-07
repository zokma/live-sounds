using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zokma.Libs.Logging
{
    /// <summary>
    /// Empty logger that does not output any outputs.
    /// </summary>
    internal class EmptyLogger : ILogger
    {
        /// <summary>
        /// Instance of the EmptyLogger.
        /// </summary>
        public static readonly EmptyLogger Instance = new EmptyLogger();

        private EmptyLogger()
        { 
        }

        /// <summary>
        /// Checks if the log level is enabled.
        /// </summary>
        /// <param name="level">LogLevel to be checked.</param>
        /// <returns>The log level is enabled.</returns>
        public bool IsEnabled(LogEventLevel level)
        {
            return false;
        }

        public void Write(LogEvent logEvent)
        {
        }

        /// <summary>
        /// Writes a specified level log.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="message">Log message.</param>
        public void Write(LogEventLevel level, string message)
        {
        }

        /// <summary>
        /// Writes a specified level log.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue">The property value for the message template.</param>
        public void Write<T>(LogEventLevel level, string messageTemplate, T propertyValue)
        {
        }

        /// <summary>
        /// Writes a specified level log.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue0">The property value for the message template.</param>
        /// <param name="propertyValue1">The property value for the message template.</param>
        public void Write<T0, T1>(LogEventLevel level, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
        }

        /// <summary>
        /// Writes a specified level log.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue0">The property value for the message template.</param>
        /// <param name="propertyValue1">The property value for the message template.</param>
        /// <param name="propertyValue2">The property value for the message template.</param>
        public void Write<T0, T1, T2>(LogEventLevel level, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        {
        }

        /// <summary>
        /// Writes a specified level log.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValues">The property values for the message template.</param>
        public void Write(LogEventLevel level, string messageTemplate, params object[] propertyValues)
        {
        }

        /// <summary>
        /// Writes a specified level log.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="message">Log message.</param>
        public void Write(LogEventLevel level, Exception exception, string message)
        {
        }

        /// <summary>
        /// Writes a specified level log.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue">The property value for the message template.</param>
        public void Write<T>(LogEventLevel level, Exception exception, string messageTemplate, T propertyValue)
        {
        }

        /// <summary>
        /// Writes a specified level log.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValue0">The property value for the message template.</param>
        /// <param name="propertyValue1">The property value for the message template.</param>
        public void Write<T0, T1>(LogEventLevel level, Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
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
        public void Write<T0, T1, T2>(LogEventLevel level, Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        {
        }

        /// <summary>
        /// Writes a specified level log.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="exception">Exception related with this log message.</param>
        /// <param name="messageTemplate">Log message.</param>
        /// <param name="propertyValues">The property values for the message template.</param>
        public void Write(LogEventLevel level, Exception exception, string messageTemplate, params object[] propertyValues)
        {
        }
    }
}
