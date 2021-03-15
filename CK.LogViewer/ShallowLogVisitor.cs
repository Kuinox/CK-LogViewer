using CK.Monitoring;
using System;
using System.IO;

namespace CK.LogViewer
{
    public class ShallowLogVisitor : LogVisitor
    {
        readonly int _unfoldedDepth;

        public ShallowLogVisitor( int unfoldedDepth, LogReader logReader ) : base( logReader ) => _unfoldedDepth = unfoldedDepth;

        protected override bool VisitLogEntry( MulticastLogEntryWithOffset entry )
        {
            if( entry.Entry.LogType == LogEntryType.CloseGroup )
            {
                if( entry.Entry.GroupDepth - 1 > _unfoldedDepth ) return SkippingLogEntry( entry );
            }
            else if( entry.Entry.GroupDepth > _unfoldedDepth ) return SkippingLogEntry( entry );
            return base.VisitLogEntry( entry );
        }

        /// <summary>
        /// Wont call overloads.
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        protected bool BaseVisitLogEntry( MulticastLogEntryWithOffset entry )
            => entry.Entry.LogType switch
            {
                LogEntryType.Line => base.VisitLogLine( entry ),
                LogEntryType.OpenGroup => base.VisitOpenGroup( entry ),
                LogEntryType.CloseGroup => base.VisitCloseGroup( entry ),
                _ => throw new InvalidDataException( "Invalid log type." ),
            };

        protected virtual bool SkippingLogEntry( MulticastLogEntryWithOffset entry ) => BaseVisitLogEntry( entry );

        protected bool SkipCurrentGroup()
        {
            throw new NotImplementedException( "Implement stats increment" );
            int currentDepth = _logReader.CurrentMulticast.GroupDepth;
            bool next = _logReader.MoveNext();
            while( next && _logReader.CurrentMulticast.GroupDepth >= currentDepth )
            {
                next = _logReader.MoveNext();
            }
            return next;
        }
    }
}
