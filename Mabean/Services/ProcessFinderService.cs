using Mabean.Models;
using Mabean.Utils;
using Microsoft.PowerShell;
using System;
using System.Collections.Generic;
using System.Management;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace Mabean.Services
{
    public class ProcessFinderService
    {
        public ProcessInfo[] GetProcesses()
        {
            var processes = new List<ProcessInfo>();
            using PowerShell ps = PowerShell.Create();
            ps.AddScript("Get-Process -IncludeUserName | Select-Object Name, Id, UserName");
            //TODO: Very unstable, find a better way to avoid getting the powershell related events 
            EventsUtils.SpawnMarkerEvent();
            var results = ps.Invoke();
            EventsUtils.SpawnMarkerEvent();
            foreach (PSObject result in results)
            {
                var name = result.Properties["Name"]?.Value;     
                var id = result.Properties["Id"]?.Value;       
                var userName = result.Properties["UserName"]?.Value;

                var processInfo = new ProcessInfo
                {
                    ProcessName = name?.ToString() ?? "Unknown",
                    ProcessId = id != null ? Convert.ToInt32(id) : 909090,
                    UserName = userName?.ToString() ?? "N/A"
                };

                processes.Add(processInfo);
            }

            return processes.ToArray();
        }
    }
}
