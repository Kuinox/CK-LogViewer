using CK.Core;
using CK.Monitoring;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.LogViewer
{
    public static class EnumerableLogStatsExtensions
    {
        public static IEnumerable<LogEntryWithStats> ComputeStats( IEnumerable<IMulticastLogEntryWithOffset> @this ) => new Enumerable( @this );
        struct Enumerable : IEnumerable<LogEntryWithStats>
        {
            readonly Enumerator _enumerator;
            public Enumerable( IEnumerable<IMulticastLogEntryWithOffset> enumerable ) => _enumerator = new Enumerator( enumerable.GetEnumerator() );

            public IEnumerator<LogEntryWithStats> GetEnumerator() => _enumerator;

            IEnumerator IEnumerable.GetEnumerator() => _enumerator;
        }

        struct Enumerator : IEnumerator<LogEntryWithStats>
        {
            readonly IEnumerator<IMulticastLogEntryWithOffset> _enumerator;
            readonly Stack<Dictionary<LogLevel, int>> _stats;

            public Enumerator( IEnumerator<IMulticastLogEntryWithOffset> enumerator )
            {
                _enumerator = enumerator;
                _stats = new Stack<Dictionary<LogLevel, int>>();
                _stats.Push( new Dictionary<LogLevel, int>() );
            }

            public LogEntryWithStats Current => new( _enumerator.Current, _stats.Peek() );

            object IEnumerator.Current => Current;

            public void Dispose() => _enumerator.Dispose();
            public void Reset() => _enumerator.Reset();

            public bool MoveNext()
            {
                if( !_enumerator.MoveNext() ) return false;
                switch( _enumerator.Current.LogType )
                {
                    case LogEntryType.Line:
                        IncrementStat( _enumerator.Current.LogLevel );
                        break;
                    case LogEntryType.OpenGroup:
                        IncrementStat( _enumerator.Current.LogLevel );
                        _stats.Push( new Dictionary<LogLevel, int>() );
                        break;
                    case LogEntryType.CloseGroup:
                        _stats.Pop();
                        break;
                }
                return true;
            }

            void IncrementStat( LogLevel logLevel )
            {
                logLevel &= LogLevel.Mask;
                foreach( Dictionary<LogLevel, int> stat in _stats )
                {
                    if( !stat.TryGetValue( logLevel, out int currCount ) )
                    {
                        stat[logLevel] = 1;
                    }
                    else
                    {
                        stat[logLevel] = currCount + 1;
                    }
                }
            }
        }
    }

    public class LogEntryWithStats : IMulticastLogEntryWithOffset
    {
        readonly IMulticastLogEntryWithOffset _multicastLogEntryWithOffset;

        public LogEntryWithStats( IMulticastLogEntryWithOffset multicastLogEntryWithOffset, Dictionary<LogLevel, int> stats )
        {
            _multicastLogEntryWithOffset = multicastLogEntryWithOffset;
            Stats = stats;
        }
        public IReadOnlyDictionary<LogLevel, int> Stats { get; }
        public long Offset => _multicastLogEntryWithOffset.Offset;
        public int GroupDepth => _multicastLogEntryWithOffset.GroupDepth;
        public LogEntryType LogType => _multicastLogEntryWithOffset.LogType;
        public LogLevel LogLevel => _multicastLogEntryWithOffset.LogLevel;
        public string Text => _multicastLogEntryWithOffset.Text;
        public CKTrait Tags => _multicastLogEntryWithOffset.Tags;
        public DateTimeStamp LogTime => _multicastLogEntryWithOffset.LogTime;
        public CKExceptionData? Exception => _multicastLogEntryWithOffset.Exception;
        public string? FileName => _multicastLogEntryWithOffset.FileName;
        public int LineNumber => _multicastLogEntryWithOffset.LineNumber;
        public IReadOnlyList<ActivityLogGroupConclusion>? Conclusions => _multicastLogEntryWithOffset.Conclusions;
        public Guid MonitorId => _multicastLogEntryWithOffset.MonitorId;
        public LogEntryType PreviousEntryType => _multicastLogEntryWithOffset.PreviousEntryType;
        public DateTimeStamp PreviousLogTime => _multicastLogEntryWithOffset.PreviousLogTime;


        public ILogEntry CreateUnicastLogEntry() => _multicastLogEntryWithOffset.CreateUnicastLogEntry();
        public void WriteLogEntry( CKBinaryWriter w ) => _multicastLogEntryWithOffset.WriteLogEntry( w );
    }
}
