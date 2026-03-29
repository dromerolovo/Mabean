using System;
using System.Collections.Generic;
using System.Text;

namespace Mabean.Models
{
    internal class BehaviorContext
    {
        public string BehaviorName { get; set; }
        public string DllPath { get; set; }
        public uint TargetPID { get; set; }
        public string? ProgramName { get; set; }
        public string? PayloadPath { get; set; }
    }
}
