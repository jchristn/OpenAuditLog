namespace OpenAuditLog
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Object extensions.
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Return a JSON string of this object.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="pretty">Enable or disable pretty print.</param>
        /// <returns>JSON string.</returns>
        public static string ToJson(this object obj, bool pretty = false)
        {
            return Common.SerializeJson(obj, pretty);
        }
    }
}
