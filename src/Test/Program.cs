namespace Test
{
    using System;
    using System.Collections.Generic;
    using OpenAuditLog;

    class Program
    {
        static AuditLog _AuditLog;

        static void Main(string[] args)
        {
            _AuditLog = new AuditLog("auditlog.db");
            _AuditLog.Logger = Console.WriteLine;
            _AuditLog.MaxAttempts = 5;
            _AuditLog.IntervalMs = 1000;
            _AuditLog.EntrySendFailure += EntrySendFailure;
            _AuditLog.EntryEvicted += EntryEvicted;

            Func<AuditLogEntry, bool> func1 = delegate (AuditLogEntry a)
            {
                Console.WriteLine("Action 1: " + a.ToJson(false));
                return true; // report success
            };

            Func<AuditLogEntry, bool> func2 = delegate (AuditLogEntry a)
            {
                Console.WriteLine("Action 2: " + a.ToJson(false));
                return false; // report failure
            };

            _AuditLog.AddTarget(new AuditLogTarget("test1", func1));
            _AuditLog.AddTarget(new AuditLogTarget("test2", func2));

            for (int i = 0; i < 5; i++)
            {
                #region Full-Object

                Person identity = new Person("Person", i.ToString());

                Dictionary<string, object> source = new Dictionary<string, object>();
                source.Add("SourceGUID", Guid.NewGuid());
                source.Add("SourceID", i);

                Dictionary<string, object> target = new Dictionary<string, object>();
                target.Add("TargetGUID", Guid.NewGuid());
                target.Add("TargetID", i);

                Dictionary<string, object> resource = new Dictionary<string, object>();
                resource.Add("ResourceGUID", Guid.NewGuid());
                resource.Add("ResourceID", i);

                Dictionary<string, object> metadata = new Dictionary<string, object>();
                metadata.Add("MetadataGUID", Guid.NewGuid());
                metadata.Add("MetadataID", i);

                AuditLogEntry entry1 = new AuditLogEntry(
                    identity,
                    source,
                    target,
                    resource,
                    metadata,
                    "TestEvent",
                    EventResult.Success,
                    100);

                #endregion

                #region Simple-Object

                AuditLogEntry entry2 = new AuditLogEntry();
                entry2.Metadata = "Hello, world!";

                #endregion

                _AuditLog.AddEvent(entry1);
                _AuditLog.AddEvent(entry2);
            }

            Console.WriteLine("Press ENTER to exit");
            Console.ReadLine();
        }

        static void EntrySendFailure(object sender, EntryEventArgs args)
        {
            Console.WriteLine("Failed sending event " + args.Entry.GUID + " to target " + args.Target.GUID);
        }

        static void EntryEvicted(object sender, EntryEventArgs args)
        {
            Console.WriteLine("Evicted event " + args.Entry.GUID + " due to excessive failures");
        }
    }

    class Person
    {
        public string First { get; set; } = null;
        public string Last { get; set; } = null;

        public Person()
        {

        }

        public Person(string first, string last)
        {
            First = first;
            Last = last;
        }
    }
}
