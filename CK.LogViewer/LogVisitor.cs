using CK.Core;
using CK.Monitoring;
using System.Collections.Generic;
using System.IO;

namespace CK.LogViewer
{
    public class LogVisitor
    {
        protected readonly LogReader _logReader;
        protected readonly Stack<Dictionary<LogLevel, int>> Stats;

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

        protected virtual bool VisitLogEntry( MulticastLogEntryWithOffset entry )
            => entry.Entry.LogType switch
            {
                LogEntryType.Line => VisitLogLine( entry ),
                LogEntryType.OpenGroup => VisitOpenGroup( entry ),
                LogEntryType.CloseGroup => VisitCloseGroup( entry ),
                _ => throw new InvalidDataException( "Invalid log type." ),
            };


        void IncrementStat( LogLevel logLevel )
        {
            var currStat = Stats.Peek();
            if( !currStat.TryGetValue( logLevel, out int currCount ) )
            {
                currStat[logLevel] = 1;
            }
            else
            {
                currStat[logLevel] = currCount + 1;
            }
        }

        protected virtual bool VisitOpenGroup( MulticastLogEntryWithOffset entry )
        {
            IncrementStat( entry.Entry.LogLevel );
            Stats.Push( new Dictionary<LogLevel, int>() );
            return true;
        }

        protected virtual bool VisitCloseGroup( MulticastLogEntryWithOffset entry )
        {
            Stats.Pop();
            return true;
        }

        protected virtual bool VisitLogLine( MulticastLogEntryWithOffset entry )
        {
            IncrementStat( entry.Entry.LogLevel );
            return true;
        }
    }
}
