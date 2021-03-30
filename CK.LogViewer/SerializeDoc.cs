using CK.Core;
using CK.Monitoring;
using Lucene.Net.Documents;
using Lucene.Net.Documents.Extensions;
using System;
using System.IO;
using System.Text.Json;

namespace CK.LogViewer
{
    public class SerializeDoc
    {
        public static void Convert( int docId, Document doc, Utf8JsonWriter writer )
        {
            switch( (LogEntryType)doc.GetField( "logType" ).GetByteValueOrDefault() )
            {
                case LogEntryType.None:
                    throw new InvalidDataException();
                case LogEntryType.Line:
                    WriteLog(docId, doc, writer );
                    break;
                case LogEntryType.OpenGroup:
                    WriteOpenGroup(docId, doc, writer );
                    break;
                case LogEntryType.CloseGroup:
                    WriteCloseGroup(docId, doc, writer );
                    break;
            }
        }

        static void WriteCloseGroup( int docId, Document doc, Utf8JsonWriter writer )
        {
            writer.WriteEndArray();
            writer.WritePropertyName( "closeLog" );
            WriteLog(docId, doc, writer );
            writer.WriteStartObject( "stats" );
            foreach( LogLevel item in Enum.GetValues<LogLevel>() )
            {
                if( item == LogLevel.IsFiltered || item == LogLevel.Mask || item == LogLevel.NumberOfBits || item == LogLevel.None ) continue;
                int? field = doc.GetField( "stat" + item )?.GetInt32Value();
                if( field.HasValue )
                {
                    writer.WriteNumber( item.ToString(), field.Value );
                }
            }
            writer.WriteEndObject();
            writer.WriteEndObject();
        }

        static void WriteOpenGroup( int docId, Document doc, Utf8JsonWriter writer )
        {
            writer.WriteStartObject();
            WriteCommonProperties(docId, doc, writer );
            writer.WriteBoolean( "isGroup", true );
            writer.WritePropertyName( "openLog" );
            WriteLog(docId, doc, writer );
            writer.WriteStartArray( "groupLogs" );
        }

        static void WriteLog( int docId, Document doc, Utf8JsonWriter writer )
        {
            writer.WriteStartObject();
            WriteCommonProperties(docId, doc, writer );
            writer.WriteBoolean( "isGroup", false );
            writer.WriteString( "text", doc.Get( "text" ) );
            writer.WriteEndObject();
        }


        static void WriteCommonProperties( int docId, Document doc, Utf8JsonWriter writer )
        {
            writer.WriteNumber( "id", docId );
            writer.WriteNumber( "logLevel", doc.GetField( "logLevel" ).GetByteValue()!.Value );
            long ticks = doc.GetField( "logTime" ).GetInt64Value()!.Value;
            byte uniquifer = doc.GetField( "logTimeUniquifier" ).GetByteValue()!.Value;
            writer.WriteString( "logTime", new DateTimeStamp( new DateTime( ticks, DateTimeKind.Utc ), uniquifer ).ToString() );
            writer.WriteString( "monitorId", doc.Get( "monitorId" ) );

            if( doc.Get( "hasException" ) == "True" )
            {
                WriteException( doc, writer );
            }
        }

        static void WriteException( Document doc, Utf8JsonWriter writer )
        {
            writer.WriteStartObject( "exception" );
            writer.WriteString( "stackTrace", doc.Get( "stackTrace" ) );
            writer.WriteString( "typeException", doc.Get( "typeException" ) );
            writer.WriteString( "message", doc.Get( "message" ) );
            writer.WriteEndObject();
        }
    }
}
