using System;
using System.Collections.Generic;
using OpenAuditLog;

namespace Test
{
    class Program
    {
        static AuditLog _AuditLog;

        static void Main(string[] args)
        {
            _AuditLog = new AuditLog("auditlog.db");
            _AuditLog.Logger = Console.WriteLine;

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
                Dictionary<string, string> md = new Dictionary<string, string>();
                md.Add("foo", "bar");

                AuditLogEntry entry = new AuditLogEntry(md);
                _AuditLog.AddEvent(entry, null, 2);
            }

            Console.WriteLine("Press ENTER to exit");
            Console.ReadLine();
        }
    }
}
