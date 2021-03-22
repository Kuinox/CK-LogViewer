using CK.Monitoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.LogViewer.Enumerable
{
    public static class LogGroupFilterExtensions
    {
        public static IEnumerable<T> TakeOnlyGroup<T>( this IEnumerable<T> @this ) where T : IMulticastLogEntry
        {
            int currentDepth = -1;
            return @this.TakeWhile( ( log ) =>
            {
                if( currentDepth == -1 )
                {
                    currentDepth = log.GroupDepth;
                    return true;
                }
                if( log.GroupDepth > currentDepth ) return true;
                return false;
            } );
        }
    }
}
