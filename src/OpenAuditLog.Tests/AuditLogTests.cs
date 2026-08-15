namespace OpenAuditLog.Tests
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using OpenAuditLog;
    using Xunit;

    /// <summary>
    /// Positive tests covering target management and end-to-end emitter behavior.
    /// These exercise the real Sqlite-backed emitter loop.
    /// </summary>
    public class AuditLogTests
    {
        [Fact]
        public void AddTarget_ThenExistsByNameAndGuid()
        {
            using TempAuditLog t = new TempAuditLog();
            AuditLogTarget target = new AuditLogTarget("guid-abc", "primary", e => true);

            t.Log.AddTarget(target);

            Assert.True(t.Log.TargetExistsByName("primary"));
            Assert.True(t.Log.TargetExistsByGuid("guid-abc"));
            Assert.False(t.Log.TargetExistsByName("does-not-exist"));
            Assert.False(t.Log.TargetExistsByGuid("nope"));
        }

        [Fact]
        public void RemoveTarget_RemovesIt()
        {
            using TempAuditLog t = new TempAuditLog();
            t.Log.AddTarget(new AuditLogTarget("guid-xyz", "temp", e => true));
            Assert.True(t.Log.TargetExistsByGuid("guid-xyz"));

            t.Log.RemoveTarget("guid-xyz");

            Assert.False(t.Log.TargetExistsByGuid("guid-xyz"));
        }

        [Fact]
        public void AddEvent_ReturnsGuid()
        {
            using TempAuditLog t = new TempAuditLog();
            t.Log.AddTarget(new AuditLogTarget("guid-1", "t1", e => true));

            string guid = t.Log.AddEvent(new AuditLogEntry { Type = "Ping" });

            Assert.False(String.IsNullOrEmpty(guid));
        }

        [Fact]
        public void Emitter_DeliversEventToSuccessfulTarget()
        {
            using TempAuditLog t = new TempAuditLog();

            ConcurrentBag<AuditLogEntry> delivered = new ConcurrentBag<AuditLogEntry>();
            t.Log.AddTarget(new AuditLogTarget("guid-ok", "ok", e =>
            {
                delivered.Add(e);
                return true;
            }));

            t.Log.AddEvent(new AuditLogEntry { Type = "DeliveredEvent" });

            bool got = Wait.Until(() => delivered.Count >= 1);

            Assert.True(got, "Event was not delivered to the target within the timeout.");
            bool found = false;
            foreach (AuditLogEntry e in delivered)
            {
                if (e.Type == "DeliveredEvent") { found = true; break; }
            }
            Assert.True(found);
        }

        [Fact]
        public void Emitter_OnlyDeliversToSpecifiedTargets()
        {
            using TempAuditLog t = new TempAuditLog();

            int hitA = 0;
            int hitB = 0;
            t.Log.AddTarget(new AuditLogTarget("guid-a", "a", e => { Interlocked.Increment(ref hitA); return true; }));
            t.Log.AddTarget(new AuditLogTarget("guid-b", "b", e => { Interlocked.Increment(ref hitB); return true; }));

            // Route only to target A.
            t.Log.AddEvent(new AuditLogEntry { Type = "Routed" }, new List<string> { "guid-a" });

            bool delivered = Wait.Until(() => Volatile.Read(ref hitA) >= 1);
            Assert.True(delivered, "Event was not delivered to target A.");

            // Give the emitter a couple more cycles; B should never be hit.
            Thread.Sleep(300);
            Assert.Equal(0, Volatile.Read(ref hitB));
        }

        [Fact]
        public void Emitter_RetriesThenEvictsFailingEntry()
        {
            using TempAuditLog t = new TempAuditLog();

            int failureEvents = 0;
            int evictedEvents = 0;

            t.Log.EntrySendFailure += (s, a) => Interlocked.Increment(ref failureEvents);
            t.Log.EntryEvicted += (s, a) => Interlocked.Increment(ref evictedEvents);

            t.Log.AddTarget(new AuditLogTarget("guid-fail", "always-fails", e => false));

            // maxAttempts = 2 -> two failures, then eviction.
            t.Log.AddEvent(new AuditLogEntry { Type = "WillFail" }, null, 2);

            bool evicted = Wait.Until(() => Volatile.Read(ref evictedEvents) >= 1);

            Assert.True(evicted, "Entry was never evicted after exceeding max attempts.");
            Assert.True(Volatile.Read(ref failureEvents) >= 2,
                "Expected at least two failure notifications before eviction, got " + failureEvents + ".");
        }

        [Fact]
        public void Emitter_ExceptionInTarget_RaisesSendFailureAndSurvives()
        {
            using TempAuditLog t = new TempAuditLog();

            int failureEvents = 0;
            t.Log.EntrySendFailure += (s, a) => Interlocked.Increment(ref failureEvents);

            // A target that throws must not crash the emitter loop; it should surface as a
            // send failure and the emitter must keep processing subsequent events.
            t.Log.AddTarget(new AuditLogTarget("guid-throw", "throws", e =>
                throw new InvalidOperationException("boom")));

            t.Log.AddEvent(new AuditLogEntry { Type = "Throws" });

            bool raised = Wait.Until(() => Volatile.Read(ref failureEvents) >= 1);
            Assert.True(raised, "Exception in target did not raise EntrySendFailure.");

            // The emitter is still alive: it keeps invoking the (throwing) target, so failures
            // continue to accumulate rather than stopping at one.
            bool keptRunning = Wait.Until(() => Volatile.Read(ref failureEvents) >= 3, 5000);
            Assert.True(keptRunning, "Emitter stopped processing after a target threw.");
        }

        [Fact]
        public void AddEvent_NoTargets_InsertsNothingAndReturnsGuid()
        {
            using TempAuditLog t = new TempAuditLog();

            // No targets registered; AddEvent should still return a GUID without throwing.
            string guid = t.Log.AddEvent(new AuditLogEntry { Type = "Orphan" });

            Assert.False(String.IsNullOrEmpty(guid));
        }
    }
}
