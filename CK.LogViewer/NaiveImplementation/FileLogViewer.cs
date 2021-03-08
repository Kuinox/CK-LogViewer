using CK.Monitoring;
using System;
using System.Collections.Generic;
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

        public void NaiveJSONDump( Utf8JsonWriter writer )
        {
            writer.WriteStartArray();
            while( _logReader.MoveNext() )
            {
                ILogEntry curr = _logReader.Current;

                switch( curr.LogType )
                {
                    case LogEntryType.Line:
                        WriteLog( writer, curr );
                        break;
                    case LogEntryType.OpenGroup:
                        writer.WriteStartObject();
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

        void WriteLog( Utf8JsonWriter writer, ILogEntry logEntry )
        {
            writer.WriteStartObject();
            writer.WriteBoolean( "isGroup", false );
            writer.WriteString( "text", logEntry.Text );
            writer.WriteEndObject();
        }

    }
}
