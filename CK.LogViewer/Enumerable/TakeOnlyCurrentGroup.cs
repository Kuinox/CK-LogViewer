using CK.Monitoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.LogViewer.Enumerable
{
    public static class LogGroupFilterExtensions
    {
        public static IObservable<T> TakeOnlyCurrentGroupContent<T>( this IObservable<T> @this ) where T : IMulticastLogEntry
        {
            int depth = -1;
            return @this.TakeWhile( ( log ) =>
            {
                if( depth == -1 )
                {
                    depth = log.GroupDepth;
                    return true;
                }
                return log.GroupDepth > depth && !(log.GroupDepth -1 == depth && log.LogType == LogEntryType.CloseGroup);
            } ).Skip( 1 ); // Skip open group.
        }
    }
}
