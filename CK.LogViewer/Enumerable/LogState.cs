using CK.Core;
using CK.Monitoring;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CK.LogViewer.Enumerable
{
    public static class EnumerableLogStatsExtensions
    {
        public static IObservable<LogEntryWithState> AddState( this IObservable<IMulticastLogEntryWithOffset> @this )
            => @this
                .GroupBy( s => s.MonitorId )
                .Select( s => new Observable( s.AsObservable() ) )
                .SelectMany( s => s );

        class Observable : IObservable<LogEntryWithState>
        {
            readonly IObservable<IMulticastLogEntryWithOffset> _observable;

            public Observable( IObservable<IMulticastLogEntryWithOffset> observable )
                => _observable = observable;

            public IDisposable Subscribe( IObserver<LogEntryWithState> observer )
                => _observable.Subscribe( new Observer( observer ) );

        }

        class Observer : IObserver<IMulticastLogEntryWithOffset>
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
            readonly Stack<GroupData> _dataStack;
            readonly IObserver<LogEntryWithState> _observer;

            public Observer( IObserver<LogEntryWithState> observer )
            {
                _dataStack = new Stack<GroupData>();
                _dataStack.Push( new GroupData() );
                _observer = observer;
            }

            public void OnCompleted() => _observer.OnCompleted();

            public void OnError( Exception error ) => _observer.OnError( error );

            public void OnNext( IMulticastLogEntryWithOffset value )
            {
                _observer.OnNext( new LogEntryWithState(
                    value,
                    CurrentData.LogLevelSummary,
                    CurrentData.ParentsGroupLevels,
                    CurrentData.GroupOffset
                ) );
                if( value.LogType == LogEntryType.CloseGroup )
                {
                    _dataStack.Pop();
                }
                switch( value.LogType )
                {
                    case LogEntryType.Line:
                        IncrementStat( value.LogLevel );
                        break;
                    case LogEntryType.OpenGroup:
                        IncrementStat( value.LogLevel );
                        _dataStack.Push(
                            new GroupData( CurrentData, value )
                        );
                        break;
                }
            }

            GroupData CurrentData => _dataStack.Peek();


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
