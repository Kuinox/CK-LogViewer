using CK.Core;
using CK.Monitoring;
using System.Text.Json;

namespace CK.LogViewer
{
    public class JSONLogVisitor : ShallowLogVisitor
    {
        readonly Utf8JsonWriter _writer;

        public JSONLogVisitor( Utf8JsonWriter utf8JsonWriter, int unfoldedDepth, LogReader logReader ) : base( unfoldedDepth, logReader )
            => _writer = utf8JsonWriter;

        public override void Visit()
        {
            _writer.WriteStartArray();
            base.Visit();
            _writer.WriteEndArray();
        }

        protected override bool VisitCloseGroup( MulticastLogEntryWithOffset entry )
        {
            _writer.WriteEndArray();
            _writer.WritePropertyName( "closeLog" );
            WriteLog( entry );
            _writer.WriteEndObject();
            return base.VisitCloseGroup( entry );
        }

        protected override bool VisitOpenGroup( MulticastLogEntryWithOffset entry )
        {
            _writer.WriteStartObject();
            WriteCommonProperties( entry );
            _writer.WriteBoolean( "isGroup", true );
            _writer.WritePropertyName( "openLog" );
            WriteLog( entry );
            _writer.WriteStartArray( "groupLogs" );
            return base.VisitOpenGroup( entry );
        }

        protected override bool VisitLogLine( MulticastLogEntryWithOffset entry )
        {
            WriteLog( entry );
            return base.VisitLogLine( entry );
        }


        void WriteLog( MulticastLogEntryWithOffset entry )
        {
            _writer.WriteStartObject();
            WriteCommonProperties( entry );
            _writer.WriteBoolean( "isGroup", false );
            _writer.WriteString( "text", entry.Entry.Text );
            _writer.WriteEndObject();
        }


        void WriteCommonProperties( MulticastLogEntryWithOffset entry )
        {
            _writer.WriteNumber( "depth", entry.Entry.GroupDepth );
            _writer.WriteNumber( "offset", entry.Offset ); ;
            _writer.WriteNumber( "logLevel", (byte)entry.Entry.LogLevel );
            _writer.WriteString( "logTime", entry.Entry.LogTime.ToString() );
            _writer.WriteString( "monitorId", entry.Entry.MonitorId );

            if( entry.Entry.Exception != null )
            {
                WriteException( entry.Entry.Exception );
            }
        }

        void WriteException( CKExceptionData exceptionData )
        {
            _writer.WriteStartObject( "exception" );
            _writer.WriteString( "stackTrace", exceptionData.StackTrace );
            _writer.WriteString( "typeException", exceptionData.ExceptionTypeName );
            _writer.WriteString( "message", exceptionData.Message );
            _writer.WriteEndObject();
        }
    }
}
