﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            string procPath = null;

            try
            {
                procPath = Process.GetCurrentProcess()?.MainModule?.FileName;
            }
            catch { }

            if(String.IsNullOrEmpty(procPath))
            {
                procPath = Assembly.GetEntryAssembly()?.Location;
            }

            if (!String.IsNullOrEmpty(procPath))
            {
                ApplicationDirectory = Path.GetDirectoryName(procPath);
            }
            else
            {
                ApplicationDirectory = "." + Path.DirectorySeparatorChar;
            }

            ApplicationRoot = new Pathfinder(String.Empty);
        }

        /// <summary>
        /// Base directory under sub dir of this application.
        /// </summary>
        public readonly string BaseDirectory;

        /// <summary>
        /// Creates Pathfinder.
        /// </summary>
        /// <param name="baseDirectory">The base directory under sub dir of this application.</param>
        /// <param name="path">The path from app base directory.</param>
        /// <exception cref="ArgumentException">The path is invalid.</exception>
        private Pathfinder(string baseDirectory, string path)
        {
            this.BaseDirectory = FindPathInfo(baseDirectory, path).FullName;
        }

        /// <summary>
        /// Creates Pathfinder.
        /// </summary>
        /// <param name="baseDirectory">The base directory under sub dir of this application.</param>
        /// <param name="paths">The paths from app base directory.</param>
        /// <exception cref="ArgumentException">The path is invalid.</exception>
        private Pathfinder(string baseDirectory, params string[] paths)
        {
            this.BaseDirectory = FindPathInfo(baseDirectory, paths).FullName;
        }

        /// <summary>
        /// Creates Pathfinder.
        /// </summary>
        /// <param name="kind">The kind of the path.</param>
        /// <param name="path">The path from app base directory.</param>
        /// <exception cref="ArgumentException">The path is invalid.</exception>
        public Pathfinder(PathKind kind, string path)
            : this(GetBasePath(kind), path)
        {
        }

        /// <summary>
        /// Creates Pathfinder.
        /// </summary>
        /// <param name="kind">The kind of the path.</param>
        /// <param name="paths">The paths from app base directory.</param>
        /// <exception cref="ArgumentException">The path is invalid.</exception>
        public Pathfinder(PathKind kind, params string[] paths)
            : this(GetBasePath(kind), paths)
        {
        }

        /// <summary>
        /// Creates Pathfinder.
        /// </summary>
        /// <param name="path">The path from app base directory.</param>
        /// <exception cref="ArgumentException">The path is invalid.</exception>
        public Pathfinder(string path)
            : this(ApplicationDirectory, path)
        {
        }

        /// <summary>
        /// Creates Pathfinder.
        /// </summary>
        /// <param name="paths">The paths from app base directory.</param>
        /// <exception cref="ArgumentException">The path is invalid.</exception>
        public Pathfinder(params string[] paths)
            : this(ApplicationDirectory, paths)
        {
        }

        /// <summary>
        /// Gets base path.
        /// </summary>
        /// <param name="kind">The kind of the path.</param>
        /// <returns>The path for the PathKind.</returns>
        private static string GetBasePath(PathKind kind)
        {
            string result = kind switch
            {
                PathKind.ApplicationRoot      => ApplicationDirectory,
                PathKind.ApplicationData      => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                PathKind.LocalApplicationData => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                PathKind.Personal             => Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                _                             => null,
            };

            return result;
        }

        /// <summary>
        /// Checks if the path is secure or not.
        /// </summary>
        /// <param name="info">FileInfo to be checked.</param>
        /// <returns>true if secure path.</returns>
        private static bool CheckSecurePath(FileInfo info, string baseDirectory)
        {
            return (info != null && !String.IsNullOrWhiteSpace(baseDirectory) && info.FullName.StartsWith(baseDirectory));
        }

        /// <summary>
        /// Builds paths.
        /// </summary>
        /// <param name="baseDirectory">The base directory under sub dir of this application.</param>
        /// <param name="paths">The paths from base directory.</param>
        /// <returns>The paths.</returns>
        private static string[] BuildPaths(string baseDirectory, params string[] paths)
        {
            if(baseDirectory == null)
            {
                return null;
            }

            int capacity = 1;

            if (paths != null && paths.Length > 0)
            {
                capacity += paths.Length;
            }

            var result = new List<string>(capacity)
            {
                baseDirectory
            };

            if (capacity > 1)
            {
                result.AddRange(paths);
            }

            return result.ToArray();
        }

        /// <summary>
        /// Finds the full path info under the sub directory of the application.
        /// </summary>
        /// <param name="baseDirectory">The base directory under sub dir of this application.</param>
        /// <param name="paths">The paths from base directory.</param>
        /// <returns>The full path info under the sub directory of the application.</returns>
        /// <exception cref="ArgumentException">The path is invalid.</exception>
        private static FileInfo FindPathInfo(string baseDirectory, params string[] paths)
        {
            FileInfo result = null;

            var pathsToBeCombined = BuildPaths(baseDirectory, paths);

            if (pathsToBeCombined != null)
            {
                result = new FileInfo(Path.Combine(pathsToBeCombined));
            }

            if (!CheckSecurePath(result, baseDirectory))
            {
                throw new ArgumentException($"Result path: {result?.FullName}");
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
        private static FileInfo FindPathInfo(string baseDirectory, string path)
        {
            FileInfo result = null;

            if(baseDirectory != null && path != null)
            {
                result = new FileInfo(Path.Combine(baseDirectory, path));
            }

            if(!CheckSecurePath(result, baseDirectory))
            {
                throw new ArgumentException($"Result path: {result?.FullName}");
            }

            return result;
        }

        /// <summary>
        /// Gets sub Pathfinder to manage sub directory.
        /// </summary>
        /// <param name="path">The path from base directory.</param>
        /// <returns>The full path info under the sub directory of the application.</returns>
        /// <exception cref="ArgumentException">The path is invalid.</exception>
        public Pathfinder GetSubPathfinder(string path)
        {
            return new Pathfinder(this.BaseDirectory, path);
        }

        /// <summary>
        /// Gets sub Pathfinder to manage sub directory.
        /// </summary>
        /// <param name="paths">The paths from base directory.</param>
        /// <returns>The full path info under the sub directory of the application.</returns>
        /// <exception cref="ArgumentException">The path is invalid.</exception>
        public Pathfinder GetSubPathfinder(params string[] paths)
        {
            return new Pathfinder(this.BaseDirectory, paths);
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
        /// <param name="baseDirectory">The base directory under sub dir of this application.</param>
        /// <param name="paths">The paths from base directory.</param>
        /// <returns>The full path info under the sub directory of the application.</returns>
        /// <exception cref="ArgumentException">The path is invalid.</exception>
        public FileInfo FindPathInfo(params string[] paths)
        {
            return FindPathInfo(this.BaseDirectory, paths);
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

        /// <summary>
        /// Finds the full path info under the sub directory of the application.
        /// </summary>
        /// <param name="paths">The paths from app base directory.</param>
        /// <returns>The full path info under the sub directory of the application.</returns>
        /// <exception cref="ArgumentException">The path is invalid.</exception>
        public string FindPathName(params string[] paths)
        {
            return FindPathInfo(paths).FullName;
        }

    }
}
