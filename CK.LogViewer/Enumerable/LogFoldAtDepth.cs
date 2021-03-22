using CK.Monitoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.LogViewer.Enumerable
{
    public static class LogFoldAtDepthExtensions
    {
        public static IEnumerable<LogEntryWithState> FoldAtDepth( this IEnumerable<LogEntryWithState> @this, int depthToFold )
            => @this.Select( s =>
            {
                if( s.LogType == LogEntryType.OpenGroup && s.GroupDepth >= depthToFold )
                {
                    s.Folded = true;
                }
                return s;
            } );
    }
}
