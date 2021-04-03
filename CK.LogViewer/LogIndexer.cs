using CK.Core;
using CK.Monitoring;
using CK.Text;
using J2N.Text;
using Lucene.Net.Analysis.Path;
using Lucene.Net.Analysis.Reverse;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace CK.LogViewer
{
    public class LogIndexer : IDisposable
    {
        // Ensures index backward compatibility
        const LuceneVersion _appLuceneVersion = LuceneVersion.LUCENE_48;
        readonly FSDirectory _fSDirectory;
        readonly StandardAnalyzer _analyzer;
        readonly IndexWriter _writer;
        Dictionary<Guid, int> _monitors;

        LogIndexer( FSDirectory fSDirectory, StandardAnalyzer analyzer, IndexWriter writer )
        {
            _fSDirectory = fSDirectory;
            _analyzer = analyzer;
            _writer = writer;
            _monitors = new Dictionary<Guid, int>();
        }

        public static LogIndexer Create( string indexesPath )
        {
            FSDirectory res = FSDirectory.Open( indexesPath );
            StandardAnalyzer analyzer = new( _appLuceneVersion );
            IndexWriterConfig indexConfig = new( _appLuceneVersion, analyzer );
            IndexWriter writer = new( res, indexConfig );
            return new LogIndexer( res, analyzer, writer );
        }

        public void IndexLogs( IEnumerable<LogEntryWithState> logs )
        {
            foreach( var log in logs )
            {
                IndexLogEntry( log );
            }
            _writer.Commit();
        }

        readonly Stack<string> _currentPathStack = new();

        class MyStack
        {
            public string[] Array = null!;
            public int Size;
        }

        void IndexLogEntry( LogEntryWithState log )
        {
            string currentPath;

            MyStack myStack = Unsafe.As<MyStack>( _currentPathStack ); // Ok i'm sorry I just wanted to have fun.
            // This is a cheap way to do a .Reverse on the stack.
            switch( log.LogType )
            {
                case LogEntryType.Line:
                    currentPath = myStack.Array.Take( myStack.Size ).Concatenate( "/" );
                    break;
                case LogEntryType.OpenGroup:
                    _currentPathStack.Push( log.Offset.ToString() );
                    currentPath = myStack.Array.Take( myStack.Size ).Concatenate( "/" );
                    break;
                case LogEntryType.CloseGroup:
                    currentPath = myStack.Array.Take( myStack.Size ).Concatenate( "/" );
                    _currentPathStack.Pop();
                    break;
                default:
                    throw new InvalidDataException();
            }

            Document doc = new()
            {
                new NumericDocValuesField( "logType", (byte)log.LogType ),
                new StoredField( "logType", (byte)log.LogType ),
                new TextField( "text", log.Text ?? "", Field.Store.YES ),
                new TextField( "emitterFileName", log.FileName ?? "", Field.Store.YES ),
                new Int32Field( "emitterLine", log.LineNumber, Field.Store.YES ),
                new NumericDocValuesField( "logLevel", (byte)(log.LogLevel & LogLevel.Mask) ),
                new StoredField( "logLevel", (byte)(log.LogLevel & LogLevel.Mask) ),
                new NumericDocValuesField( "logTime", log.LogTime.TimeUtc.Ticks ),
                new StoredField( "logTime", log.LogTime.TimeUtc.Ticks ),
                new NumericDocValuesField( "logTimeUniquifier", log.LogTime.Uniquifier ),
                new StoredField( "logTimeUniquifier", log.LogTime.Uniquifier ),
                new NumericDocValuesField( "offset", log.Offset ),
                new StoredField( "offset", log.Offset ),
                new TextField( "monitorId", GetMonitorId( log.MonitorId ), Field.Store.YES ),
                new Int32Field( "depth", log.LogType != LogEntryType.CloseGroup ? log.GroupDepth : log.GroupDepth - 1, Field.Store.YES ),
                new TextField( "groupPath", currentPath, Field.Store.YES ),
                new TextField( "groupPath", new PathHierarchyTokenizer( new StringReader( currentPath ) ) ),
            };


            bool hasException = log.Exception != null;
            doc.Add( new StringField( "hasException", hasException.ToString(), Field.Store.YES ) );
            if( log.Exception != null )
            {
                doc.Add( new TextField( "exceptionMessage", log.Exception.Message, Field.Store.YES ) );
                doc.Add( new TextField( "typeException", log.Exception.ExceptionTypeName, Field.Store.YES ) );
                doc.Add( new TextField( "stackTrace", log.Exception.StackTrace, Field.Store.YES ) );
            }

            if( log.LogType == LogEntryType.CloseGroup )
            {
                foreach( var stat in log.Stats )
                {
                    doc.Add( new Int32Field( "stat" + stat.Key, stat.Value, Field.Store.YES ) );
                }
            }
            _writer.AddDocument( doc );
        }

        private string GetMonitorId( Guid monitorId )
        {
            int value;
            if( _monitors.TryGetValue( monitorId, out value ) )
            {
                return value.ToString();
            }
            else
            {
                var newId = _monitors.Count + 1;
                _monitors.Add(monitorId, newId);
                return newId.ToString();
            }
        }

        public void Dispose()
        {
            _fSDirectory.Dispose();
            _analyzer.Dispose();
            _writer.Dispose();
        }
    }
}
