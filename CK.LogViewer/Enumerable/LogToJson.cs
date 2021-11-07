using CK.Core;
using CK.Monitoring;
using System.Collections.Generic;
using System.Text.Json;
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
                WriteLog( entry, writer );
            }
            writer.WriteEndArray();
        }


        static void WriteLog( LogEntryWithState entry, Utf8JsonWriter writer )
        {
            writer.WriteStartObject();
            writer.WriteBoolean( "isGroup", false );
            writer.WriteString( "text", entry.Text );
            writer.WriteNumber( "offset", entry.Offset ); ;
            writer.WriteNumber( "logLevel", (byte)entry.LogLevel );
            writer.WriteString( "logTime", entry.LogTime.ToString() );
            writer.WriteString( "monitorId", entry.MonitorId.ToString() );
            writer.WriteNumber( "logType", (int)entry.LogType );
            writer.WriteNumber( "groupOffset", entry.GroupOffset );
            WriteException( entry.Exception, writer );
            WriteParentsLogLevel( entry, writer );
            WriteStats( entry, writer );
            writer.WriteEndObject();
        }

        static void WriteStats( LogEntryWithState entry, Utf8JsonWriter writer )
        {
            if( entry.LogType != LogEntryType.OpenGroup ) return;
            writer.WritePropertyName( "stats" );
            writer.WriteStartObject();
            foreach( var stat in entry.Stats )
            {
                writer.WriteNumber( stat.Key.ToString(), stat.Value );
            }
            writer.WriteEndObject();
        }

        static void WriteParentsLogLevel( LogEntryWithState entry, Utf8JsonWriter writer )
        {
            writer.WritePropertyName( "parentsLogLevel" );
            writer.WriteStartArray();
            foreach( var logLevel in entry.ParentsLogLevel )
            {
                writer.WriteStartObject();
                writer.WritePropertyName( "logLevel" );
                writer.WriteNumberValue( (int)logLevel.logLevel );
                writer.WritePropertyName( "groupOffset" );
                writer.WriteNumberValue( logLevel.groupOffset );
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
        }

        static void WriteException( CKExceptionData? exceptionData, Utf8JsonWriter writer )
        {
            if( exceptionData == null ) return;
            writer.WriteStartObject( "exception" );
            writer.WriteString( "stackTrace", exceptionData.StackTrace );
            writer.WriteString( "typeException", exceptionData.ExceptionTypeName );
            writer.WriteString( "message", exceptionData.Message );
            writer.WriteEndObject();
        }
    }
}
