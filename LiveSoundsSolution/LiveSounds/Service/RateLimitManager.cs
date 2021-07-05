using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zokma.Libs;
using Zokma.Libs.Logging;

namespace LiveSounds.Service
{
    /// <summary>
    /// Rate limit manager.
    /// </summary>
    internal class RateLimitManager
    {
        /// <summary>
        /// User counter.
        /// </summary>
        private class UserCounter
        {
            /// <summary>
            /// Elapsed timer.
            /// </summary>
            public Stopwatch ElapsedTimer { get; private set; }

            /// <summary>
            /// Creates UserCounter.
            /// </summary>
            public UserCounter()
            {
                this.ElapsedTimer = Stopwatch.StartNew();
            }
        }

        /// <summary>
        /// Lock for global counter.
        /// </summary>
        private readonly RockLock globalLock;

        /// <summary>
        /// Lock for user counter.
        /// </summary>
        private readonly RockLock userLock;

        /// <summary>
        /// Global Counter.
        /// </summary>
        private int globalCounter;

        /// <summary>
        /// Global Elapsed timer.
        /// </summary>
        private Stopwatch globalElapsedTimer;

        /// <summary>
        /// User counters.
        /// </summary>
        private Dictionary<string, UserCounter> userCounters;

        /// <summary>
        /// Global Limit.
        /// </summary>
        private int globalLimit;

        /// <summary>
        /// Global Limit.
        /// </summary>
        public int GlobalLimit 
        {
            get
            {
                using var rl = this.globalLock.EnterReadLock();
                
                return this.globalLimit;
            }

            set
            {
                using var wl = this.globalLock.EnterWriteLock();
                
                this.globalLimit = value;
            }
        }

        /// <summary>
        /// User Limit.
        /// </summary>
        private int userLimit;

        /// <summary>
        /// User Limit.
        /// </summary>
        public int UserLimit
        {
            get
            {
                using var rl = this.userLock.EnterReadLock();

                return this.userLimit;
            }

            set
            {
                using var wl = this.userLock.EnterWriteLock();

                this.userLimit = value;
            }
        }

        /// <summary>
        /// Creates RateLimitManager.
        /// </summary>
        public RateLimitManager()
        {
            this.globalLock = new RockLock();
            this.userLock   = new RockLock();

            this.globalCounter      = 0;
            this.globalElapsedTimer = new Stopwatch();

            this.userCounters = new Dictionary<string, UserCounter>();
        }

        /// <summary>
        /// Resets counters.
        /// </summary>
        public void Reset()
        {
            using(var wl = this.globalLock.EnterWriteLock())
            {
                this.globalCounter = 0;
                this.globalElapsedTimer.Restart();
            }

            using (var wl = this.userLock.EnterWriteLock())
            {
                this.userCounters.Clear();
            }
        }

        /// <summary>
        /// Checks global retry after value.
        /// </summary>
        /// <returns>Retry-After value for Global.</returns>
        public int CheckGlobalRetryAfterSecounds()
        {
            using var wl = this.globalLock.EnterWriteLock();

            if(this.globalLimit <= 0)
            {
                return 60;
            }

            int result = 0;

            this.globalCounter++;

            if(this.globalCounter > this.globalLimit)
            {
                double secs = this.globalElapsedTimer.Elapsed.TotalSeconds;

                if(secs > 60.0d)
                {
                    this.globalCounter = 1;
                    this.globalElapsedTimer.Restart();

                    CleanUserCounter();

                    if (Log.IsDebugEnabled)
                    {
                        Log.Debug("Global Counter restarted.");
                    }
                }
                else
                {
                    result = (int)(60.0d - secs);

                    if(result < 1)
                    {
                        result = 1;
                    }

                    if (Log.IsDebugEnabled)
                    {
                        Log.Debug("Global Counter: Retry-After={RetryAfter}", result);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Checks user retry after value.
        /// </summary>
        /// <param name="userHash">User hash value.</param>
        /// <returns>Retry-After value for User.</returns>
        public int CheckUserRetryAfterSecounds(string userHash)
        {
            using var wl = this.userLock.EnterWriteLock();

            if(this.userLimit <= 0)
            {
                return 60;
            }

            UserCounter counter;

            if(!this.userCounters.TryGetValue(userHash, out counter))
            {
                counter = new UserCounter();

                this.userCounters.TryAdd(userHash, counter);

                return 0;
            }

            int result = 0;

            double thresholdSecs = 60.0f / this.userLimit;
            double secs          = counter.ElapsedTimer.Elapsed.TotalSeconds;

            if (secs > thresholdSecs)
            {
                counter.ElapsedTimer.Restart();

                if (Log.IsDebugEnabled)
                {
                    Log.Debug("User Counter restarted.");
                }
            }
            else
            {
                result = (int)(thresholdSecs - secs);

                if (result < 1)
                {
                    result = 1;
                }

                if (Log.IsDebugEnabled)
                {
                    Log.Debug("User Counter: Retry-After={RetryAfter}", result);
                }
            }

            return result;
        }

        /// <summary>
        /// Cleans UserCounter.
        /// </summary>
        private void CleanUserCounter()
        {
            using var wl = this.userLock.EnterWriteLock();

            foreach (var item in this.userCounters)
            {
                double secs = item.Value.ElapsedTimer.Elapsed.TotalSeconds;

                if (secs > 60.0d)
                {
                    if (this.userCounters.Remove(item.Key))
                    {
                        if (Log.IsDebugEnabled)
                        {
                            Log.Debug("User Counter Removed: ElapsedSeconds = {Seconds}", secs);
                        }
                    }
                }
            }

            if (Log.IsDebugEnabled)
            {
                Log.Debug("User Counter Cleaned.");
            }
        }

    }
}
