using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Watson.ORM.Core;
using Watson.ORM.Sqlite;
using Newtonsoft.Json;

namespace OpenAuditLog
{
    /// <summary>
    /// OpenAuditLog.
    /// </summary>
    public class AuditLog : IDisposable
    {
        #region Public-Members

        /// <summary>
        /// Method to invoke to send log messages.
        /// </summary>
        public Action<string> Logger { get; set; } = null;
         
        /// <summary>
        /// The interval, in milliseconds, with which OpenAuditLog will attempt to send events to configured targets.
        /// Default value is 5000ms.
        /// Minimum value is 1.
        /// </summary>
        public int IntervalMs
        {
            get
            {
                return _IntervalMs;
            }
            set
            {
                if (value < 1) throw new ArgumentException("IntervalMs must be greater than zero.");
                _IntervalMs = value;
            }
        }

        /// <summary>
        /// The maximum number of events to retrieve and process in each interval.
        /// Default value is 100.
        /// Minimum value is 1.
        /// </summary>
        public int BatchSize
        {
            get
            {
                return _BatchSize;
            }
            set
            {
                if (value < 1) throw new ArgumentException("BatchSize must be greater than zero.");
                _BatchSize = value;
            }
        }

        /// <summary>
        /// The maximum number of times to retry sending an event to a target.  
        /// Default value is 5.
        /// Minimum value is 1.
        /// </summary>
        public int MaxAttempts
        {
            get
            {
                return _MaxAttempts;
            }
            set
            {
                if (value < 1) throw new ArgumentException("Retries must be one or greater.");
                _MaxAttempts = value;
            }
        }

        #endregion

        #region Private-Members

        private string _Header = "[OpenAuditLog] ";
        private int _IntervalMs = 5000;
        private int _BatchSize = 100;
        private int _MaxAttempts = 5;

        private string _DatabaseFilename = null;
        private WatsonORM _ORM = null;

        private readonly object _Lock = new object();
        private List<AuditLogTarget> _Targets = new List<AuditLogTarget>();

