using Mabean.Models;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mabean.Services
{
    public class EventsService : BackgroundService
    {
        private EventLogWatcher? _watcher;
        private const string _sysmonLogName = "Microsoft-Windows-Sysmon/Operational";
        private const int _maxEventsLimit = 500;

        private readonly Subject<SecurityEvent> _eventSubject = new();
        //private readonly BehaviorSubject<string> _statusSubject = new("Starting...");

        public IObservable<SecurityEvent> EventStream => _eventSubject.AsObservable();
        //public IObservable<string> StatusStream => _statusSubject.AsObservable();

        public bool IsRunning { get; private set; }
        public bool IsWatching { get; set; } = true;

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(() => StartWatcher(stoppingToken), stoppingToken);
        }

        private void StartWatcher(CancellationToken cancellationToken)
        {
            try
            {
                if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var process = Process.GetCurrentProcess();
                    var processId = Process.GetCurrentProcess().Id;

                    string processPath = process.MainModule.FileName;

                    var xpathQuery = $@"
*[System[Provider[@Name='Microsoft-Windows-Sysmon']]]
[EventData[
  Data[@Name='ProcessId']='{processId}' or
  Data[@Name='ParentProcessId']='{processId}' or
  Data[@Name='SourceProcessId']='{processId}' or
  Data[@Name='TargetProcessId']='{processId}' or
  Data[@Name='Image']='{processPath}' or
  Data[@Name='ParentImage']='{processPath}'
]]
";


                    //Research more about events query(Win32 api) and XPath
                    var query = new EventLogQuery(_sysmonLogName, PathType.LogName, xpathQuery);
                    _watcher = new EventLogWatcher(query);
                    _watcher.EventRecordWritten += OnEventRecordWritten;
                    _watcher.Enabled = true;

                    IsWatching = true;
                }
                

            } catch (Exception ex)
            {
                
            }
        }

        private void OnEventRecordWritten(object? sender, EventRecordWrittenEventArgs e)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;
            if (e.EventRecord == null) return;

            using var record = e.EventRecord;
            if (record == null) return;
            var @event = new SecurityEvent
            {
                RecordId = record.RecordId ?? 0,
                TimeCreated = record.TimeCreated ?? DateTime.Now,
                EventId = record.Id,
                Source = record.ProviderName ?? string.Empty,
                TaskCategory = record.TaskDisplayName ?? string.Empty,
                Message = record.FormatDescription() ?? string.Empty
            };

            _eventSubject.OnNext(@event);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            if (_watcher != null)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    _watcher.EventRecordWritten -= OnEventRecordWritten;
                    _watcher.Enabled = false;
                    _watcher.Dispose();
                    _watcher = null;
                    _eventSubject.OnCompleted();
                }
            }
            IsWatching = false;
            return base.StopAsync(cancellationToken);
        }
    }
}
