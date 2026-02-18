using Mabean.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Mabean.Utils
{
    public static class EventsUtils
    {
        public static void SpawnMarkerEvent()
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = Path.Combine(Paths.DataDir, "MabeanMarker.exe"),
                    CreateNoWindow = true,
                    UseShellExecute = false
                }
            };
            process.Start();
            process.WaitForExit();
        }
    }
}