        private CancellationTokenSource _TokenSource = new CancellationTokenSource();
        private CancellationToken _Token;
        private Task _Emitter = null;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="filename">Sqlite database filename.</param>
        public AuditLog(string filename)
        {
            _DatabaseFilename = filename ?? throw new ArgumentNullException(nameof(filename));

            _ORM = new WatsonORM(new DatabaseSettings(_DatabaseFilename));
            _ORM.InitializeDatabase();
            _ORM.InitializeTable(typeof(AuditLogEntry));

            _Token = _TokenSource.Token;
            _Emitter = Task.Run(() => Emitter(_Token), _Token);
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Dispose of the object.
        /// </summary>
        public void Dispose()
        {
            Log("disposing");

            _TokenSource.Cancel();
        }

        /// <summary>
        /// Add an event target.
        /// </summary>
        /// <param name="target">Target.</param>
        public void AddTarget(AuditLogTarget target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (String.IsNullOrEmpty(target.Name)) throw new ArgumentNullException(nameof(AuditLogTarget.Name));
            if (String.IsNullOrEmpty(target.GUID)) throw new ArgumentNullException(nameof(AuditLogTarget.GUID));
            if (target.Action == null) throw new ArgumentNullException(nameof(AuditLogTarget.Action));

            lock (_Lock)
            {
                if (_Targets != null && _Targets.Count > 0)
                {
                    AuditLogTarget test = _Targets.FirstOrDefault(t => t.Name.Equals(target.Name));
                    if (test != null && test != default(AuditLogTarget))
                    {
                        throw new ArgumentException("Target with name '" + target.Name + "' already exists.");
                    }

                    test = _Targets.FirstOrDefault(t => t.GUID.Equals(target.GUID));
                    if (test != null && test != default(AuditLogTarget))
                    {
                        throw new ArgumentException("Target with GUID '" + target.GUID + "' already exists.");
                    }
                }

                _Targets.Add(target);
            }

            Log("added target " + target.Name + " GUID " + target.GUID);
        }

        /// <summary>
        /// Check if a target exists by name.
        /// </summary>
        /// <param name="name">Name of the target.</param>
        /// <returns>True if exists.</returns>
        public bool TargetExistsByName(string name)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

            lock (_Lock)
            {
                if (_Targets != null && _Targets.Count > 0)
                {
                    AuditLogTarget test = _Targets.FirstOrDefault(t => t.Name.Equals(name));
                    if (test != null && test != default(AuditLogTarget)) return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Check if a target exists by GUID.
        /// </summary>
        /// <param name="guid">GUID of the target.</param>
        /// <returns>True if exists.</returns>
        public bool TargetExistsByGuid(string guid)
        {
            if (String.IsNullOrEmpty(guid)) throw new ArgumentNullException(nameof(guid));

            lock (_Lock)
            {
                if (_Targets != null && _Targets.Count > 0)
                {
                    AuditLogTarget test = _Targets.FirstOrDefault(t => t.GUID.Equals(guid));
                    if (test != null && test != default(AuditLogTarget)) return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Remove an event target.
        /// This will also delete any events destined to the specified target.
        /// </summary>
        /// <param name="guid">GUID of the target.</param>
        public void RemoveTarget(string guid)
        {
            if (String.IsNullOrEmpty(guid)) throw new ArgumentNullException(nameof(guid));
             
            lock (_Lock)
            {
                if (_Targets.Any(t => t.GUID.Equals(guid)))
                {
                    DbExpression e = new DbExpression(_ORM.GetColumnName<AuditLogEntry>(nameof(AuditLogEntry.TargetGUID)), DbOperators.Equals, guid);
                    _ORM.DeleteMany<AuditLogEntry>(e);

                    List<AuditLogTarget> remove = _Targets.Where(t => t.GUID.Equals(guid)).ToList();
                    foreach (AuditLogTarget target in remove)
                    {
                        _Targets.Remove(target);
                       
                        Log("removed target " + target.Name + " GUID " + target.GUID);
                    }
                }
            }
        }

        /// <summary>
        /// Add an event.
        /// </summary>
        /// <param name="entry">Audit log entry.</param>
        /// <param name="targetGuids">List of target GUIDs to which the event should be sent.  Leave null to send to all targets.</param>
        /// <param name="maxAttempts">The maximum number of attempts to make in sending this event.  Leave null to use the value from the 'Retries' property.</param>
        public void AddEvent(AuditLogEntry entry, List<string> targetGuids = null, int? maxAttempts = null)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));

            if (maxAttempts != null)
            {
                if (maxAttempts.Value < 1) throw new ArgumentException("The value for 'maxAttempts' must be one or greater.");
                entry.MaxAttempts = maxAttempts.Value;
            }
            else
            {
                entry.MaxAttempts = _MaxAttempts;
            }

            lock (_Lock)
            {
                if (targetGuids != null)
                {
                    foreach (string targetGuid in targetGuids)
                    {
                        entry.TargetGUID = targetGuid;
                        _ORM.Insert<AuditLogEntry>(entry);
                    }
                }
                else
                {
                    foreach (AuditLogTarget target in _Targets)
                    {
                        Console.WriteLine("Attaching target GUID " + target.GUID + " to event GUID " + entry.GUID);
                        entry.TargetGUID = target.GUID;
                        _ORM.Insert<AuditLogEntry>(entry);
                    }
                }
            }
        }

        /// <summary>
        /// Remove an event.
        /// </summary>
        /// <param name="guid">GUID of the event.</param>
        /// <param name="targetGuid">Target GUID from which to remove the event.  Leave null to remove event from all targets.</param>
        public void RemoveEvent(string guid, string targetGuid = null)
        {
            if (String.IsNullOrEmpty(guid)) throw new ArgumentNullException(nameof(guid)); 
            DbExpression e = new DbExpression(_ORM.GetColumnName<AuditLogEntry>(nameof(AuditLogEntry.GUID)), DbOperators.Equals, guid);
            if (!String.IsNullOrEmpty(targetGuid)) e.PrependAnd(_ORM.GetColumnName<AuditLogEntry>(nameof(AuditLogEntry.TargetGUID)), DbOperators.Equals, targetGuid);
            _ORM.DeleteMany<AuditLogEntry>(e);
        }

        #endregion

        #region Private-Methods

        private void Emitter(CancellationToken token)
        { 
            DbExpression expr = new DbExpression(_ORM.GetColumnName<AuditLogEntry>(nameof(AuditLogEntry.Id)), DbOperators.GreaterThan, 0);
            DbResultOrder[] ro = new DbResultOrder[1];
            ro[0] = new DbResultOrder(_ORM.GetColumnName<AuditLogEntry>(nameof(AuditLogEntry.CreatedUtc)), DbOrderDirection.Ascending);

            List<AuditLogTarget> targets = null;

            while (!token.IsCancellationRequested)
            {
                Task.Delay(_IntervalMs).Wait();
                targets = new List<AuditLogTarget>();

                try
                {
                    lock (_Lock)
                    {
                        if (_Targets == null || _Targets.Count < 1) continue;
                        targets = new List<AuditLogTarget>(_Targets);
                    }

                    List<AuditLogEntry> entries = _ORM.SelectMany<AuditLogEntry>(null, _BatchSize, expr, ro);
                    if (entries == null || entries.Count < 1) continue;

                    foreach (AuditLogEntry entry in entries)
                    {
                        if (entry.Attempts >= entry.MaxAttempts)
                        {
                            Log("entry " + entry.GUID + " reached maximum attempts, deleting (" + entry.Attempts + " of " + entry.MaxAttempts + " max):" + Environment.NewLine + Common.SerializeJson(entry, false));
                            _ORM.Delete<AuditLogEntry>(entry);
                            continue;
                        }

                        AuditLogTarget target = targets.FirstOrDefault(t => t.GUID.Equals(entry.TargetGUID));
                        if (target == default(AuditLogTarget))
                        {
                            Log("target " + entry.TargetGUID + " not found, deleting entry " + entry.GUID);
                            _ORM.Delete<AuditLogEntry>(entry);
                        }
                        else
                        {
                            try
                            {
                                if (!target.Action(entry))
                                {
                                    entry.Attempts = entry.Attempts + 1;
                                    _ORM.Update<AuditLogEntry>(entry);
                                    Log("target " + target.GUID + " " + target.Name + " reported failure for entry " + entry.GUID + " (" + entry.Attempts + " of " + entry.MaxAttempts + " max)");

                                    if (entry.Attempts >= entry.MaxAttempts)
                                    {
                                        Log("entry " + entry.GUID + " reached maximum attempts, deleting (" + entry.Attempts + " of " + entry.MaxAttempts + " max):" + Environment.NewLine + Common.SerializeJson(entry, false));
                                        _ORM.Delete<AuditLogEntry>(entry);
                                    }
                                }
                                else
                                {
                                    Log("target " + target.GUID + " " + target.Name + " success for entry " + entry.GUID);
                                    _ORM.Delete<AuditLogEntry>(entry);
                                }
                            }
                            catch (Exception eInner)
                            {
                                Log("exception encountered using target " + target.GUID + " " + target.Name + " for entry " + entry.GUID + Environment.NewLine + Common.SerializeJson(eInner, true));
                            }
                        }
                    }
                }
                catch (TaskCanceledException)
                {
                    Log("emitter canceled at " + DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"));
                    break;
                }
                catch (OperationCanceledException)
                {
                    Log("emitter canceled at " + DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"));
                    break;
                }
                catch (Exception eOuter)
                {
                    Log("exception encountered" + Environment.NewLine + Common.SerializeJson(eOuter, true));
                } 
            } 
        }

        private void Log(string msg)
        {
            Logger?.Invoke(_Header + msg);
        }

        #endregion
    }
}
