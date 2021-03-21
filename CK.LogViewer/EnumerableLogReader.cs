using CK.Monitoring;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.LogViewer
{
    public static class EnumerableLogReaderExtensions
    {
        public static IEnumerable<IMulticastLogEntryWithOffset> ToEnumerable( this LogReader @this ) => new EnumerableLogReader( @this );
    }

    class EnumerableLogReader : IEnumerable<IMulticastLogEntryWithOffset>
    {
        public struct Enumerator : IEnumerator<IMulticastLogEntryWithOffset>
        {
            readonly LogReader _logReader;
            public Enumerator( LogReader logReader ) => _logReader = logReader;

            public IMulticastLogEntryWithOffset Current => new MulticastLogEntryWithOffsetImpl( _logReader.CurrentMulticastWithOffset );

            object IEnumerator.Current => Current;

            public void Dispose() => _logReader.Dispose();

            public bool MoveNext() => _logReader.MoveNext();

            public void Reset() => _logReader.MoveNext();
        }

        readonly Enumerator _logReader;
        public EnumerableLogReader( LogReader logReader ) => _logReader = new Enumerator( logReader );

        public IEnumerator<IMulticastLogEntryWithOffset > GetEnumerator() => _logReader;

        IEnumerator IEnumerable.GetEnumerator() => _logReader;
    }
}
