using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Zokma.Libs
{

    /// <summary>
    /// Enter and Exit Read lock.
    /// </summary>
    public class ReadLock : IDisposable
    {
        /// <summary>
        /// Reader Writer lock.
        /// </summary>
        private readonly ReaderWriterLockSlim rwLock;

        /// <summary>
        /// Creates ReadLock.
        /// </summary>
        /// <param name="rwLock">Base Lock.</param>
        internal ReadLock(ReaderWriterLockSlim rwLock)
        {
            this.rwLock = rwLock;
        }

        /// <summary>
        /// Enters Read lock.
        /// </summary>
        internal void Enter()
        {
            this.rwLock.EnterReadLock();
        }

        /// <summary>
        /// Exits Read lock.
        /// </summary>
        public void Dispose()
        {
            this.rwLock.ExitReadLock();
        }
    }

    /// <summary>
    /// Enter and Exit Write lock.
    /// </summary>
    public class WriteLock : IDisposable
    {
        /// <summary>
        /// Reader Writer lock.
        /// </summary>
        private readonly ReaderWriterLockSlim rwLock;

        /// <summary>
        /// Creates WriteLock.
        /// </summary>
        /// <param name="rwLock">Base Lock.</param>
        internal WriteLock(ReaderWriterLockSlim rwLock)
        {
            this.rwLock = rwLock;
        }

        /// <summary>
        /// Enters Write lock.
        /// </summary>
        internal void Enter()
        {
            this.rwLock.EnterWriteLock();
        }

        /// <summary>
        /// Exits Write lock.
        /// </summary>
        public void Dispose()
        {
            this.rwLock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Lock that should be used with using statement.
    /// </summary>
    public class RockLock
    {
        /// <summary>
        /// Reader Writer lock.
        /// </summary>
        private readonly ReaderWriterLockSlim rwLock;

        /// <summary>
        /// Read lock.
        /// </summary>
        private readonly ReadLock readLock;

        /// <summary>
        /// Write lock.
        /// </summary>
        private readonly WriteLock writeLock;

        /// <summary>
        /// Creates RockLock.
        /// </summary>
        /// <param name="recursionPolicy">Recursion policy.</param>
        public RockLock(LockRecursionPolicy recursionPolicy = LockRecursionPolicy.NoRecursion)
        {
            this.rwLock = new ReaderWriterLockSlim(recursionPolicy);

            this.readLock  = new ReadLock(this.rwLock);
            this.writeLock = new WriteLock(this.rwLock);
        }

        /// <summary>
        /// Enters read lock. ReadLock shoud be disposed by using statement.
        /// </summary>
        /// <returns>Read lock.</returns>
        public ReadLock EnterReadLock()
        {
            this.readLock.Enter();

            return this.readLock;
        }

        /// <summary>
        /// Enters Write lock. WriteLock shoud be disposed by using statement.
        /// </summary>
        /// <returns>Write lock.</returns>
        public WriteLock EnterWriteLock()
        {
            this.writeLock.Enter();

            return this.writeLock;
        }
    }
}
