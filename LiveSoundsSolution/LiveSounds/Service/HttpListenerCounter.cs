using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LiveSounds.Service
{
    /// <summary>
    /// Counter for HttpListener.
    /// </summary>
    internal class HttpListenerCounter
    {
        /// <summary>
        /// Max threads.
        /// </summary>
        private int maxThreads;

        /// <summary>
        /// Running threads.
        /// </summary>
        private int runningThreads;

        /// <summary>
        /// Threds terminated unexpectedly.
        /// </summary>
        private int unexpectedThreads;

        /// <summary>
        /// Running threads.
        /// </summary>
        public int RunningThreads => this.runningThreads;

        /// <summary>
        /// true if the HttpListener is alive.
        /// </summary>
        public bool IsAlive => (this.unexpectedThreads < this.maxThreads);

        /// <summary>
        /// Crates HttpListenerCounter.
        /// </summary>
        /// <param name="maxThreads">Max threads.</param>
        public HttpListenerCounter(int maxThreads)
        {
            this.maxThreads        = maxThreads;
            this.runningThreads    = 0;
            this.unexpectedThreads = 0;
        }

        /// <summary>
        /// Reports Listener started.
        /// </summary>
        public void ReportListenerStarted()
        {
            Interlocked.Increment(ref this.runningThreads);
        }

        /// <summary>
        /// Reports Listener stopped.
        /// </summary>
        public void ReportListenerStopped()
        {
            Interlocked.Decrement(ref this.runningThreads);
        }

        /// <summary>
        /// Reports Listener terminated unexpectedly.
        /// </summary>
        public void ReportListenerTerminatedUnexpectedly()
        {
            Interlocked.Increment(ref this.unexpectedThreads);
            ReportListenerStopped();
        }

    }
}
