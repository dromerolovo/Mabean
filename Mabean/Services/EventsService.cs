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
        private EventLogWatcher? _reverseShellWatcher;
        private const string _sysmonLogName = "Microsoft-Windows-Sysmon/Operational";
        private const int _maxEventsLimit = 500;
        private const string _markerProcessExe = "MabeanMarker.exe";
        private volatile bool _markerActivated = false;
        private readonly int _currentPid = Process.GetCurrentProcess().Id;

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

        private static readonly Dictionary<int, string> _sysmonCategories = new()
        {
            { 1,  "Process Create" },
            { 2,  "File Creation Time" },
            { 3,  "Network Connect" },
            { 4,  "Sysmon Service State" },
            { 5,  "Process Terminate" },
            { 6,  "Driver Load" },
            { 7,  "Image Load" },
            { 8,  "Create Remote Thread" },
            { 9,  "Raw Disk Access" },
            { 10, "Process Access" },
            { 11, "File Create" },
            { 12, "Registry Create/Delete" },
            { 13, "Registry Set Value" },
            { 14, "Registry Rename" },
            { 15, "File Create Stream Hash" },
            { 16, "Sysmon Config Change" },
            { 17, "Pipe Created" },
            { 18, "Pipe Connected" },
            { 19, "WMI Filter" },
            { 20, "WMI Consumer" },
            { 21, "WMI Consumer Filter" },
            { 22, "DNS Query" },
            { 23, "File Delete" },
            { 24, "Clipboard Change" },
            { 25, "Process Tamper" },
            { 26, "File Delete Logged" },
            { 27, "File Block Executable" },
            { 28, "File Block Shredding" },
            { 29, "File Executable Detected" },
            { 255, "Error" },
        };

        private static string ResolveSysmonCategory(int eventId) =>
            _sysmonCategories.TryGetValue(eventId, out var name) ? name : $"Event {eventId}";

        private void OnEventRecordWritten(object? sender, EventRecordWrittenEventArgs e)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;
            if (e.EventRecord == null) return;

            using var record = e.EventRecord;

            var message = record.FormatDescription() ?? string.Empty;

            if(record.Id == 1 && message.Contains(_markerProcessExe, StringComparison.OrdinalIgnoreCase))
            {
                if(!_markerActivated)
                {
                    Console.WriteLine("Marker Activated");
                    _markerActivated = true;
                } else
                {
                    Console.WriteLine("Marker Deactivated");
                    //TODO: Very unstable, find a better way to avoid getting the powershell related events
                    _markerActivated = false;
                }
                return;
            }

            if (_markerActivated) return;

            if (message.Contains(_markerProcessExe, StringComparison.OrdinalIgnoreCase)) return;

            if (record.Id == 22) return;

            Console.WriteLine($"Pwsh event");
            var @event = new SecurityEvent
            {
                RecordId = record.RecordId ?? 0,
                TimeCreated = record.TimeCreated ?? DateTime.Now,
                EventId = record.Id,
                Source = record.ProviderName ?? string.Empty,
                TaskCategory = ResolveSysmonCategory(record.Id),
                Message = message
            };

            _eventSubject.OnNext(@event);
        }

        public void StartReverseShellWatcher(string lhost, string lport)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;

            _reverseShellWatcher?.Dispose();

            var xpathQuery = $@"
            *[System[Provider[@Name='Microsoft-Windows-Sysmon']]]
            [System[EventID=3]]
            [EventData[
              Data[@Name='DestinationPort']='{lport}' and
              Data[@Name='DestinationIp']='{lhost}'
            ]]";

            var query = new EventLogQuery(_sysmonLogName, PathType.LogName, xpathQuery);
            _reverseShellWatcher = new EventLogWatcher(query);
            _reverseShellWatcher.EventRecordWritten += OnReverseShellEventWritten;
            _reverseShellWatcher.Enabled = true;
        }

        private void OnReverseShellEventWritten(object? sender, EventRecordWrittenEventArgs e)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;
            if (e.EventRecord == null) return;

            using var record = e.EventRecord;
            var message = record.FormatDescription() ?? string.Empty;

            if (message.Contains($"ProcessId: {_currentPid}")) return;

            var @event = new SecurityEvent
            {
                RecordId = record.RecordId ?? 0,
                TimeCreated = record.TimeCreated ?? DateTime.Now,
                EventId = record.Id,
                Source = record.ProviderName ?? string.Empty,
                TaskCategory = "Reverse Shell Connection",
                Message = message
            };

            _eventSubject.OnNext(@event);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (_watcher != null)
                {
                    _watcher.EventRecordWritten -= OnEventRecordWritten;
                    _watcher.Enabled = false;
                    _watcher.Dispose();
                    _watcher = null;
                }

                if (_reverseShellWatcher != null)
                {
                    _reverseShellWatcher.EventRecordWritten -= OnReverseShellEventWritten;
                    _reverseShellWatcher.Enabled = false;
                    _reverseShellWatcher.Dispose();
                    _reverseShellWatcher = null;
                }

                _eventSubject.OnCompleted();
            }
            IsWatching = false;
            return base.StopAsync(cancellationToken);
        }
    }
}
