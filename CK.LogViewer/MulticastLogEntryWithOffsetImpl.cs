using CK.Core;
using CK.Monitoring;
using System;
using System.Collections.Generic;

namespace CK.LogViewer
{
    public class MulticastLogEntryWithOffsetImpl : IMulticastLogEntryWithOffset
    {
        readonly IMulticastLogEntry _multicastLogEntry;

        public MulticastLogEntryWithOffsetImpl( MulticastLogEntryWithOffset multicastLogEntry )
        {
            _multicastLogEntry = multicastLogEntry.Entry;
            Offset = multicastLogEntry.Offset;
        }

        public long Offset { get; }

        public int GroupDepth => _multicastLogEntry.GroupDepth;

        public LogEntryType LogType => _multicastLogEntry.LogType;

        public LogLevel LogLevel => _multicastLogEntry.LogLevel;

        public string Text => _multicastLogEntry.Text;

        public CKTrait Tags => _multicastLogEntry.Tags;

        public DateTimeStamp LogTime => _multicastLogEntry.LogTime;

        public CKExceptionData? Exception => _multicastLogEntry.Exception;

        public string? FileName => _multicastLogEntry.FileName;

        public int LineNumber => _multicastLogEntry.LineNumber;

        public IReadOnlyList<ActivityLogGroupConclusion>? Conclusions => _multicastLogEntry.Conclusions;

        public Guid MonitorId => _multicastLogEntry.MonitorId;

        public LogEntryType PreviousEntryType => _multicastLogEntry.PreviousEntryType;

        public DateTimeStamp PreviousLogTime => _multicastLogEntry.PreviousLogTime;


        public ILogEntry CreateUnicastLogEntry() => _multicastLogEntry.CreateUnicastLogEntry();

        public void WriteLogEntry( CKBinaryWriter w )
        {
            _multicastLogEntry.WriteLogEntry( w );
        }
    }
}
