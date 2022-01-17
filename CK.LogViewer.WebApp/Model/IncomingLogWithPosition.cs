using CK.Monitoring;
using System;

namespace CK.LogViewer.WebApp.Model
{
    public class IncomingLogWithPosition
    {
        public Guid InstanceGuid { get; set; }
        public IMulticastLogEntryWithOffset LogEntry { get; set; } = null!;
        public string Topic { get; set; } = null!;
    }
}
