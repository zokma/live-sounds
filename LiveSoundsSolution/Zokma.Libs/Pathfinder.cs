using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Zokma.Libs
{

    /// <summary>
    /// Do you know pathfinder?
    /// He is one of the Legends in the Apex Legends.
    /// He finds a way/path.
    /// This class also finds paths securely.
    /// The paths found are always under sub directory of the application.
    /// This means this class never find a path which are not related with this application.
    /// </summary>
    public static class Pathfinder
    {
        public static string ApplicationDirectory { get; private set; }

        static Pathfinder()
        {
            ApplicationDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        }

        /// <summary>
        /// Finds the full path info under the sub directory of the application.
        /// </summary>
        /// <param name="path">The path from app base directory.</param>
        /// <returns>The full path info under the sub directory of the application.</returns>
        public static FileInfo FindPathInfo(string path)
        {
            if (path != null)
            {
                path = path.Trim();
            }

            if (String.IsNullOrWhiteSpace(path) || path.StartsWith('.') ||
                path.StartsWith('/') || path.StartsWith('\\') || path.Contains(':'))
            {
                throw new ArgumentException(String.Format("Invalid path info: {0}", path), nameof(path));
            }

            return new FileInfo(Path.Combine(ApplicationDirectory, path));
        }

        /// <summary>
        /// Finds the full path info under the sub directory of the application.
        /// </summary>
        /// <param name="path">The path from app base directory.</param>
        /// <returns>The full path info under the sub directory of the application.</returns>
        public static string FindPathName(string path)
        {
            return FindPathInfo(path).FullName;
        }

    }
}
