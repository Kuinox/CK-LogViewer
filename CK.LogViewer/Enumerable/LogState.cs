using CK.Core;
using CK.LogViewer;
using CK.Monitoring;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace CK.LogViewer
{
    public static class EnumerableLogStatsExtensions
    {
        public static IEnumerable<LogEntryWithState> AddState( this IEnumerable<IMulticastLogEntryWithOffset> @this ) => new Enumerable( @this );
        struct Enumerable : IEnumerable<LogEntryWithState>
        {
            readonly Enumerator _enumerator;
            public Enumerable( IEnumerable<IMulticastLogEntryWithOffset> enumerable ) => _enumerator = new Enumerator( enumerable.GetEnumerator() );

            public IEnumerator<LogEntryWithState> GetEnumerator() => _enumerator;

            IEnumerator IEnumerable.GetEnumerator() => _enumerator;
        }

        struct Enumerator : IEnumerator<LogEntryWithState>
        {
            readonly IEnumerator<IMulticastLogEntryWithOffset> _enumerator;
            readonly Stack<Dictionary<LogLevel, int>> _stats;
            readonly Dictionary<Guid, int> _monitors;
            public Enumerator( IEnumerator<IMulticastLogEntryWithOffset> enumerator )
            {
                _enumerator = enumerator;
                _stats = new Stack<Dictionary<LogLevel, int>>();
                _monitors = new Dictionary<Guid, int>();
                _stats.Push( new Dictionary<LogLevel, int>() );
                _currentStats = null!;
            }

            public LogEntryWithState Current => new( _enumerator.Current, _currentStats, GetMonitorId( _enumerator.Current.MonitorId ) );
            Dictionary<LogLevel, int> _currentStats;
            object IEnumerator.Current => Current;

            public void Dispose() => _enumerator.Dispose();
            public void Reset() => _enumerator.Reset();

            int GetMonitorId( Guid monitorId )
            {
                if( monitorId == Guid.Empty ) return 0;
                int value;
                if( _monitors.TryGetValue( monitorId, out value ) )
                {
                    return value;
                }
                else
                {
                    var newId = _monitors.Count + 1;
                    _monitors.Add( monitorId, newId );
                    return newId;
                }
            }

            public bool MoveNext()
            {
                if( !_enumerator.MoveNext() ) return false;
                _currentStats = _stats.Peek();
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

        public static IEnumerable<LogEntryWithState> ComputeState( this IEnumerable<IMulticastLogEntryWithOffset> @this ) => new Enumerable( @this );

        // OK this class is for all entries but it's state is for all entries.
        public class LogEntryWithState : IMulticastLogEntryWithOffset
        {
            readonly IMulticastLogEntryWithOffset _multicastLogEntryWithOffset;

            public LogEntryWithState( IMulticastLogEntryWithOffset multicastLogEntryWithOffset, Dictionary<LogLevel, int> stats, int monitorSimpleId )
            {
                Debug.Assert( stats != null );
                _multicastLogEntryWithOffset = multicastLogEntryWithOffset;
                Stats = stats;
                MonitorSimpleId = monitorSimpleId;
            }

            public IReadOnlyDictionary<LogLevel, int> Stats { get; }
            public int MonitorSimpleId { get; }
            public bool Folded { get; set; }

            #region InterfaceImpl
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
            #endregion
        }
    }
}
