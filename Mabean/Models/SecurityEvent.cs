using System;
using System.Collections.Generic;
using System.Text;

namespace Mabean.Models
{
    public record SecurityEvent
    {
        public long RecordId { get; init; }
        public DateTime TimeCreated { get; init; }
        public int EventId { get; init; }
        public string Source { get; init; } = string.Empty;
        public string TaskCategory { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
    }
}
