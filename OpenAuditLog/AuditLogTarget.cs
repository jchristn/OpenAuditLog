using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuditLog
{
    /// <summary>
    /// Audit log target.
    /// </summary>
    public class AuditLogTarget
    {
        #region Public-Members

        /// <summary>
        /// GUID.
        /// </summary>
        public string GUID { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Name of the target.
        /// </summary>
        public string Name { get; set; } = null;

        /// <summary>
        /// Method to invoke to send the event to the target.  The method should return true if successful, and false if failed.
        /// </summary>
        public Func<AuditLogEntry, bool> Action { get; set; } = null;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="name">Name of the target.</param>
        /// <param name="action">Method to invoke to send the event to the target.  The method should return true if successful, and false if failed.</param>
        public AuditLogTarget(string name, Func<AuditLogEntry, bool> action)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Action = action ?? throw new ArgumentNullException(nameof(action));
        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="guid">GUID.</param>
        /// <param name="name">Name of the target.</param>
        /// <param name="action">Method to invoke to send the event to the target.  The method should return true if successful, and false if failed.</param>
        public AuditLogTarget(string guid, string name, Func<AuditLogEntry, bool> action)
        {
            GUID = guid ?? throw new ArgumentNullException(nameof(guid));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Action = action ?? throw new ArgumentNullException(nameof(action));
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
