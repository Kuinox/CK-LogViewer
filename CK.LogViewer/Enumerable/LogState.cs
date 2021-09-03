using CK.Core;
using CK.LogViewer;
using CK.Monitoring;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

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

        class Enumerator : IEnumerator<LogEntryWithState>
        {
            class GroupData
            {
                public readonly Dictionary<LogLevel, int> LogLevelSummary = new();
                public readonly ImmutableArray<(LogLevel logLevel, long groupOffset)> ParentsGroupLevels;
                public readonly long GroupOffset;

                public GroupData()
                {
                    ParentsGroupLevels = ImmutableArray<(LogLevel, long)>.Empty;
                    GroupOffset = 0;
                }
                public GroupData( GroupData parent, IMulticastLogEntryWithOffset entry )
                {
                    ParentsGroupLevels = parent.ParentsGroupLevels.Add( (entry.LogLevel, entry.Offset) );
                    GroupOffset = entry.Offset;
                }
            }
            readonly IEnumerator<IMulticastLogEntryWithOffset> _enumerator;
            readonly Stack<GroupData> _dataStack;
            readonly Dictionary<Guid, int> _monitors;
            public Enumerator( IEnumerator<IMulticastLogEntryWithOffset> enumerator )
            {
                _enumerator = enumerator;
                _dataStack = new Stack<GroupData>();
                _monitors = new Dictionary<Guid, int>();
                _dataStack.Push( new GroupData() );
                _currentData = null!;
            }

            public LogEntryWithState Current => new(
                _enumerator.Current,
                _currentData.LogLevelSummary,
                GetMonitorId( _enumerator.Current.MonitorId ),
                _currentData.ParentsGroupLevels,
                _currentData.GroupOffset
            );
            GroupData _currentData;
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
                _currentData = _dataStack.Peek();
                switch( _enumerator.Current.LogType )
                {
                    case LogEntryType.Line:
                        IncrementStat( _enumerator.Current.LogLevel );
                        break;
                    case LogEntryType.OpenGroup:
                        IncrementStat( _enumerator.Current.LogLevel );
                        _dataStack.Push(
                            new GroupData( _currentData, _enumerator.Current )
                        );
                        break;
                    case LogEntryType.CloseGroup:
                        _dataStack.Pop();
                        break;
                }
                return true;
            }

            void IncrementStat( LogLevel logLevel )
            {
                logLevel &= LogLevel.Mask;
                foreach( var data in _dataStack )
                {
                    if( !data.LogLevelSummary.TryGetValue( logLevel, out int currCount ) )
                    {
                        data.LogLevelSummary[logLevel] = 1;
                    }
                    else
                    {
                        data.LogLevelSummary[logLevel] = currCount + 1;
                    }
                }
            }
        }

        public static IEnumerable<LogEntryWithState> ComputeState( this IEnumerable<IMulticastLogEntryWithOffset> @this ) => new Enumerable( @this );

        // OK this class is for all entries but it's state is for all entries.
        public class LogEntryWithState : IMulticastLogEntryWithOffset
        {
            readonly IMulticastLogEntryWithOffset _multicastLogEntryWithOffset;

            public LogEntryWithState(
                IMulticastLogEntryWithOffset multicastLogEntryWithOffset,
                Dictionary<LogLevel, int> stats,
                int monitorSimpleId,
                ImmutableArray<(LogLevel logLevel, long groupOffset)> parentsLogLevel,
                long groupOffset
            )
            {
                Debug.Assert( stats != null );
                _multicastLogEntryWithOffset = multicastLogEntryWithOffset;
                Stats = stats;
                MonitorSimpleId = monitorSimpleId;
                ParentsLogLevel = parentsLogLevel;
                GroupOffset = groupOffset;
            }

            public IReadOnlyDictionary<LogLevel, int> Stats { get; }
            public int MonitorSimpleId { get; }
            public ImmutableArray<(LogLevel logLevel, long groupOffset)> ParentsLogLevel { get; }
            public long GroupOffset { get; }

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
