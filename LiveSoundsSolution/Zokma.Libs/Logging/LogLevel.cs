using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zokma.Libs.Logging
{
    /// <summary>
    /// Level to be logged.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Verbose level.
        /// </summary>
        Verbose,

        /// <summary>
        /// Debug level.
        /// </summary>
        Debug,

        /// <summary>
        /// Information level.
        /// </summary>
        Information,

        /// <summary>
        /// Warning level.
        /// </summary>
        Warning,

        /// <summary>
        /// Error level.
        /// </summary>
        Error,

        /// <summary>
        /// Fatal level.
        /// </summary>
        Fatal,
    }
}
