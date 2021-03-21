using CK.Monitoring;

namespace CK.LogViewer
{
    public interface IMulticastLogEntryWithOffset : IMulticastLogEntry
    {
        long Offset { get; }
    }
}
