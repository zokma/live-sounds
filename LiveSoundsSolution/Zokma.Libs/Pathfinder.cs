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
    public class Pathfinder
    {
        /// <summary>
        /// Entry application directory.
        /// </summary>
        public static readonly string ApplicationDirectory;

        /// <summary>
        /// Pathfinder for application root directory;
        /// </summary>
        public static readonly Pathfinder ApplicationRoot;

        static Pathfinder()
        {
            ApplicationDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            ApplicationRoot      = new Pathfinder(String.Empty);
        }

        /// <summary>
        /// Base directory under sub dir of this application.
        /// </summary>
        public readonly string BaseDirectory;

        /// <summary>
        /// Creates Pathfinder.
        /// </summary>
        /// <param name="path">The path from app base directory.</param>
        /// <exception cref="ArgumentException">The path is invalid.</exception>
        public Pathfinder(string path)
        {
            this.BaseDirectory = FindPathInfo(ApplicationDirectory, path).FullName;
        }

        /// <summary>
        /// Finds the full path info under the sub directory of the application.
        /// </summary>
        /// <param name="baseDirectory">The base directory under sub dir of this application.</param>
        /// <param name="path">The path from base directory.</param>
        /// <returns>The full path info under the sub directory of the application.</returns>
        /// <exception cref="ArgumentException">The path is invalid.</exception>
        private static FileInfo FindPathInfo(string baseDirectory, string path)
        {
            FileInfo result = null;

            if(baseDirectory != null && path != null)
            {
                result = new FileInfo(Path.Combine(baseDirectory, path));
            }

            if(result == null || !result.FullName.StartsWith(ApplicationDirectory))
            {
                throw new ArgumentException($"Invalid param: {nameof(baseDirectory)} = {baseDirectory}, {nameof(path)} = {path}");
            }

            return result;
        }

        /// <summary>
        /// Finds the full path info under the sub directory of the application.
        /// </summary>
        /// <param name="baseDirectory">The base directory under sub dir of this application.</param>
        /// <param name="path">The path from base directory.</param>
        /// <returns>The full path info under the sub directory of the application.</returns>
        /// <exception cref="ArgumentException">The path is invalid.</exception>
        public FileInfo FindPathInfo(string path)
        {
            return FindPathInfo(this.BaseDirectory, path);
        }

        /// <summary>
        /// Finds the full path info under the sub directory of the application.
        /// </summary>
        /// <param name="path">The path from app base directory.</param>
        /// <returns>The full path info under the sub directory of the application.</returns>
        /// <exception cref="ArgumentException">The path is invalid.</exception>
        public string FindPathName(string path)
        {
            return FindPathInfo(path).FullName;
        }
    }
}
