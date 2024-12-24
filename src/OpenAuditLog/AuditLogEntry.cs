namespace OpenAuditLog
{
    using System;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Audit log entry.
    /// </summary>
    public class AuditLogEntry
    {
        #region Public-Members

        /// <summary>
        /// GUID.
        /// </summary>
        public string GUID { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// GUID.
        /// </summary>
        public string TargetGUID { get; set; } = null;

        /// <summary>
        /// Identity of the originator of the request.
        /// </summary>
        public object Identity { get; set; } = null;

        /// <summary>
        /// Event source.
        /// </summary>
        public object Source { get; set; } = null;
        
        /// <summary>
        /// Event target.
        /// </summary>
        public object Target { get; set; } = null;

        /// <summary>
        /// Resource on the target with which the source was attempting to interact.
        /// </summary>
        public object Resource { get; set; } = null;
        
        /// <summary>
        /// Type of event.
        /// </summary>
        public string Type { get; set; } = null;

        /// <summary>
        /// Indicates the result of the operation.
        /// </summary>
        public EventResult Result { get; set; } = EventResult.Unknown;

        /// <summary>
        /// Amount of data involved in the event.
        /// </summary>
        public long ContentLength { get; set; } = 0;

        /// <summary>
        /// User-supplied metadata.
        /// </summary>
        public object Metadata { get; set; } = null;

        /// <summary>
        /// The maximum number of attempts allowed to send this event.
        /// </summary>
        public int MaxAttempts { get; set; } = 0;

        /// <summary>
        /// Timestamp in UTC from the creation of the event.
        /// </summary>
        public DateTime CreatedUtc { get; set; } = DateTime.Now.ToUniversalTime();

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public AuditLogEntry()
        {

        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="metadata">User-supplied metadata.</param>
        /// <param name="identity">Identity of the event originator.</param>
        /// <param name="source">Event source.</param>
        /// <param name="target">Event target.</param>
        /// <param name="resource">Resource on the target with which the source was attempting to interact.</param>
        /// <param name="eventType">Type of event.</param>
        /// <param name="eventResult">Result of the event.</param>
        /// <param name="contentLength">Amount of data involved in the event.</param>
        public AuditLogEntry(
            object identity = null,
            object source = null,
            object target = null,
            object resource = null,
            object metadata = null,
            string eventType = null,
            EventResult eventResult = EventResult.Unknown,
            long contentLength = 0)
        {
            Identity = identity;
            Source = source;
            Target = target;
            Resource = resource;
            Metadata = metadata;
            Type = eventType;
            Result = eventResult;
            ContentLength = contentLength;
        }
         
        #endregion

        #region Public-Methods

        #endregion
    }
}
