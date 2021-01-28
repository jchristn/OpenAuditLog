﻿using System;
using System.Collections.Generic;
using Watson.ORM.Core;
using Newtonsoft.Json;

namespace OpenAuditLog
{
    /// <summary>
    /// Audit log entry.
    /// </summary>
    [Table("entries")]
    public class AuditLogEntry
    {
        #region Public-Members

        /// <summary>
        /// Database row ID.
        /// </summary>
        [JsonIgnore]
        [Column("id", true, DataTypes.Int, false)]
        public int Id { get; set; } = 0;

        /// <summary>
        /// GUID.
        /// </summary>
        [Column("guid", false, DataTypes.Nvarchar, 64, false)]
        public string GUID { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// GUID.
        /// </summary>
        [Column("targetguid", false, DataTypes.Nvarchar, 64, false)]
        public string TargetGUID { get; set; } = null;

        /// <summary>
        /// Identity of the originator of the request.
        /// </summary>
        [Column("identity", false, DataTypes.Nvarchar, 64, false)]
        public string Identity { get; set; } = null;

        /// <summary>
        /// Event source.
        /// </summary>
        [Column("source", false, DataTypes.Nvarchar, 64, false)]
        public string Source { get; set; } = null;
        
        /// <summary>
        /// Event target.
        /// </summary>
        [Column("target", false, DataTypes.Nvarchar, 64, false)]
        public string Target { get; set; } = null;

        /// <summary>
        /// Resource on the target with which the source was attempting to interact.
        /// </summary>
        [Column("resource", false, DataTypes.Nvarchar, 256, false)]
        public string Resource { get; set; } = null;
        
        /// <summary>
        /// The handle of the resource.
        /// </summary>
        [Column("handle", false, DataTypes.Nvarchar, 256, false)]
        public string Handle { get; set; } = null;
        
        /// <summary>
        /// Type of event.
        /// </summary>
        [Column("type", false, DataTypes.Nvarchar, 64, false)]
        public string Type { get; set; } = null;

        /// <summary>
        /// Indicates the result of the operation.
        /// </summary>
        [Column("result", false, DataTypes.Nvarchar, 32, false)]
        public EventResult Result { get; set; } = EventResult.Unknown;

        /// <summary>
        /// Amount of data involved in the event.
        /// </summary>
        [Column("len", false, DataTypes.Long, true)]
        public long ContentLength { get; set; } = 0;

        /// <summary>
        /// User-supplied metadata.
        /// </summary>
        [Column("md", false, DataTypes.Blob, true)]
        public string Metadata { get; set; } = null;

        /// <summary>
        /// The number of attempts made to send this event.
        /// </summary>
        [Column("attempts", false, DataTypes.Int, false)]
        public int Attempts { get; set; } = 0;

        /// <summary>
        /// The maximum number of attempts allowed to send this event.
        /// </summary>
        [Column("maxattempts", false, DataTypes.Int, false)]
        public int MaxAttempts { get; set; } = 0;

        /// <summary>
        /// Timestamp of the creation of the event.
        /// </summary>
        [Column("createdutc", false, DataTypes.DateTime, false)]
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
        /// <param name="identity">Identity of the event originator.</param>
        /// <param name="source">Event source.</param>
        /// <param name="target">Event target.</param>
        /// <param name="resource">Resource on the target with which the source was attempting to interact.</param>
        /// <param name="handle">The handle of the resource.</param>
        /// <param name="eventType">Type of event.</param>
        /// <param name="eventResult">Result of the event.</param>
        /// <param name="contentLength">Amount of data involved in the event.</param>
        /// <param name="metadata">User-supplied metadata.</param>
        public AuditLogEntry(string identity, string source, string target, string resource, string handle, string eventType, EventResult eventResult, long contentLength = 0, string metadata = null)
        {
            Identity = identity ?? throw new ArgumentNullException(nameof(identity));
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Target = target ?? throw new ArgumentNullException(nameof(target));
            Resource = resource ?? throw new ArgumentNullException(nameof(resource));
            Handle = handle ?? throw new ArgumentNullException(nameof(handle));
            Type = eventType ?? throw new ArgumentNullException(nameof(eventType));
            Result = eventResult;
            ContentLength = contentLength;
            Metadata = metadata; 
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Create a JSON representation.
        /// </summary>
        /// <param name="pretty">Enable or disable pretty print.</param>
        /// <returns>JSON string.</returns>
        public string ToJson(bool pretty)
        {
            return Common.SerializeJson(this, pretty);
        }

        #endregion
    }
}