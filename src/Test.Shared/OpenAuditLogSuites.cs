namespace Test.Shared
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using OpenAuditLog;
    using Touchstone.Core;

    /// <summary>
    /// Shared Touchstone suites for OpenAuditLog.  This is the central source of truth for
    /// every test; the Touchstone console runner (Test.Automated), the xUnit adapter
    /// (Test.Xunit), and the NUnit adapter (Test.Nunit) all execute these same descriptors.
    /// </summary>
    public static class OpenAuditLogSuites
    {
        #region Public-Members

        /// <summary>
        /// All shared suites, positive and negative, covering the full public surface of the library.
        /// </summary>
        public static IReadOnlyList<TestSuiteDescriptor> All
        {
            get
            {
                return new List<TestSuiteDescriptor>
                {
                    EntrySuite(),
                    TargetSuite(),
                    ModelSuite(),
                    EventArgsSuite(),
                    EventResultSuite(),
                    SerializationSuite(),
                    ValidationSuite(),
                    TargetManagementSuite(),
                    EventSuite(),
                    EmitterSuite()
                };
            }
        }

        #endregion

        #region AuditLogEntry

        private static TestSuiteDescriptor EntrySuite()
        {
            string suiteId = "entry";
            return new TestSuiteDescriptor(
                suiteId,
                "AuditLogEntry construction and defaults",
                new List<TestCaseDescriptor>
                {
                    new TestCaseDescriptor(suiteId, "default_constructor_sets_defaults", "Default constructor sets expected default values", _ =>
                    {
                        AuditLogEntry entry = new AuditLogEntry();
                        Assert(!String.IsNullOrEmpty(entry.GUID), "GUID should be auto-populated.");
                        Assert(entry.TargetGUID == null, "TargetGUID should default to null.");
                        Assert(entry.Identity == null, "Identity should default to null.");
                        Assert(entry.Source == null, "Source should default to null.");
                        Assert(entry.Target == null, "Target should default to null.");
                        Assert(entry.Resource == null, "Resource should default to null.");
                        Assert(entry.Metadata == null, "Metadata should default to null.");
                        Assert(entry.Type == null, "Type should default to null.");
                        Assert(entry.Result == EventResult.Unknown, "Result should default to Unknown.");
                        Assert(entry.ContentLength == 0, "ContentLength should default to zero.");
                        Assert(entry.MaxAttempts == 0, "MaxAttempts should default to zero.");
                        Assert(entry.CreatedUtc != default(DateTime), "CreatedUtc should be populated.");
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "default_constructor_generates_unique_guids", "Each entry receives a distinct GUID", _ =>
                    {
                        AuditLogEntry a = new AuditLogEntry();
                        AuditLogEntry b = new AuditLogEntry();
                        Assert(a.GUID != b.GUID, "Two entries should not share a GUID.");
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "parameterized_constructor_sets_fields", "Parameterized constructor assigns every argument", _ =>
                    {
                        AuditLogEntry entry = new AuditLogEntry(
                            identity: "user",
                            source: "src",
                            target: "tgt",
                            resource: "res",
                            metadata: "md",
                            eventType: "Login",
                            eventResult: EventResult.Success,
                            contentLength: 99);

                        Assert((string)entry.Identity == "user", "Identity mismatch.");
                        Assert((string)entry.Source == "src", "Source mismatch.");
                        Assert((string)entry.Target == "tgt", "Target mismatch.");
                        Assert((string)entry.Resource == "res", "Resource mismatch.");
                        Assert((string)entry.Metadata == "md", "Metadata mismatch.");
                        Assert(entry.Type == "Login", "Type mismatch.");
                        Assert(entry.Result == EventResult.Success, "Result mismatch.");
                        Assert(entry.ContentLength == 99, "ContentLength mismatch.");
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "parameterized_constructor_defaults", "Parameterized constructor applies documented defaults", _ =>
                    {
                        AuditLogEntry entry = new AuditLogEntry();
                        Assert(entry.Result == EventResult.Unknown, "Default result should be Unknown.");
                        Assert(entry.ContentLength == 0, "Default content length should be zero.");
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "properties_are_mutable", "Public properties can be assigned and read back", _ =>
                    {
                        AuditLogEntry entry = new AuditLogEntry();
                        entry.GUID = "guid-1";
                        entry.TargetGUID = "target-1";
                        entry.Type = "Custom";
                        entry.Result = EventResult.Denied;
                        entry.ContentLength = 512;
                        entry.MaxAttempts = 7;

                        Assert(entry.GUID == "guid-1", "GUID assignment failed.");
                        Assert(entry.TargetGUID == "target-1", "TargetGUID assignment failed.");
                        Assert(entry.Type == "Custom", "Type assignment failed.");
                        Assert(entry.Result == EventResult.Denied, "Result assignment failed.");
                        Assert(entry.ContentLength == 512, "ContentLength assignment failed.");
                        Assert(entry.MaxAttempts == 7, "MaxAttempts assignment failed.");
                        return Task.CompletedTask;
                    })
                });
        }

        #endregion

        #region AuditLogTarget

        private static TestSuiteDescriptor TargetSuite()
        {
            string suiteId = "target";
            return new TestSuiteDescriptor(
                suiteId,
                "AuditLogTarget construction and validation",
                new List<TestCaseDescriptor>
                {
                    new TestCaseDescriptor(suiteId, "two_arg_constructor_sets_fields", "Two-argument constructor sets name and action and auto-generates a GUID", _ =>
                    {
                        AuditLogTarget target = new AuditLogTarget("primary", e => true);
                        Assert(target.Name == "primary", "Name was not set.");
                        Assert(target.Action != null, "Action was not set.");
                        Assert(!String.IsNullOrEmpty(target.GUID), "GUID should be auto-generated.");
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "three_arg_constructor_sets_guid", "Three-argument constructor honors the supplied GUID", _ =>
                    {
                        AuditLogTarget target = new AuditLogTarget("guid-1", "primary", e => true);
                        Assert(target.GUID == "guid-1", "GUID was not honored.");
                        Assert(target.Name == "primary", "Name was not set.");
                        Assert(target.Action != null, "Action was not set.");
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "distinct_instances_generate_distinct_guids", "Auto-generated GUIDs are unique per instance", _ =>
                    {
                        AuditLogTarget a = new AuditLogTarget("a", e => true);
                        AuditLogTarget b = new AuditLogTarget("b", e => true);
                        Assert(a.GUID != b.GUID, "Targets should not share auto-generated GUIDs.");
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "two_arg_null_name_throws", "Two-argument constructor rejects a null name", _ =>
                    {
                        AssertThrows<ArgumentNullException>(() => new AuditLogTarget(null, e => true), "Null name should throw.");
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "two_arg_null_action_throws", "Two-argument constructor rejects a null action", _ =>
                    {
                        AssertThrows<ArgumentNullException>(() => new AuditLogTarget("name", null), "Null action should throw.");
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "three_arg_null_guid_throws", "Three-argument constructor rejects a null GUID", _ =>
                    {
                        AssertThrows<ArgumentNullException>(() => new AuditLogTarget(null, "name", e => true), "Null GUID should throw.");
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "three_arg_null_name_throws", "Three-argument constructor rejects a null name", _ =>
                    {
                        AssertThrows<ArgumentNullException>(() => new AuditLogTarget("guid", null, e => true), "Null name should throw.");
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "three_arg_null_action_throws", "Three-argument constructor rejects a null action", _ =>
                    {
                        AssertThrows<ArgumentNullException>(() => new AuditLogTarget("guid", "name", null), "Null action should throw.");
                        return Task.CompletedTask;
                    })
                });
        }

        #endregion

        #region AuditLogModel

        private static TestSuiteDescriptor ModelSuite()
        {
            string suiteId = "model";
            return new TestSuiteDescriptor(
                suiteId,
                "AuditLogModel conversion and round-trip",
                new List<TestCaseDescriptor>
                {
                    new TestCaseDescriptor(suiteId, "default_constructor_sets_defaults", "Default constructor populates a GUID and defaults", _ =>
                    {
                        AuditLogModel model = new AuditLogModel();
                        Assert(!String.IsNullOrEmpty(model.GUID), "GUID should be populated.");
                        Assert(model.Result == EventResult.Unknown, "Result should default to Unknown.");
                        Assert(model.Attempts == 0, "Attempts should default to zero.");
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "null_entry_constructor_throws", "Constructing from a null entry throws", _ =>
                    {
                        AssertThrows<ArgumentNullException>(() => new AuditLogModel(null), "Null entry should throw.");
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "constructor_copies_scalar_fields", "Constructor copies scalar fields from the entry", _ =>
                    {
                        AuditLogEntry entry = new AuditLogEntry { Type = "Access", Result = EventResult.Denied, ContentLength = 1024, MaxAttempts = 3 };
                        AuditLogModel model = new AuditLogModel(entry);
                        Assert(model.GUID == entry.GUID, "GUID mismatch.");
                        Assert(model.Type == "Access", "Type mismatch.");
                        Assert(model.Result == EventResult.Denied, "Result mismatch.");
                        Assert(model.ContentLength == 1024, "ContentLength mismatch.");
                        Assert(model.MaxAttempts == 3, "MaxAttempts mismatch.");
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "constructor_serializes_reference_fields", "Constructor serializes reference fields to JSON strings", _ =>
                    {
                        AuditLogEntry entry = new AuditLogEntry { Identity = "id", Source = "s", Target = "t", Resource = "r", Metadata = "m" };
                        AuditLogModel model = new AuditLogModel(entry);
                        Assert(!String.IsNullOrEmpty(model.Identity), "Identity should be serialized.");
                        Assert(!String.IsNullOrEmpty(model.Source), "Source should be serialized.");
                        Assert(!String.IsNullOrEmpty(model.Target), "Target should be serialized.");
                        Assert(!String.IsNullOrEmpty(model.Resource), "Resource should be serialized.");
                        Assert(!String.IsNullOrEmpty(model.Metadata), "Metadata should be serialized.");
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "constructor_assigns_guid_when_entry_guid_empty", "Constructor assigns a GUID when the entry GUID is empty", _ =>
                    {
                        AuditLogEntry entry = new AuditLogEntry { GUID = "" };
                        AuditLogModel model = new AuditLogModel(entry);
                        Assert(!String.IsNullOrEmpty(model.GUID), "Model GUID should be populated.");
                        Assert(!String.IsNullOrEmpty(entry.GUID), "Entry GUID should be back-filled.");
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "to_entry_round_trip_preserves_scalars", "ToAuditLogEntry preserves scalar fields", _ =>
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

                        Assert(restored.GUID == entry.GUID, "GUID mismatch.");
                        Assert(restored.Type == "Access", "Type mismatch.");
                        Assert(restored.Result == EventResult.Denied, "Result mismatch.");
                        Assert(restored.ContentLength == 1024, "ContentLength mismatch.");
                        Assert(restored.MaxAttempts == 3, "MaxAttempts mismatch.");
                        Assert(restored.Identity != null, "Identity should be restored.");
                        Assert(restored.Metadata != null, "Metadata should be restored.");
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "to_entry_null_reference_fields_stay_null", "ToAuditLogEntry leaves unset reference fields null", _ =>
                    {
                        AuditLogModel model = new AuditLogModel(new AuditLogEntry { Type = "Bare" });
                        AuditLogEntry restored = model.ToAuditLogEntry();
                        Assert(restored.Identity == null, "Identity should remain null.");
                        Assert(restored.Source == null, "Source should remain null.");
                        Assert(restored.Target == null, "Target should remain null.");
                        Assert(restored.Resource == null, "Resource should remain null.");
                        Assert(restored.Metadata == null, "Metadata should remain null.");
                        return Task.CompletedTask;
                    })
                });
        }

        #endregion

        #region EntryEventArgs

        private static TestSuiteDescriptor EventArgsSuite()
        {
            string suiteId = "event-args";
            return new TestSuiteDescriptor(
                suiteId,
                "EntryEventArgs construction",
                new List<TestCaseDescriptor>
                {
                    new TestCaseDescriptor(suiteId, "default_constructor", "Default constructor leaves members null", _ =>
                    {
                        EntryEventArgs args = new EntryEventArgs();
                        Assert(args.Entry == null, "Entry should be null.");
                        Assert(args.Target == null, "Target should be null.");
                        Assert(args.Exception == null, "Exception should be null.");
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "constructor_sets_entry_target_exception", "Constructor sets entry, target, and exception", _ =>
                    {
                        AuditLogEntry entry = new AuditLogEntry();
                        AuditLogTarget target = new AuditLogTarget("t", e => true);
                        Exception ex = new InvalidOperationException("boom");
                        EntryEventArgs args = new EntryEventArgs(entry, target, ex);
                        Assert(ReferenceEquals(args.Entry, entry), "Entry mismatch.");
                        Assert(ReferenceEquals(args.Target, target), "Target mismatch.");
                        Assert(ReferenceEquals(args.Exception, ex), "Exception mismatch.");
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "constructor_null_entry_throws", "Constructor rejects a null entry", _ =>
                    {
                        AssertThrows<ArgumentNullException>(() => new EntryEventArgs(null, null), "Null entry should throw.");
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "constructor_allows_null_target_and_exception", "Constructor allows null target and exception", _ =>
                    {
                        EntryEventArgs args = new EntryEventArgs(new AuditLogEntry(), null);
                        Assert(args.Entry != null, "Entry should be set.");
                        Assert(args.Target == null, "Target should be null.");
                        Assert(args.Exception == null, "Exception should be null.");
                        return Task.CompletedTask;
                    })
                });
        }

        #endregion

        #region EventResult

        private static TestSuiteDescriptor EventResultSuite()
        {
            string suiteId = "event-result";
            return new TestSuiteDescriptor(
                suiteId,
                "EventResult enum serialization",
                new List<TestCaseDescriptor>
                {
                    new TestCaseDescriptor(suiteId, "serializes_as_string_name", "Enum values serialize as their string name", _ =>
                    {
                        AuditLogEntry entry = new AuditLogEntry { Result = EventResult.Success };
                        string json = Common.SerializeJson(entry, false);
                        Assert(json.Contains("\"Success\""), "Enum should serialize as its string name.");
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "round_trips_through_json", "Every enum value round-trips through JSON", _ =>
                    {
                        foreach (EventResult value in new[] { EventResult.Unknown, EventResult.Success, EventResult.Failure, EventResult.Denied })
                        {
                            AuditLogEntry entry = new AuditLogEntry { Result = value };
                            string json = Common.SerializeJson(entry, false);
                            AuditLogEntry restored = Common.DeserializeJson<AuditLogEntry>(json);
                            Assert(restored.Result == value, "Enum round-trip failed for " + value + ".");
                        }
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "unknown_is_default", "Unknown is the default enum value", _ =>
                    {
                        Assert(default(EventResult) == EventResult.Unknown, "Default enum value should be Unknown.");
                        return Task.CompletedTask;
                    })
                });
        }

        #endregion

        #region Common + ObjectExtensions

        private static TestSuiteDescriptor SerializationSuite()
        {
            string suiteId = "serialization";
            return new TestSuiteDescriptor(
                suiteId,
                "Common serialization helpers and ObjectExtensions",
                new List<TestCaseDescriptor>
                {
                    new TestCaseDescriptor(suiteId, "serialize_null_returns_null", "Serializing null returns null", _ =>
                    {
                        Assert(Common.SerializeJson(null, false) == null, "Serializing null should return null.");
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "serialize_pretty_produces_indented", "Pretty serialization produces multi-line output", _ =>
                    {
                        string json = Common.SerializeJson(new AuditLogEntry { Type = "X" }, true);
                        Assert(json.Contains("\n"), "Pretty output should contain newlines.");
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "serialize_compact_no_newlines", "Compact serialization has no newlines", _ =>
                    {
                        string json = Common.SerializeJson(new AuditLogEntry { Type = "X" }, false);
                        Assert(!json.Contains("\n"), "Compact output should not contain newlines.");
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "serialize_omits_null_values", "Null members are omitted from serialized output", _ =>
                    {
                        string json = Common.SerializeJson(new AuditLogEntry(), false);
                        Assert(!json.Contains("\"Identity\""), "Null Identity should be omitted.");
                        Assert(!json.Contains("\"Metadata\""), "Null Metadata should be omitted.");
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "deserialize_null_string_throws", "Deserializing a null string throws", _ =>
                    {
                        AssertThrows<ArgumentNullException>(() => Common.DeserializeJson<AuditLogEntry>((string)null), "Null string should throw.");
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "deserialize_empty_string_throws", "Deserializing an empty string throws", _ =>
                    {
                        AssertThrows<ArgumentNullException>(() => Common.DeserializeJson<AuditLogEntry>(""), "Empty string should throw.");
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "deserialize_null_bytes_throws", "Deserializing a null byte array throws", _ =>
                    {
                        AssertThrows<ArgumentNullException>(() => Common.DeserializeJson<AuditLogEntry>((byte[])null), "Null bytes should throw.");
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "deserialize_empty_bytes_throws", "Deserializing an empty byte array throws", _ =>
                    {
                        AssertThrows<ArgumentNullException>(() => Common.DeserializeJson<AuditLogEntry>(new byte[0]), "Empty bytes should throw.");
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "deserialize_string_round_trip", "String serialize/deserialize round-trips", _ =>
                    {
                        AuditLogEntry entry = new AuditLogEntry { Type = "Login", Result = EventResult.Success };
                        string json = Common.SerializeJson(entry, false);
                        AuditLogEntry copy = Common.DeserializeJson<AuditLogEntry>(json);
                        Assert(copy.Type == entry.Type, "Type mismatch.");
                        Assert(copy.Result == entry.Result, "Result mismatch.");
                        Assert(copy.GUID == entry.GUID, "GUID mismatch.");
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "deserialize_bytes_round_trip", "Byte-array deserialization round-trips", _ =>
                    {
                        AuditLogEntry entry = new AuditLogEntry { Type = "Bytes" };
                        string json = Common.SerializeJson(entry, false);
                        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);
                        AuditLogEntry copy = Common.DeserializeJson<AuditLogEntry>(bytes);
                        Assert(copy.Type == "Bytes", "Type mismatch on byte round-trip.");
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "copy_object_produces_equivalent_copy", "CopyObject produces an equivalent copy", _ =>
                    {
                        AuditLogEntry entry = new AuditLogEntry { Type = "Copy", ContentLength = 7 };
                        AuditLogEntry copy = Common.CopyObject<AuditLogEntry>(entry);
                        Assert(copy != null, "Copy should not be null.");
                        Assert(copy.Type == entry.Type, "Type mismatch.");
                        Assert(copy.ContentLength == entry.ContentLength, "ContentLength mismatch.");
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "copy_object_null_returns_default", "CopyObject of null returns default", _ =>
                    {
                        Assert(Common.CopyObject<AuditLogEntry>(null) == null, "Copy of null should be null.");
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "to_json_serializes_object", "ObjectExtensions.ToJson serializes an object", _ =>
                    {
                        AuditLogEntry entry = new AuditLogEntry { Type = "TestEvent", ContentLength = 42 };
                        string json = entry.ToJson(false);
                        Assert(!String.IsNullOrEmpty(json), "JSON should not be empty.");
                        Assert(json.Contains("TestEvent"), "JSON should contain the type.");
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "to_json_pretty_produces_indented", "ObjectExtensions.ToJson pretty produces multi-line output", _ =>
                    {
                        string json = new AuditLogEntry { Type = "X" }.ToJson(true);
                        Assert(json.Contains("\n"), "Pretty ToJson should contain newlines.");
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "to_json_null_returns_null", "ObjectExtensions.ToJson on null returns null", _ =>
                    {
                        object nothing = null;
                        Assert(nothing.ToJson(false) == null, "ToJson on null should return null.");
                        return Task.CompletedTask;
                    })
                });
        }

        #endregion

        #region AuditLog validation (negative)

        private static TestSuiteDescriptor ValidationSuite()
        {
            string suiteId = "validation";
            return new TestSuiteDescriptor(
                suiteId,
                "AuditLog input validation (negative cases)",
                new List<TestCaseDescriptor>
                {
                    new TestCaseDescriptor(suiteId, "constructor_null_filename_throws", "Constructor rejects a null filename", _ =>
                    {
                        AssertThrows<ArgumentNullException>(() => new AuditLog(null), "Null filename should throw.");
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "interval_ms_below_minimum_throws", "IntervalMs rejects values below one", _ =>
                    {
                        using (TempAuditLog t = new TempAuditLog())
                        {
                            AssertThrows<ArgumentException>(() => t.Log.IntervalMs = 0, "IntervalMs = 0 should throw.");
                            AssertThrows<ArgumentException>(() => t.Log.IntervalMs = -1, "IntervalMs = -1 should throw.");
                        }
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "batch_size_below_minimum_throws", "BatchSize rejects values below one", _ =>
                    {
                        using (TempAuditLog t = new TempAuditLog())
                        {
                            AssertThrows<ArgumentException>(() => t.Log.BatchSize = 0, "BatchSize = 0 should throw.");
                            AssertThrows<ArgumentException>(() => t.Log.BatchSize = -5, "BatchSize = -5 should throw.");
                        }
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "max_attempts_below_minimum_throws", "MaxAttempts rejects values below one", _ =>
                    {
                        using (TempAuditLog t = new TempAuditLog())
                        {
                            AssertThrows<ArgumentException>(() => t.Log.MaxAttempts = 0, "MaxAttempts = 0 should throw.");
                            AssertThrows<ArgumentException>(() => t.Log.MaxAttempts = -3, "MaxAttempts = -3 should throw.");
                        }
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "add_target_null_throws", "AddTarget rejects a null target", _ =>
                    {
                        using (TempAuditLog t = new TempAuditLog())
                        {
                            AssertThrows<ArgumentNullException>(() => t.Log.AddTarget(null), "Null target should throw.");
                        }
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "add_target_empty_name_throws", "AddTarget rejects a target with an empty name", _ =>
                    {
                        using (TempAuditLog t = new TempAuditLog())
                        {
                            AuditLogTarget target = new AuditLogTarget("guid", "name", e => true);
                            target.Name = "";
                            AssertThrows<ArgumentNullException>(() => t.Log.AddTarget(target), "Empty name should throw.");
                        }
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "add_target_empty_guid_throws", "AddTarget rejects a target with an empty GUID", _ =>
                    {
                        using (TempAuditLog t = new TempAuditLog())
                        {
                            AuditLogTarget target = new AuditLogTarget("guid", "name", e => true);
                            target.GUID = "";
                            AssertThrows<ArgumentNullException>(() => t.Log.AddTarget(target), "Empty GUID should throw.");
                        }
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "add_target_null_action_throws", "AddTarget rejects a target with a null action", _ =>
                    {
                        using (TempAuditLog t = new TempAuditLog())
                        {
                            AuditLogTarget target = new AuditLogTarget("guid", "name", e => true);
                            target.Action = null;
                            AssertThrows<ArgumentNullException>(() => t.Log.AddTarget(target), "Null action should throw.");
                        }
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "add_target_duplicate_name_throws", "AddTarget rejects a duplicate name", _ =>
                    {
                        using (TempAuditLog t = new TempAuditLog())
                        {
                            t.Log.AddTarget(new AuditLogTarget("guid-1", "dup", e => true));
                            AssertThrows<ArgumentException>(() => t.Log.AddTarget(new AuditLogTarget("guid-2", "dup", e => true)), "Duplicate name should throw.");
                        }
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "add_target_duplicate_guid_throws", "AddTarget rejects a duplicate GUID", _ =>
                    {
                        using (TempAuditLog t = new TempAuditLog())
                        {
                            t.Log.AddTarget(new AuditLogTarget("guid-1", "name-a", e => true));
                            AssertThrows<ArgumentException>(() => t.Log.AddTarget(new AuditLogTarget("guid-1", "name-b", e => true)), "Duplicate GUID should throw.");
                        }
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "target_exists_by_name_null_throws", "TargetExistsByName rejects null", _ =>
                    {
                        using (TempAuditLog t = new TempAuditLog())
                        {
                            AssertThrows<ArgumentNullException>(() => t.Log.TargetExistsByName(null), "Null name should throw.");
                        }
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "target_exists_by_guid_empty_throws", "TargetExistsByGuid rejects an empty string", _ =>
                    {
                        using (TempAuditLog t = new TempAuditLog())
                        {
                            AssertThrows<ArgumentNullException>(() => t.Log.TargetExistsByGuid(""), "Empty GUID should throw.");
                        }
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "remove_target_null_throws", "RemoveTarget rejects null", _ =>
                    {
                        using (TempAuditLog t = new TempAuditLog())
                        {
                            AssertThrows<ArgumentNullException>(() => t.Log.RemoveTarget(null), "Null GUID should throw.");
                        }
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "add_event_null_throws", "AddEvent rejects a null entry", _ =>
                    {
                        using (TempAuditLog t = new TempAuditLog())
                        {
                            AssertThrows<ArgumentNullException>(() => t.Log.AddEvent(null), "Null entry should throw.");
                        }
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "add_event_max_attempts_below_one_throws", "AddEvent rejects a maxAttempts below one", _ =>
                    {
                        using (TempAuditLog t = new TempAuditLog())
                        {
                            AssertThrows<ArgumentException>(() => t.Log.AddEvent(new AuditLogEntry(), null, 0), "maxAttempts = 0 should throw.");
                        }
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "remove_event_null_guid_throws", "RemoveEvent rejects a null GUID", _ =>
                    {
                        using (TempAuditLog t = new TempAuditLog())
                        {
                            AssertThrows<ArgumentNullException>(() => t.Log.RemoveEvent(null), "Null GUID should throw.");
                        }
                        return Task.CompletedTask;
                    })
                });
        }

        #endregion

        #region AuditLog target management (positive)

        private static TestSuiteDescriptor TargetManagementSuite()
        {
            string suiteId = "targets";
            return new TestSuiteDescriptor(
                suiteId,
                "AuditLog target management",
                new List<TestCaseDescriptor>
                {
                    new TestCaseDescriptor(suiteId, "add_target_then_exists_by_name_and_guid", "A registered target is discoverable by name and GUID", _ =>
                    {
                        using (TempAuditLog t = new TempAuditLog())
                        {
                            t.Log.AddTarget(new AuditLogTarget("guid-abc", "primary", e => true));
                            Assert(t.Log.TargetExistsByName("primary"), "Target should exist by name.");
                            Assert(t.Log.TargetExistsByGuid("guid-abc"), "Target should exist by GUID.");
                            Assert(!t.Log.TargetExistsByName("does-not-exist"), "Unknown name should not exist.");
                            Assert(!t.Log.TargetExistsByGuid("nope"), "Unknown GUID should not exist.");
                        }
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "remove_target_removes_it", "RemoveTarget removes the target", _ =>
                    {
                        using (TempAuditLog t = new TempAuditLog())
                        {
                            t.Log.AddTarget(new AuditLogTarget("guid-xyz", "temp", e => true));
                            Assert(t.Log.TargetExistsByGuid("guid-xyz"), "Target should exist before removal.");
                            t.Log.RemoveTarget("guid-xyz");
                            Assert(!t.Log.TargetExistsByGuid("guid-xyz"), "Target should not exist after removal.");
                        }
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "remove_unknown_target_is_noop", "Removing an unknown target does not throw", _ =>
                    {
                        using (TempAuditLog t = new TempAuditLog())
                        {
                            t.Log.RemoveTarget("never-registered");
                        }
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "add_multiple_targets", "Multiple distinct targets can be registered", _ =>
                    {
                        using (TempAuditLog t = new TempAuditLog())
                        {
                            t.Log.AddTarget(new AuditLogTarget("guid-1", "one", e => true));
                            t.Log.AddTarget(new AuditLogTarget("guid-2", "two", e => true));
                            t.Log.AddTarget(new AuditLogTarget("guid-3", "three", e => true));
                            Assert(t.Log.TargetExistsByName("one"), "First target missing.");
                            Assert(t.Log.TargetExistsByName("two"), "Second target missing.");
                            Assert(t.Log.TargetExistsByName("three"), "Third target missing.");
                        }
                        return Task.CompletedTask;
                    })
                });
        }

        #endregion

        #region AuditLog events

        private static TestSuiteDescriptor EventSuite()
        {
            string suiteId = "events";
            return new TestSuiteDescriptor(
                suiteId,
                "AuditLog event submission",
                new List<TestCaseDescriptor>
                {
                    new TestCaseDescriptor(suiteId, "add_event_returns_guid", "AddEvent returns a GUID", _ =>
                    {
                        using (TempAuditLog t = new TempAuditLog())
                        {
                            t.Log.AddTarget(new AuditLogTarget("guid-1", "t1", e => true));
                            string guid = t.Log.AddEvent(new AuditLogEntry { Type = "Ping" });
                            Assert(!String.IsNullOrEmpty(guid), "AddEvent should return a GUID.");
                        }
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "add_event_no_targets_returns_guid", "AddEvent with no targets still returns a GUID", _ =>
                    {
                        using (TempAuditLog t = new TempAuditLog())
                        {
                            string guid = t.Log.AddEvent(new AuditLogEntry { Type = "Orphan" });
                            Assert(!String.IsNullOrEmpty(guid), "AddEvent should return a GUID even with no targets.");
                        }
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "add_event_assigns_guid_when_missing", "AddEvent assigns a GUID when the entry GUID is empty", _ =>
                    {
                        using (TempAuditLog t = new TempAuditLog())
                        {
                            t.Log.AddTarget(new AuditLogTarget("guid-1", "t1", e => true));
                            string guid = t.Log.AddEvent(new AuditLogEntry { GUID = "" });
                            Assert(!String.IsNullOrEmpty(guid), "AddEvent should assign a GUID.");
                        }
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "add_event_respects_supplied_guid", "AddEvent returns the caller-supplied GUID", _ =>
                    {
                        using (TempAuditLog t = new TempAuditLog())
                        {
                            t.Log.AddTarget(new AuditLogTarget("guid-1", "t1", e => true));
                            string guid = t.Log.AddEvent(new AuditLogEntry { GUID = "my-guid" });
                            Assert(guid == "my-guid", "AddEvent should preserve the supplied GUID.");
                        }
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "add_event_max_attempts_override_accepted", "AddEvent accepts a per-event maxAttempts override", _ =>
                    {
                        using (TempAuditLog t = new TempAuditLog())
                        {
                            t.Log.AddTarget(new AuditLogTarget("guid-1", "t1", e => true));
                            string guid = t.Log.AddEvent(new AuditLogEntry { Type = "Override" }, null, 3);
                            Assert(!String.IsNullOrEmpty(guid), "AddEvent with override should return a GUID.");
                        }
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "remove_event_does_not_throw", "RemoveEvent for an unknown GUID does not throw", _ =>
                    {
                        using (TempAuditLog t = new TempAuditLog())
                        {
                            t.Log.RemoveEvent(Guid.NewGuid().ToString());
                            t.Log.RemoveEvent(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
                        }
                        return Task.CompletedTask;
                    })
                });
        }

        #endregion

        #region Emitter (integration)

        private static TestSuiteDescriptor EmitterSuite()
        {
            string suiteId = "emitter";
            return new TestSuiteDescriptor(
                suiteId,
                "Emitter end-to-end behavior",
                new List<TestCaseDescriptor>
                {
                    new TestCaseDescriptor(suiteId, "delivers_event_to_successful_target", "The emitter delivers an event to a successful target", _ =>
                    {
                        using (TempAuditLog t = new TempAuditLog())
                        {
                            ConcurrentBag<AuditLogEntry> delivered = new ConcurrentBag<AuditLogEntry>();
                            t.Log.AddTarget(new AuditLogTarget("guid-ok", "ok", e =>
                            {
                                delivered.Add(e);
                                return true;
                            }));

                            t.Log.AddEvent(new AuditLogEntry { Type = "DeliveredEvent" });

                            bool got = Wait.Until(() => delivered.Count >= 1);
                            Assert(got, "Event was not delivered to the target within the timeout.");

                            bool found = false;
                            foreach (AuditLogEntry e in delivered)
                            {
                                if (e.Type == "DeliveredEvent") { found = true; break; }
                            }
                            Assert(found, "Delivered entry did not carry the expected type.");
                        }
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "only_delivers_to_specified_targets", "The emitter routes an event only to specified targets", _ =>
                    {
                        using (TempAuditLog t = new TempAuditLog())
                        {
                            int hitA = 0;
                            int hitB = 0;
                            t.Log.AddTarget(new AuditLogTarget("guid-a", "a", e => { Interlocked.Increment(ref hitA); return true; }));
                            t.Log.AddTarget(new AuditLogTarget("guid-b", "b", e => { Interlocked.Increment(ref hitB); return true; }));

                            t.Log.AddEvent(new AuditLogEntry { Type = "Routed" }, new List<string> { "guid-a" });

                            bool delivered = Wait.Until(() => Volatile.Read(ref hitA) >= 1);
                            Assert(delivered, "Event was not delivered to target A.");

                            Thread.Sleep(300);
                            Assert(Volatile.Read(ref hitB) == 0, "Target B should never have been hit.");
                        }
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "retries_then_evicts_failing_entry", "A failing entry is retried then evicted after exceeding max attempts", _ =>
                    {
                        using (TempAuditLog t = new TempAuditLog())
                        {
                            int failureEvents = 0;
                            int evictedEvents = 0;
                            t.Log.EntrySendFailure += (s, a) => Interlocked.Increment(ref failureEvents);
                            t.Log.EntryEvicted += (s, a) => Interlocked.Increment(ref evictedEvents);

                            t.Log.AddTarget(new AuditLogTarget("guid-fail", "always-fails", e => false));
                            t.Log.AddEvent(new AuditLogEntry { Type = "WillFail" }, null, 2);

                            bool evicted = Wait.Until(() => Volatile.Read(ref evictedEvents) >= 1);
                            Assert(evicted, "Entry was never evicted after exceeding max attempts.");
                            Assert(Volatile.Read(ref failureEvents) >= 2, "Expected at least two failure notifications before eviction, got " + failureEvents + ".");
                        }
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "exception_in_target_raises_send_failure_and_survives", "An exception thrown by a target raises a send failure and the emitter survives", _ =>
                    {
                        using (TempAuditLog t = new TempAuditLog())
                        {
                            int failureEvents = 0;
                            t.Log.EntrySendFailure += (s, a) => Interlocked.Increment(ref failureEvents);

                            t.Log.AddTarget(new AuditLogTarget("guid-throw", "throws", e =>
                                throw new InvalidOperationException("boom")));

                            t.Log.AddEvent(new AuditLogEntry { Type = "Throws" });

                            bool raised = Wait.Until(() => Volatile.Read(ref failureEvents) >= 1);
                            Assert(raised, "Exception in target did not raise EntrySendFailure.");

                            bool keptRunning = Wait.Until(() => Volatile.Read(ref failureEvents) >= 3, 5000);
                            Assert(keptRunning, "Emitter stopped processing after a target threw.");
                        }
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "logger_receives_messages", "A configured logger receives diagnostic messages", _ =>
                    {
                        using (TempAuditLog t = new TempAuditLog())
                        {
                            ConcurrentBag<string> logs = new ConcurrentBag<string>();
                            t.Log.Logger = msg => logs.Add(msg);

                            t.Log.AddTarget(new AuditLogTarget("guid-log", "log-target", e => true));

                            bool logged = Wait.Until(() => logs.Count >= 1);
                            Assert(logged, "Logger did not receive any messages after adding a target.");
                        }
                        return Task.CompletedTask;
                    }),
                    new TestCaseDescriptor(suiteId, "remove_target_stops_delivery", "Removing a target stops further delivery attempts for its events", async token =>
                    {
                        using (TempAuditLog t = new TempAuditLog())
                        {
                            int failures = 0;
                            t.Log.EntrySendFailure += (s, a) => Interlocked.Increment(ref failures);

                            t.Log.AddTarget(new AuditLogTarget("guid-rm", "removable", e => false));
                            t.Log.AddEvent(new AuditLogEntry { Type = "Pending" }, null, 100);

                            bool sawFailure = Wait.Until(() => Volatile.Read(ref failures) >= 1);
                            Assert(sawFailure, "Expected at least one failure before removing the target.");

                            t.Log.RemoveTarget("guid-rm");
                            await Task.Delay(200, token).ConfigureAwait(false);
                            int snapshot = Volatile.Read(ref failures);
                            await Task.Delay(400, token).ConfigureAwait(false);

                            Assert(Volatile.Read(ref failures) == snapshot, "Failures continued after the target and its events were removed.");
                        }
                    })
                });
        }

        #endregion

        #region Assertions

        private static void Assert(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }

        private static void AssertThrows<T>(Action action, string message)
            where T : Exception
        {
            try
            {
                action();
            }
            catch (T)
            {
                return;
            }

            throw new InvalidOperationException(message);
        }

        #endregion
    }
}
