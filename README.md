![alt tag](https://github.com/jchristn/openauditlog/blob/main/assets/logo.ico)

# OpenAuditLog

[![NuGet Version](https://img.shields.io/nuget/v/OpenAuditLog.svg?style=flat)](https://www.nuget.org/packages/OpenAuditLog/) [![NuGet](https://img.shields.io/nuget/dt/OpenAuditLog.svg)](https://www.nuget.org/packages/OpenAuditLog) 

Simple C# event and audit log library with persistence allowing you to write your own emitters.

## How it Works

### Instantiate

Instantiate the ```AuditLog``` object, specifying the name of the Sqlite database file to use.

```csharp
using OpenAuditLog;

AuditLog log = new AuditLog("auditlog.db");
```

### Define Event Targets

Define and add your targets using ```AddTarget``` and the ```AuditLogTarget``` class.  Your target function should return a boolean with ```true``` indicating success, or, ```false``` indicating failure and that OpenAuditLog should try again.

```csharp
Func<AuditLogEntry, bool> func1 = delegate (AuditLogEntry a)
{
  Console.WriteLine("Action 1: " + a.ToJson(false));
  return true; // report success
};

log.AddTarget("MyTarget", func1);
```

### Logger of Last Resort

Set ```Logger``` if you wish to output messages to the console or elsewhere.  This also serves as a "logger of last resort" in case an event can't be sent to a target.  Additionally, you can create handlers for the ```EntrySendFailure``` and ```EntryEvicted``` events.

```csharp
log.Logger = Console.WriteLine;
log.EntrySendFailure += MyEntrySendFailure;
log.EntryEvicted += MyEntryEvicted;

static void MyEntrySendFailure(object sender, EntryEventArgs args)
{
  Console.WriteLine("Entry " + args.Entry.GUID + " couldn't be sent to target " + args.Target.GUID);
}

static void MyEntryEvicted(object sender, EntryEventArgs args)
{
  Console.WriteLine("Entry " + args.Entry.GUID + " was evicted, failed too hard!");
}
```

### Let 'Er Rip

Add events using ```AddEvent``` and watch them fly.

Events added through ```AddEvent``` are persisted in Sqlite until emitted successfully.  If multiple targets are configured, one record per target will be created for each event.  You can control the number of attempts to send an event by modifying ```AuditLog.MaxAttempts``` or specify the optional parameter ```maxAttempts``` in ```AuditLog.AddEvent```.  

Events sent successfully are removed from Sqlite, and those that have failed the specified number of times will also be removed.  Refer to "Logger of Last Resort" above to determine how to intercept such situations.

```csharp
// build it yourself
AuditLogEntry myEntry = new AuditLogEntry();
myEntry.Metadata = myClassInstance;
// also set Identity, Source, Target, Resource, Type, etc, if you like
```

```csharp
// or use the giga-constructor
AuditLogEntry myEntry = new AuditLogEntry(metadata, identity, source, ...);
```

Fire away!

```csharp
log.AddEvent(myEntry);
```

## Simple Example

Refer to the ```Test``` project for a full example.

```csharp
AuditLog auditLog = new AuditLog("auditlog.db");
auditLog.Logger = Console.WriteLine;
auditLog.MaxAttempts = 3;

Func<AuditLogEntry, bool> myTarget = delegate (AuditLogEntry a)
{
    Console.WriteLine("My target: " + a.ToJson(false));
    return true; 
};

// add the target
auditLog.AddTarget(new AuditLogTarget("target1", myTarget));

// create the event
AuditLogEntry entry = new AuditLogEntry();
entry.Metadata = myClassInstance;
// also set Identity, Source, Target, Resource, Type, etc

// fire away!
auditLog.AddEvent(entry);
```

## Version History

Refer to ```CHANGELOG.md``` for version history.
