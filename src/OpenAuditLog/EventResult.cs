using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace OpenAuditLog
{
    /// <summary>
    /// The result of the event.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
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
