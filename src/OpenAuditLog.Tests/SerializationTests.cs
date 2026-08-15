namespace OpenAuditLog.Tests
{
    using System;
    using OpenAuditLog;
    using Xunit;

    /// <summary>
    /// Positive and negative tests for serialization helpers and model conversion.
    /// </summary>
    public class SerializationTests
    {
        [Fact]
        public void ToJson_SerializesObject()
        {
            AuditLogEntry entry = new AuditLogEntry { Type = "TestEvent", ContentLength = 42 };
            string json = entry.ToJson(false);

            Assert.False(String.IsNullOrEmpty(json));
            Assert.Contains("TestEvent", json);
            Assert.Contains("42", json);
        }

        [Fact]
        public void SerializeJson_Null_ReturnsNull()
        {
            Assert.Null(Common.SerializeJson(null, false));
        }

        [Fact]
        public void SerializeJson_OmitsNullValues()
        {
            // Result is a non-null enum; null members should be dropped by WhenWritingNull.
            AuditLogEntry entry = new AuditLogEntry();
            string json = Common.SerializeJson(entry, false);

            Assert.DoesNotContain("\"Identity\"", json);
            Assert.DoesNotContain("\"Metadata\"", json);
        }

        [Fact]
        public void DeserializeJson_RoundTrip()
        {
            AuditLogEntry entry = new AuditLogEntry { Type = "Login", Result = EventResult.Success };
            string json = Common.SerializeJson(entry, false);

            AuditLogEntry copy = Common.DeserializeJson<AuditLogEntry>(json);

            Assert.Equal(entry.Type, copy.Type);
            Assert.Equal(entry.Result, copy.Result);
            Assert.Equal(entry.GUID, copy.GUID);
        }

        [Fact]
        public void DeserializeJson_NullString_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => Common.DeserializeJson<AuditLogEntry>((string)null));
        }

        [Fact]
        public void DeserializeJson_EmptyString_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => Common.DeserializeJson<AuditLogEntry>(""));
        }

        [Fact]
        public void DeserializeJson_NullBytes_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => Common.DeserializeJson<AuditLogEntry>((byte[])null));
        }

        [Fact]
        public void CopyObject_ProducesEquivalentCopy()
        {
            AuditLogEntry entry = new AuditLogEntry { Type = "Copy", ContentLength = 7 };
            AuditLogEntry copy = Common.CopyObject<AuditLogEntry>(entry);

            Assert.NotNull(copy);
            Assert.Equal(entry.Type, copy.Type);
            Assert.Equal(entry.ContentLength, copy.ContentLength);
        }

        [Fact]
        public void CopyObject_Null_ReturnsDefault()
        {
            Assert.Null(Common.CopyObject<AuditLogEntry>(null));
        }

        [Fact]
        public void Model_RoundTrip_PreservesScalarFields()
        {
            AuditLogEntry entry = new AuditLogEntry(
                identity: "user@example.com",
                source: "1.2.3.4",
                target: "server-a",
                resource: "/secret",
                metadata: "note",
                eventType: "Access",
                eventResult: EventResult.Denied,
                contentLength: 1024);
            entry.MaxAttempts = 3;

            AuditLogModel model = new AuditLogModel(entry);
            AuditLogEntry restored = model.ToAuditLogEntry();

            Assert.Equal(entry.GUID, restored.GUID);
            Assert.Equal(entry.Type, restored.Type);
            Assert.Equal(EventResult.Denied, restored.Result);
            Assert.Equal(1024, restored.ContentLength);
            Assert.Equal(3, restored.MaxAttempts);
            Assert.NotNull(restored.Identity);
            Assert.NotNull(restored.Metadata);
        }
    }
}
