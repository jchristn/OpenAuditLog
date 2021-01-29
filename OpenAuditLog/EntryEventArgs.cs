using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuditLog
{
    /// <summary>
    /// Event arguments for attempts to send an entry to a target.
    /// </summary>
    public class EntryEventArgs : EventArgs
    {
        #region Public-Members

        /// <summary>
        /// Audit log entry.
        /// </summary>
        public AuditLogEntry Entry { get; private set; } = null;

        /// <summary>
        /// Audit log target.
        /// </summary>
        public AuditLogTarget Target { get; private set; } = null;

        /// <summary>
        /// Exception.
        /// </summary>
        public Exception Exception { get; private set; } = null;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public EntryEventArgs()
        {

        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="entry">Audit log entry.</param>
        /// <param name="target">Audit log target.</param>
        /// <param name="exception">Exception data, if any.</param>
        public EntryEventArgs(AuditLogEntry entry, AuditLogTarget target, Exception exception = null)
        {
            Entry = entry ?? throw new ArgumentNullException(nameof(entry));
            Target = target;
            Exception = exception;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
