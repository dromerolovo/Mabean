using System;
using System.Collections.Generic;
using System.Text;

namespace Mabean.Models
{
    public class SecurityEvent
    {
        public long RecordId { get; set; }
        public DateTime TimeCreated { get; set; }
        public int EventId { get; set; }
        public string Source { get; set; } = string.Empty;
        public string TaskCategory { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsSelected { get; set; } = false;
    }
}
