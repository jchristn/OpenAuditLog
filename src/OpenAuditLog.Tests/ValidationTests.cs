namespace OpenAuditLog.Tests
{
    using System;
    using System.Collections.Generic;
    using OpenAuditLog;
    using Xunit;

    /// <summary>
    /// Negative test cases: verify that invalid inputs are rejected.
    /// </summary>
    public class ValidationTests
    {
        [Fact]
        public void Constructor_NullFilename_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new AuditLog(null));
        }

        [Fact]
        public void IntervalMs_BelowMinimum_Throws()
        {
            using TempAuditLog t = new TempAuditLog();
            Assert.Throws<ArgumentException>(() => t.Log.IntervalMs = 0);
            Assert.Throws<ArgumentException>(() => t.Log.IntervalMs = -1);
        }

        [Fact]
        public void BatchSize_BelowMinimum_Throws()
        {
            using TempAuditLog t = new TempAuditLog();
            Assert.Throws<ArgumentException>(() => t.Log.BatchSize = 0);
            Assert.Throws<ArgumentException>(() => t.Log.BatchSize = -5);
        }

        [Fact]
        public void MaxAttempts_BelowMinimum_Throws()
        {
            using TempAuditLog t = new TempAuditLog();
            Assert.Throws<ArgumentException>(() => t.Log.MaxAttempts = 0);
            Assert.Throws<ArgumentException>(() => t.Log.MaxAttempts = -3);
        }

        [Fact]
        public void AddTarget_Null_Throws()
        {
            using TempAuditLog t = new TempAuditLog();
            Assert.Throws<ArgumentNullException>(() => t.Log.AddTarget(null));
        }

        [Fact]
        public void AddTarget_DuplicateName_Throws()
        {
            using TempAuditLog t = new TempAuditLog();
            t.Log.AddTarget(new AuditLogTarget("guid-1", "dup", e => true));
            Assert.Throws<ArgumentException>(() =>
                t.Log.AddTarget(new AuditLogTarget("guid-2", "dup", e => true)));
        }

        [Fact]
        public void AddTarget_DuplicateGuid_Throws()
        {
            using TempAuditLog t = new TempAuditLog();
            t.Log.AddTarget(new AuditLogTarget("guid-1", "name-a", e => true));
            Assert.Throws<ArgumentException>(() =>
                t.Log.AddTarget(new AuditLogTarget("guid-1", "name-b", e => true)));
        }

        [Fact]
        public void TargetExistsByName_Null_Throws()
        {
            using TempAuditLog t = new TempAuditLog();
            Assert.Throws<ArgumentNullException>(() => t.Log.TargetExistsByName(null));
        }

        [Fact]
        public void TargetExistsByGuid_Null_Throws()
        {
            using TempAuditLog t = new TempAuditLog();
            Assert.Throws<ArgumentNullException>(() => t.Log.TargetExistsByGuid(""));
        }

        [Fact]
        public void RemoveTarget_Null_Throws()
        {
            using TempAuditLog t = new TempAuditLog();
            Assert.Throws<ArgumentNullException>(() => t.Log.RemoveTarget(null));
        }

        [Fact]
        public void AddEvent_Null_Throws()
        {
            using TempAuditLog t = new TempAuditLog();
            Assert.Throws<ArgumentNullException>(() => t.Log.AddEvent(null));
        }

        [Fact]
        public void AddEvent_MaxAttemptsBelowOne_Throws()
        {
            using TempAuditLog t = new TempAuditLog();
            Assert.Throws<ArgumentException>(() =>
                t.Log.AddEvent(new AuditLogEntry(), null, 0));
        }

        [Fact]
        public void RemoveEvent_NullGuid_Throws()
        {
            using TempAuditLog t = new TempAuditLog();
            Assert.Throws<ArgumentNullException>(() => t.Log.RemoveEvent(null));
        }

        [Fact]
        public void Target_Constructor_NullName_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new AuditLogTarget(null, e => true));
        }

        [Fact]
        public void Target_Constructor_NullAction_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new AuditLogTarget("name", null));
        }

        [Fact]
        public void Target_Constructor_NullGuid_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new AuditLogTarget(null, "name", e => true));
        }

        [Fact]
        public void Model_Constructor_NullEntry_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new AuditLogModel(null));
        }

        [Fact]
        public void EntryEventArgs_NullEntry_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new EntryEventArgs(null, null));
        }
    }
}
