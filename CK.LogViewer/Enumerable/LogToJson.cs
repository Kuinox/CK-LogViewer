using CK.Core;
using CK.Monitoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static CK.LogViewer.EnumerableLogStatsExtensions;

namespace CK.LogViewer
{
    public static class EnumerableLogToJsonExtensions
    {
        public static void WriteTo( this IEnumerable<LogEntryWithState> @this, Utf8JsonWriter writer )
        {
            writer.WriteStartArray();
            foreach( LogEntryWithState entry in @this )
            {
                switch( entry.LogType )
                {
                    case LogEntryType.Line:
                        WriteLog( entry, writer );
                        break;
                    case LogEntryType.OpenGroup:
                        WriteOpenGroup( entry, writer );
                        break;
                    case LogEntryType.CloseGroup:
                        WriteCloseGroup( entry, writer );
                        break;
                };
            }
            writer.WriteEndArray();

        }

        public static void WriteSingleGroupOrEntry( this IEnumerable<LogEntryWithState> @this, Utf8JsonWriter writer )
        {
            foreach( LogEntryWithState entry in @this )
            {
                switch( entry.LogType )
                {
                    case LogEntryType.Line:
                        WriteLog( entry, writer );
                        break;
                    case LogEntryType.OpenGroup:
                        WriteOpenGroup( entry, writer );
                        break;
                    case LogEntryType.CloseGroup:
                        WriteCloseGroup( entry, writer );
                        break;
                };
            }
        }

        static void WriteCloseGroup( LogEntryWithState entry, Utf8JsonWriter writer )
        {
            writer.WriteEndArray();
            writer.WritePropertyName( "closeLog" );
            WriteLog( entry, writer );
            writer.WriteStartObject( "stats" );
            var stats = entry.Stats;
            foreach( KeyValuePair<LogLevel, int> statPair in stats )
            {
                writer.WriteNumber( statPair.Key.ToString(), statPair.Value );
            }
            writer.WriteEndObject();
            writer.WriteEndObject();
        }

        static void WriteOpenGroup( LogEntryWithState entry, Utf8JsonWriter writer )
        {
            writer.WriteStartObject();
            //WriteCommonProperties( entry, writer );
            writer.WriteBoolean( "isGroup", true );
            writer.WritePropertyName( "openLog" );
            WriteLog( entry, writer );
            writer.WriteStartArray( "groupLogs" );
        }

        static void WriteLog( LogEntryWithState entry, Utf8JsonWriter writer )
        {
            writer.WriteStartObject();
            WriteCommonProperties( entry, writer );
            writer.WriteBoolean( "isGroup", false );
            writer.WriteString( "text", entry.Text );
            writer.WriteEndObject();
        }


        static void WriteCommonProperties( LogEntryWithState entry, Utf8JsonWriter writer )
        {
            writer.WriteNumber( "depth", entry.GroupDepth );
            writer.WriteNumber( "offset", entry.Offset ); ;
            writer.WriteNumber( "logLevel", (byte)entry.LogLevel );
            writer.WriteString( "logTime", entry.LogTime.ToString() );
            writer.WriteString( "monitorId", entry.MonitorSimpleId.ToString() );

            if( entry.Exception != null )
            {
                WriteException( entry.Exception, writer );
            }
        }

        static void WriteException( CKExceptionData exceptionData, Utf8JsonWriter writer )
        {
            writer.WriteStartObject( "exception" );
            writer.WriteString( "stackTrace", exceptionData.StackTrace );
            writer.WriteString( "typeException", exceptionData.ExceptionTypeName );
            writer.WriteString( "message", exceptionData.Message );
            writer.WriteEndObject();
        }
    }
}
