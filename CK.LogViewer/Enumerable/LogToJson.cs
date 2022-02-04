using CK.Core;
using CK.Monitoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using static CK.LogViewer.Enumerable.EnumerableLogStatsExtensions;

namespace CK.LogViewer.Enumerable
{
    public static class LogToJson
    {

        public static async Task WriteToAsync( this IObservable<LogEntryWithState> @this, Utf8JsonWriter writer )
        {
            writer.WriteStartArray();
            await @this.ForEachAsync( ( entry ) => WriteLog( entry, writer ) );
            writer.WriteEndArray();
        }

        public static void WriteLog( LogEntryWithState entry, Utf8JsonWriter writer )
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
            WriteConclusions( entry, writer );
            writer.WriteEndObject();
        }

        private static void WriteConclusions( LogEntryWithState entry, Utf8JsonWriter writer )
        {
            if( entry.Conclusions != null && entry.Conclusions.Count > 0 )
            {
                writer.WriteStartArray( "conclusions" );
                foreach( ActivityLogGroupConclusion conclusion in entry.Conclusions )
                {
                    writer.WriteStartObject();
                    writer.WriteString( "text", conclusion.Text );
                    writer.WriteString( "tag", conclusion.Tag );
                    writer.WriteEndObject();
                }
                writer.WriteEndArray();
            }
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
