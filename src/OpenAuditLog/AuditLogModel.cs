using System;
using System.Text.Json.Serialization;
using DatabaseWrapper.Core;
using Watson.ORM.Core;

namespace OpenAuditLog
{
    /// <summary>
    /// Database model for audit log entry.
    /// </summary>
    [Table("entries")]
    public class AuditLogModel
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
        [Column("identity", false, DataTypes.Blob, true)]
        public string Identity { get; set; } = null;

        /// <summary>
        /// Event source.
        /// </summary>
        [Column("source", false, DataTypes.Blob, true)]
        public string Source { get; set; } = null;
        
        /// <summary>
        /// Event target.
        /// </summary>
        [Column("target", false, DataTypes.Blob, true)]
        public string Target { get; set; } = null;

        /// <summary>
        /// Resource on the target with which the source was attempting to interact.
        /// </summary>
        [Column("resource", false, DataTypes.Blob, true)]
        public string Resource { get; set; } = null;
        
        /// <summary>
        /// Type of event.
        /// </summary>
        [Column("type", false, DataTypes.Nvarchar, 64, true)]
        public string Type { get; set; } = null;

        /// <summary>
        /// Indicates the result of the operation.
        /// </summary>
        [Column("result", false, DataTypes.Nvarchar, 32, true)]
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
        public AuditLogModel()
        {

        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="entry">Audit log entry.</param>
        public AuditLogModel(AuditLogEntry entry)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            if (String.IsNullOrEmpty(entry.GUID)) entry.GUID = Guid.NewGuid().ToString();

            GUID = entry.GUID;
            TargetGUID = entry.TargetGUID;

            if (entry.Identity != null) Identity = Common.SerializeJson(entry.Identity, false);
            if (entry.Source != null) Source = Common.SerializeJson(entry.Source, false);
            if (entry.Target != null) Target = Common.SerializeJson(entry.Target, false);
            if (entry.Resource != null) Resource = Common.SerializeJson(entry.Resource, false);

            Type = entry.Type;
            Result = entry.Result;
            ContentLength = entry.ContentLength;

            if (entry.Metadata != null) Metadata = Common.SerializeJson(entry.Metadata, false);

            MaxAttempts = entry.MaxAttempts;
            CreatedUtc = entry.CreatedUtc;
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Create an audit log entry object from the model.
        /// </summary>
        /// <returns></returns>
        public AuditLogEntry ToAuditLogEntry()
        {
            AuditLogEntry ret = new AuditLogEntry();

            ret.GUID = GUID;
            ret.TargetGUID = TargetGUID;

            if (!String.IsNullOrEmpty(Identity)) ret.Identity = Common.DeserializeJson<object>(Identity);
            if (!String.IsNullOrEmpty(Source)) ret.Source = Common.DeserializeJson<object>(Source);
            if (!String.IsNullOrEmpty(Target)) ret.Target = Common.DeserializeJson<object>(Target);
            if (!String.IsNullOrEmpty(Resource)) ret.Resource = Common.DeserializeJson<object>(Resource);

            ret.Type = Type;
            ret.Result = Result;
            ret.ContentLength = ContentLength;
            
            if (!String.IsNullOrEmpty(Metadata)) ret.Metadata = Common.DeserializeJson<object>(Metadata);

            ret.MaxAttempts = MaxAttempts;
            ret.CreatedUtc = CreatedUtc;

            return ret;
        }

        #endregion
    }
}
