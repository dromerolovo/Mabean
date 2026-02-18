using System;
using System.Collections.Generic;
using System.Text;

namespace Mabean.Models
{
    public class ProcessInfo
    {
        public int ProcessId { get; set; }
        public required string ProcessName { get; set; }
        public required string UserName { get; set; }
    }
}
