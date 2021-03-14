using CK.Monitoring;

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
                if( entry.Entry.GroupDepth - 1 > _unfoldedDepth ) return true;
            }
            else if( entry.Entry.GroupDepth > _unfoldedDepth ) return true;
            return base.VisitLogEntry( entry );
        }
    }
}
