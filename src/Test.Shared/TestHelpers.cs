namespace Test.Shared
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using OpenAuditLog;

    /// <summary>
    /// Disposable wrapper that creates an AuditLog backed by a unique, temporary Sqlite
    /// database and cleans the file up on disposal.  Keeps each test isolated so they can
    /// run in parallel without contending on the same database.
    /// </summary>
    internal sealed class TempAuditLog : IDisposable
    {
        /// <summary>
        /// The audit log under test.
        /// </summary>
        internal AuditLog Log { get; }

        private readonly string _Path;

        /// <summary>
        /// Instantiate the wrapper.
        /// </summary>
        /// <param name="intervalMs">Emitter interval in milliseconds.  Kept small so integration tests do not wait on the 5000ms default.</param>
        internal TempAuditLog(int intervalMs = 50)
        {
            _Path = TempDatabase.CreatePath();

            Log = new AuditLog(_Path);

            // Drive the emitter fast so integration tests do not wait on the 5000ms default.
            Log.IntervalMs = intervalMs;
        }

        /// <summary>
        /// Dispose of the audit log and best-effort delete the backing database file.
        /// </summary>
        public void Dispose()
        {
            try { Log.Dispose(); }
            catch { /* best effort */ }

            TempDatabase.DeleteWithRetry(_Path);
        }
    }

    /// <summary>
    /// Temp database path and cleanup helpers shared by Sqlite-backed tests.
    /// </summary>
    internal static class TempDatabase
    {
        /// <summary>
        /// Create a unique database path for a test.
        /// </summary>
        /// <returns>Database path.</returns>
        internal static string CreatePath()
        {
            string dir = Path.Combine(Path.GetTempPath(), "oal-tests");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, Guid.NewGuid().ToString() + ".db");
        }

        /// <summary>
        /// Best-effort delete of a database file that may have just been used by the emitter.
        /// </summary>
        /// <param name="path">Database path.</param>
        internal static void DeleteWithRetry(string path)
        {
            if (String.IsNullOrEmpty(path)) return;

            // The emitter task may still hold the Sqlite handle briefly after cancellation.
            for (int i = 0; i < 120; i++)
            {
                try
                {
                    if (File.Exists(path)) File.Delete(path);
                    break;
                }
                catch
                {
                    Thread.Sleep(50);
                }
            }
        }
    }

    /// <summary>
    /// Small polling helper for integration-style assertions against the asynchronous emitter.
    /// </summary>
    internal static class Wait
    {
        /// <summary>
        /// Poll a predicate until it returns true or the timeout elapses.
        /// Returns true if the condition was satisfied.
        /// </summary>
        /// <param name="condition">Condition to evaluate.</param>
        /// <param name="timeoutMs">Maximum time to wait, in milliseconds.</param>
        /// <param name="pollMs">Polling interval, in milliseconds.</param>
        /// <returns>True if the condition was satisfied before the timeout.</returns>
        internal static bool Until(Func<bool> condition, int timeoutMs = 20000, int pollMs = 25)
        {
            Stopwatch sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                if (condition()) return true;
                Thread.Sleep(pollMs);
            }

            return condition();
        }
    }
}
