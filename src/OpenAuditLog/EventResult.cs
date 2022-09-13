using System.Text.Json.Serialization;
using System.Runtime.Serialization;

namespace OpenAuditLog
{
    /// <summary>
    /// The result of the event.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum EventResult
    {
        /// <summary>
        /// Unknown.
        /// </summary>
        [EnumMember(Value = "Unknown")]
        Unknown,
        /// <summary>
        /// Success.
        /// </summary>
        [EnumMember(Value = "Success")]
        Success,
        /// <summary>
        /// Failure.
        /// </summary>
        [EnumMember(Value = "Failure")]
        Failure,
        /// <summary>
        /// Denied.
        /// </summary>
        [EnumMember(Value = "Denied")]
        Denied
    }
}
