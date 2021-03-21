using CK.Monitoring;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.LogViewer
{
    public static class EnumerableLogDepthFilterExtension
    {
        public static IEnumerable<T> FilterDepth<T>( this IEnumerable<T> @this, int unfoldedDepth ) where T : IMulticastLogEntry
            => @this.Where( entry =>
             {
                 return !(entry.LogType != LogEntryType.CloseGroup && entry.GroupDepth > unfoldedDepth
                   || entry.LogType == LogEntryType.CloseGroup && entry.GroupDepth - 1 > unfoldedDepth);
             } );
    }
}
