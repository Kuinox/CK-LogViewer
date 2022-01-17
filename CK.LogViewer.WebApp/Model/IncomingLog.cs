using CK.Monitoring;
using System;

namespace CK.LogViewer.WebApp.Model
{
    public class IncomingLog
    {
        public Guid InstanceGuid { get; set; }
        public IMulticastLogEntry LogEntry { get; set; } = null!;
        public string Topic { get; set; } = null!;
    }
}
