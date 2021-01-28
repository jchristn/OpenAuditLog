using System;
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
                _AuditLog.AddEvent(new AuditLogEntry("identity", "source", "target", "resource", "handle", "eventType", EventResult.Success, 0, "{'foo'='bar'}"), null, 2);
            }

            Console.WriteLine("Press ENTER to exit");
            Console.ReadLine();
        }
    }
}
