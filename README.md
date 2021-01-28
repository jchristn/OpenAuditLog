![alt tag](https://github.com/jchristn/openauditlog/blob/main/assets/logo.ico)

# OpenAuditLog

[![NuGet Version](https://img.shields.io/nuget/v/OpenAuditLog.svg?style=flat)](https://www.nuget.org/packages/OpenAuditLog/) [![NuGet](https://img.shields.io/nuget/dt/OpenAuditLog.svg)](https://www.nuget.org/packages/OpenAuditLog) 

Simple C# event and audit log library with persistence allowing you to write your own emitters.

## How it Works

1) Instantiate the ```AuditLog``` object, specifying the name of the Sqlite database file to use.

- Set ```Logger``` if you wish to output messages to the console or elsewhere
- Also serves as a "logger of last resort" in case an event can't be sent to a target

2) Define and add your targets using ```AddTarget``` and the ```AuditLogTarget``` class.

- Your target function should return a boolean with ```true``` indicating success...
- ...or ```false``` indicating failure and that OpenAuditLog should try again

3) Add events using ```AddEvent``` and watch them fly.

- Events added through ```AddEvent``` are persisted in Sqlite until emitted successfully
- If multiple targets are configured, one record per target will be created for each event
- Control the number of attempts to send an event:
  - By modifying ```Retries```
  - Or specify the optional parameter ```maxAttempts``` in ```AddEvent```
  - Those that cannot be sent successfully will be removed from the queue...
  - ...and sent to the ```Logger``` method

## Simple Example

Refer to the ```Test``` project for a full example.

```csharp
AuditLog auditLog = new AuditLog("auditlog.db");
auditLog.Logger = Console.WriteLine;
auditLog.MaxAttempts = 3;

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

auditLog.AddTarget(new AuditLogTarget("target1", func1)); // these will succeed
auditLog.AddTarget(new AuditLogTarget("target2", func2)); // these will fail
auditLog.AddEvent(new AuditLogEntry("identity", "source", "target", "resource", "handle", "eventType", EventResult.Success, 0, "{'foo'='bar'}"));
```

## Version History

Refer to ```CHANGELOG.md``` for version history.
