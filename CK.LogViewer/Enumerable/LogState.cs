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
        public static IEnumerable<LogEntryWithState> AddState( this IEnumerable<IMulticastLogEntryWithOffset> @this )
            => @this
                .GroupBy( s => s.MonitorId )
                .Select( s => new Enumerable( s ) ) //this is a single monitor method.
                .SelectMany( s => s )
                .OrderBy( s => s.LogTime );

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
            public Enumerator( IEnumerator<IMulticastLogEntryWithOffset> enumerator )
            {
                _enumerator = enumerator;
                _dataStack = new Stack<GroupData>();
                _dataStack.Push( new GroupData() );
            }

            public LogEntryWithState Current => new(
                _enumerator.Current,
                CurrentData.LogLevelSummary,
                _enumerator.Current.LogType != LogEntryType.OpenGroup ?
                    CurrentData.ParentsGroupLevels
                    : CurrentData.ParentsGroupLevels.RemoveRange( CurrentData.ParentsGroupLevels.Length - 1, 1 ),
                CurrentData.GroupOffset
            );
            GroupData CurrentData => _dataStack.Peek();
            object IEnumerator.Current => Current;

            public void Dispose() => _enumerator.Dispose();
            public void Reset() => _enumerator.Reset();

            public bool MoveNext()
            {
                if( _enumerator.Current?.LogType == LogEntryType.CloseGroup )
                {
                    _dataStack.Pop();
                }
                if( !_enumerator.MoveNext() ) return false;

                switch( _enumerator.Current!.LogType )
                {
                    case LogEntryType.Line:
                        IncrementStat( _enumerator.Current.LogLevel );
                        break;
                    case LogEntryType.OpenGroup:
                        IncrementStat( _enumerator.Current.LogLevel );
                        _dataStack.Push(
                            new GroupData( CurrentData, _enumerator.Current )
                        );
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
                IReadOnlyDictionary<LogLevel, int> stats,
                ImmutableArray<(LogLevel logLevel, long groupOffset)> parentsLogLevel,
                long groupOffset
            )
            {
                Debug.Assert( stats != null );
                _multicastLogEntryWithOffset = multicastLogEntryWithOffset;
                Stats = stats;
                ParentsLogLevel = parentsLogLevel;
                GroupOffset = groupOffset;
            }

            public IReadOnlyDictionary<LogLevel, int> Stats { get; }
            public ImmutableArray<(LogLevel logLevel, long groupOffset)> ParentsLogLevel { get; }
            public long GroupOffset { get; }

            #region InterfaceImpl
            public long Offset => _multicastLogEntryWithOffset.Offset;
            public int GroupDepth => _multicastLogEntryWithOffset.GroupDepth;
            public LogEntryType LogType => _multicastLogEntryWithOffset.LogType;
            public LogLevel LogLevel => _multicastLogEntryWithOffset.LogLevel;
            public string? Text => _multicastLogEntryWithOffset.Text;
            public CKTrait Tags => _multicastLogEntryWithOffset.Tags;
            public DateTimeStamp LogTime => _multicastLogEntryWithOffset.LogTime;
            public CKExceptionData? Exception => _multicastLogEntryWithOffset.Exception;
            public string? FileName => _multicastLogEntryWithOffset.FileName;
            public int LineNumber => _multicastLogEntryWithOffset.LineNumber;
            public IReadOnlyList<ActivityLogGroupConclusion>? Conclusions => _multicastLogEntryWithOffset.Conclusions;
            public string MonitorId => _multicastLogEntryWithOffset.MonitorId;
            public LogEntryType PreviousEntryType => _multicastLogEntryWithOffset.PreviousEntryType;
            public DateTimeStamp PreviousLogTime => _multicastLogEntryWithOffset.PreviousLogTime;
            public ILogEntry CreateUnicastLogEntry() => _multicastLogEntryWithOffset.CreateUnicastLogEntry();
            public void WriteLogEntry( CKBinaryWriter w ) => _multicastLogEntryWithOffset.WriteLogEntry( w );
            #endregion
        }
    }
}
