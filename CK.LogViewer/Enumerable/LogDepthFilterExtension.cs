using CK.Monitoring;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.LogViewer
{
    public static class LogDepthFilterExtension
    {
        public static IObservable<T> FilterDepth<T>( this IObservable<T> @this, int unfoldedDepth ) where T : IMulticastLogEntry
            => @this.Where( entry => !(entry.LogType != LogEntryType.CloseGroup && entry.GroupDepth > unfoldedDepth
                                          || entry.LogType == LogEntryType.CloseGroup && entry.GroupDepth - 1 > unfoldedDepth) );
    }
}
