using CK.Core;
using CK.Monitoring;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace CK.LogViewer
{
    public class FileLogViewer
    {
        readonly LogReader _logReader;
        public FileLogViewer( string logPath )
        {
            _logReader = LogReader.Open( logPath );
        }

        public FileLogViewer( Stream log )
        {
            _logReader = LogReader.Open( log );
        }

        public void NaiveJSONDump( Utf8JsonWriter writer )
        {
            writer.WriteStartArray();
            while( _logReader.MoveNext() )
            {
                MulticastLogEntryWithOffset curr = _logReader.CurrentMulticastWithOffset;

                switch( curr.Entry.LogType )
                {
                    case LogEntryType.Line:
                        WriteLog( writer, curr );
                        break;
                    case LogEntryType.OpenGroup:
                        writer.WriteStartObject();
                        WriteCommonProperties( writer, curr );
                        writer.WriteBoolean( "isGroup", true );
                        writer.WritePropertyName( "openLog" );
                        WriteLog( writer, curr );
                        writer.WriteStartArray( "groupLogs" );
                        break;
                    case LogEntryType.CloseGroup:
                        writer.WriteEndArray();
                        writer.WritePropertyName( "closeLog" );
                        WriteLog( writer, curr );
                        writer.WriteEndObject();
                        break;
                }
            }
            writer.WriteEndArray();
        }

        static void WriteCommonProperties( Utf8JsonWriter writer, MulticastLogEntryWithOffset logEntry )
        {
            writer.WriteNumber( "offset", logEntry.Offset ); ;
            writer.WriteNumber( "logLevel", (byte)logEntry.Entry.LogLevel );
            writer.WriteString( "logTime", logEntry.Entry.LogTime.ToString() );
            writer.WriteString( "monitorId", logEntry.Entry.MonitorId );

            if( logEntry.Entry.Exception != null )
            {
                WriteException( writer, logEntry.Entry.Exception );
            }
        }

        static void WriteLog( Utf8JsonWriter writer, MulticastLogEntryWithOffset logEntry )
        {
            writer.WriteStartObject();
            WriteCommonProperties( writer, logEntry );
            writer.WriteBoolean( "isGroup", false );
            writer.WriteString( "text", logEntry.Entry.Text );
            writer.WriteEndObject();
        }

        static void WriteException( Utf8JsonWriter writer, CKExceptionData exceptionData )
        {
            writer.WriteStartObject( "exception" );
            writer.WriteString( "stackTrace", exceptionData.StackTrace );
            writer.WriteString( "typeException", exceptionData.ExceptionTypeName );
            writer.WriteString( "message", exceptionData.Message );
            writer.WriteEndObject();
        }

    }
}
