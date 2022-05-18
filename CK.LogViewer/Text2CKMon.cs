using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CK.Core;
using CK.Monitoring;
using CK.Monitoring.Handlers;


namespace CKLogViewer
{
    public static class Text2CKMon
    {
        class MyBinaryOutput : IDisposable
        {
            readonly CKBinaryWriter _writer;
            public MyBinaryOutput( string configuredPath )
            {
                _writer = new CKBinaryWriter( File.OpenWrite( configuredPath ) );
                _writer.Write( LogReader.FileHeader );
                _writer.Write( LogReader.CurrentStreamVersion );
            }

            public void Write( IMulticastLogEntry entry ) => entry.WriteLogEntry( _writer );

            public void Dispose()
            {
                _writer.Write( 0 );
                _writer.Dispose();
            }
        }
        public static async Task Run( Stream input, string ckmonPath )
        {
            using StreamReader sr = new( input );
            string text = await sr.ReadToEndAsync();
            StringBuilder sb = new();
            using( MyBinaryOutput bf = new( ckmonPath ) )
            {
                Parse( bf );
            }

            void Parse( MyBinaryOutput bf )
            {
                Dictionary<string, string> idMap = new() { { "###", GrandOutput.ExternalLogMonitorUniqueId } };
                Dictionary<string, (LogEntryType, DateTimeStamp, LogLevel)> typeMap = new() { { "###", (LogEntryType.Line, DateTimeStamp.Invalid, LogLevel.Info) } };
                ReadOnlyMemory<char> buffer = text.AsMemory();
                SequenceReader<char> reader = new( new( buffer ) );
                while( reader.Remaining > 0 )
                {
                    (DateTime dateTime, int dateTimeSize) = ParseTimeStamp( ref reader );

                    DateTimeStamp stamp = new( dateTime );
                    string monitorId = ParseMonitorId( ref reader );
                    bool notNewMonitor = idMap.ContainsKey( monitorId );
                    Is( ref reader, " " );
                    LogLevel ParseLogLevel( ref SequenceReader<char> reader )
                    {
                        // 2022-01-20 00h41.55.2591299 ~002 t |  [] Hello
                        // 2022-01-20 00h41.55.2591482 ~002 i |  [] Hello
                        // 2022-01-20 00h41.55.2591490 ~002 W |  [] Hello
                        // 2022-01-20 00h41.55.2591512 ~002 E |  [] Hello
                        // 2022-01-20 00h41.55.2591516 ~002 F |  [] Hello
                        reader.TryRead( out char logLevel );
                        return logLevel switch
                        {
                            'i' => LogLevel.Info,
                            't' => LogLevel.Trace,
                            'W' => LogLevel.Warn,
                            'E' => LogLevel.Error,
                            'F' => LogLevel.Fatal,
                            'd' => LogLevel.Debug,
                            ' ' => typeMap[monitorId].Item3,
                            _ => throw new InvalidDataException( $"Unknown log level '{logLevel}'" ),
                        };
                    }
                    LogLevel logLevel = ParseLogLevel( ref reader );

                    Is( ref reader, " " );
                    (LogEntryType type, int depth) = ParseIndent( ref reader );
                    if( true ) //wtf
                    {
                        CKTrait trait = ParseTrait( ref reader );
                        string logText = ParseText( ref reader, depth, dateTimeSize );
                        const string monitorPrefix = "Monitor: ~";
                        if( logText.StartsWith( monitorPrefix ) )
                        {
                            string guid = logText.Substring( monitorPrefix.Length, 11 );
                            idMap.Add( monitorId, guid );
                        }
                        else if( !notNewMonitor )
                        {
                            throw new InvalidDataException( "Unknown monitor ID." );
                        }

                        (LogEntryType, DateTimeStamp, LogLevel) SafeGetVal( string monitorId )
                        {
                            if( !typeMap.TryGetValue( monitorId, out var vals ) )
                            {
                                return (LogEntryType.None, DateTimeStamp.Invalid, LogLevel.Fatal);
                            }
                            return vals;
                        }

                        IMulticastLogEntry entry = type switch
                        {
                            LogEntryType.Line => LogEntry.CreateMulticastLog( "old-ckmon", idMap[monitorId], SafeGetVal( monitorId ).Item1, SafeGetVal( monitorId ).Item2, depth, logText, stamp, logLevel, null, 0, trait, null ),
                            LogEntryType.OpenGroup => LogEntry.CreateMulticastOpenGroup( "old-ckmon", idMap[monitorId], SafeGetVal( monitorId ).Item1, SafeGetVal( monitorId ).Item2, depth, logText, stamp, logLevel, null, 0, trait, null ),
                            LogEntryType.CloseGroup => LogEntry.CreateMulticastCloseGroup( "old-ckmon", idMap[monitorId], SafeGetVal( monitorId ).Item1, SafeGetVal( monitorId ).Item2, depth, stamp, logLevel, null ),
                            _ => throw new InvalidOperationException(),
                        };
                        bf.Write( entry );
                    }
                    else
                    {
                        reader.TryAdvanceTo( '\n' );
                    }
                    typeMap[monitorId] = (type, stamp, logLevel);
                }
            }

            string ParseText( ref SequenceReader<char> reader, int depth, int dateTimeSize )
            {
                sb.Clear();
                while( true )
                {
                    reader.TryReadTo( out ReadOnlySpan<char> val, '\n', false );
                    if( val[^1] == '\r' ) val = val[..^1];
                    reader.Advance( 1 );
                    sb.Append( val );
                    if( !reader.TryPeek( out char peek ) || peek != ' ' )
                    {
                        break;
                    }
                    sb.Append( '\n' );
                    int indent = dateTimeSize + 10 + depth * 2;
                    reader.Advance( indent );
                }
                return sb.ToString();
            }

            CKTrait ParseTrait( ref SequenceReader<char> reader )
            {
                return ActivityMonitor.Tags.Empty;//doesnt work on dan logs.
                if( reader.TryPeek( out char open ) && open != '[' ) return ActivityMonitor.Tags.Empty;
                reader.TryReadTo( out ReadOnlySpan<char> trait, ']' );
                Is( ref reader, " " );
                return ActivityMonitor.Tags.Register( trait.ToString() );
            }


            (LogEntryType type, int depth) ParseIndent( ref SequenceReader<char> reader )
            {
                int depth = 0;
                LogEntryType type = LogEntryType.Line;
                while( reader.TryPeek( out char c ) && (c == '|' || c == '>' || c == '<') )
                {
                    reader.Advance( 1 );
                    Is( ref reader, " " );
                    depth++;
                    type = c switch
                    {
                        '>' => LogEntryType.OpenGroup,
                        '<' => LogEntryType.CloseGroup, //TODO: parse conclusions.
                        _ => LogEntryType.Line
                    };
                }
                return (type, depth);
            }



            (DateTime dateTime, int dateTimeSize) ParseTimeStamp( ref SequenceReader<char> reader )
            {
                reader.TryReadTo( out ReadOnlySpan<char> datetime, '~' );
                datetime = datetime[..^1];

                //Parse date of format '2022-01-14 03h15.53.8206192'
                bool res = DateTime.TryParseExact( datetime,
                                            datetime.Length == 27 ? @"yyyy-MM-dd HH\hmm.ss.fffffff" : @"yyyy-MM-dd HH\hmm.ss.fff",
                                            CultureInfo.InvariantCulture,
                                            DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
                                            out DateTime parsed
                                            );
                if( !res )
                {
                    throw new InvalidDataException( "Invalid date format detected." );
                }
                return (parsed, datetime.Length);
            }

            string ParseMonitorId( ref SequenceReader<char> reader )
            {
                Memory<char> buffer = new char[3];
                reader.TryCopyTo( buffer.Span );
                reader.Advance( 3 );
                return new string( buffer.Span );
            }

            void Is( ref SequenceReader<char> reader, string expected )
            {
                ReadOnlySpan<char> buff = expected.AsSpan();
                while( buff.Length > 0 )
                {
                    ReadOnlySpan<char> curr = reader.UnreadSpan;
                    if( curr.Length > buff.Length ) curr = curr[..buff.Length];
                    if( buff.SequenceEqual( curr ) )
                    {
                        reader.Advance( curr.Length );
                        buff = buff.Slice( curr.Length );
                    }
                    else
                    {
                        throw new Exception( $"Expected '{expected}' but found '{curr.ToString()}'" );
                    }
                }
            }

        }
    }
}
