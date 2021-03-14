using CK.Monitoring;
using System.IO;

namespace CK.LogViewer
{
    public class LogVisitor
    {
        protected readonly LogReader _logReader;

        public LogVisitor( LogReader logReader )
            => _logReader = logReader;
        protected MulticastLogEntryWithOffset Current { get; private set; }

        public virtual void Visit()
        {
            bool shouldContinue = true;
            while( shouldContinue && _logReader.MoveNext() )
            {
                Current = _logReader.CurrentMulticastWithOffset;
                shouldContinue = VisitLogEntry( Current );
            }
        }

        protected bool SkipCurrentGroup()
        {
            int currentDepth = _logReader.CurrentMulticast.GroupDepth;
            bool next = _logReader.MoveNext();
            while( next && _logReader.CurrentMulticast.GroupDepth >= currentDepth )
            {
                next = _logReader.MoveNext();
            }
            return next;
        }

        protected virtual bool VisitLogEntry( MulticastLogEntryWithOffset entry )
            => entry.Entry.LogType switch
            {
                LogEntryType.Line => VisitLogLine( entry ),
                LogEntryType.OpenGroup => VisitOpenGroup( entry ),
                LogEntryType.CloseGroup => VisitCloseGroup( entry ),
                _ => throw new InvalidDataException( "Invalid log type." ),
            };

        protected virtual bool VisitOpenGroup( MulticastLogEntryWithOffset entry ) => true;

        protected virtual bool VisitCloseGroup( MulticastLogEntryWithOffset entry ) => true;

        protected virtual bool VisitLogLine( MulticastLogEntryWithOffset entry ) => true;
    }
}
