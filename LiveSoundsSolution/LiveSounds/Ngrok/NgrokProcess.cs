using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Zokma.Libs;
using Zokma.Libs.Logging;

namespace LiveSounds.Ngrok
{
    /// <summary>
    /// Process for Ngrok.
    /// </summary>
    internal class NgrokProcess : IDisposable
    {

        /// <summary>
        /// Ngrok exe name.
        /// </summary>
        private const string NGROK_EXE_NAME = "ngrok.exe";

        /// <summary>
        /// Tools Directory name.
        /// </summary>
        private const string TOOLS_DIRECTORY_NAME = "Tools";

        /// <summary>
        /// Wait duration to exit ngrok process in millisecounds.
        /// </summary>
        private const int WAIT_DURATION_TO_EXIT_PROCESS_MS = 2500;

        /// <summary>
        /// Default region.
        /// </summary>
        private const string DEFAULT_REGION = "us";

        /// <summary>
        /// ngrok regions.
        /// It maybe updated in the future.
        /// </summary>
        private static readonly string[] REGIONS = { DEFAULT_REGION, "jp", "eu", "ap", "au", "sa", "in" };

        /// <summary>
        /// Tools Directory.
        /// </summary>
        public static readonly Pathfinder ToolsDirectory;

        /// <summary>
        /// Path to ngrok.exe.
        /// </summary>
        public static readonly string NgrokExePath;


        #region Interop to send Ctrl+C key signal.

        /// <summary>
        /// Generates a CTRL+C signal.
        /// </summary>
        private const uint CTRL_C_EVENT = 0;

        /// <summary>
        /// Sends a specified signal to a console.
        /// </summary>
        /// <param name="dwCtrlEvent">The type of signal to be generated.</param>
        /// <param name="dwProcessGroupId">The identifier of the process group to receive the signal. </param>
        /// <returns>If the function succeeds, the return value is nonzero.</returns>
        [DllImport("kernel32.dll")]
        private static extern bool GenerateConsoleCtrlEvent(uint dwCtrlEvent, uint dwProcessGroupId);

        /// <summary>
        /// Attaches the calling process to the console of the specified process as a client application.
        /// </summary>
        /// <param name="dwProcessId">The identifier of the process whose console is to be used.</param>
        /// <returns>If the function succeeds, the return value is nonzero.</returns>
        [DllImport("kernel32.dll")]
        private static extern bool AttachConsole(uint dwProcessId);

        /// <summary>
        /// Detaches the calling process from its console.
        /// </summary>
        /// <returns>If the function succeeds, the return value is nonzero.</returns>
        [DllImport("kernel32.dll")]
        private static extern bool FreeConsole();

        /// <summary>
        /// Adds or removes an application-defined HandlerRoutine function from the list of handler functions for the calling process.
        /// </summary>
        /// <param name="handlerRoutine">A pointer to the application-defined HandlerRoutine function to be added or removed.</param>
        /// <param name="add">If this parameter is TRUE, the handler is added.</param>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleCtrlHandler(ConsoleCtrlHandler handlerRoutine, bool add);

        /// <summary>
        /// The application-defined HandlerRoutine function.
        /// </summary>
        /// <param name="dwCtrlType">The type of control signal received by the handler.</param>
        /// <returns>If the function handles the control signal, it should return true.</returns>
        private delegate bool ConsoleCtrlHandler(uint dwCtrlType);

        #endregion


        /// <summary>
        /// Process for ngrok.
        /// </summary>
        private Process ngrokProcess;

        private bool disposedValue;

        static NgrokProcess()
        {
            ToolsDirectory = App.UserDirectory.GetSubPathfinder(TOOLS_DIRECTORY_NAME);
            NgrokExePath = ToolsDirectory.FindPathName(NGROK_EXE_NAME);
        }

        /// <summary>
        /// true if the process is running.
        /// </summary>
        public bool IsRunning
        {
            get
            {
                var proc = this.ngrokProcess;

                return (proc != null && !proc.HasExited);
            }
        }

        /// <summary>
        /// Cannot instanciate directly from outside.
        /// </summary>
        private NgrokProcess() { }

        /// <summary>
        /// Gets valid region name.
        /// </summary>
        /// <param name="region">Region name.</param>
        /// <returns>Valid region name.</returns>
        public static string GetValidRegion(string region)
        {
            if(String.IsNullOrWhiteSpace(region))
            {
                return DEFAULT_REGION;
            }

            region = region.Trim().ToLower();

            string result = DEFAULT_REGION;

            // The regions list is tiny, so simple search will be a reasonable way.
            foreach (var item in REGIONS)
            {
                if(region == item)
                {
                    result = item;
                    break;
                }
            }

            return result;
        }


        /// <summary>
        /// Starts ngrok process.
        /// </summary>
        /// <param name="port">Port number.</param>
        /// <param name="region">Region name.</param>
        /// <returns>The created NgrokProcess.</returns>
        public static NgrokProcess StartProcess(int port, string region)
        {
            var info = new ProcessStartInfo()
            {
                FileName = NgrokExePath,
                Arguments = $"http -region={ GetValidRegion(region) } -bind-tls=true { port }",
                UseShellExecute = false,
            };

            var proc = Process.Start(info);

            return new NgrokProcess() { ngrokProcess = proc };
        }

        /// <summary>
        /// Closes the ngrok process.
        /// </summary>
        /// <returns>true if the process is closed.</returns>
        public bool Close()
        {
            bool result = false;

            var proc = this.ngrokProcess;

            if(this.IsRunning)
            {
                if(AttachConsole((uint)proc.Id))
                {
                    // Attaches to the ngrok console, then disable Ctrl+C command to caller process.
                    SetConsoleCtrlHandler(null, true);

                    try
                    {
                        if(GenerateConsoleCtrlEvent(CTRL_C_EVENT, 0))
                        {
                            result = proc.WaitForExit(WAIT_DURATION_TO_EXIT_PROCESS_MS);
                        }
                    }
                    catch (SystemException se)
                    {
                        Log.Error(se, "Failed to exit ngrok process.");
                    }
                    finally
                    {
                        SetConsoleCtrlHandler(null, false);
                        FreeConsole();
                    }
                }
            }

            return result;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.ngrokProcess?.Dispose();
                    this.ngrokProcess = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~NgrokProcess()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
