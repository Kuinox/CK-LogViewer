using CK.Core;
using Microsoft.AspNetCore.Http;
using CK.Monitoring;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CK.Text;
using Lucene.Net.Documents;

namespace CK.LogViewer.WebApp.Controllers
{
    [ApiController]
    [Route( "/api/[controller]" )]
    public class LogViewerController : ControllerBase
    {
        readonly IActivityMonitor _m;

        public LogViewerController( IActivityMonitor m )
        {
            _m = m;
        }

        Task DocumentsToJson( Utf8JsonWriter writer, IEnumerable<(int, Document)> documents )
        {
            writer.WriteStartArray();
            foreach( var log in documents )
            {
                SerializeDoc.Convert( log.Item1, log.Item2, writer );
            }
            writer.WriteEndArray();
            return writer.FlushAsync();
        }

        [HttpGet( "{logName}" )]
        public async Task GetLogJson( string logName, [FromQuery] int depth = 2, int scopedOnGroupId = -1 )
        {
            NormalizedPath logFolder = "saveLog";
            NormalizedPath indexPaths = logFolder.AppendPart( logName ).AppendPart( "indexes" );
            await using( Utf8JsonWriter writer = new( HttpContext.Response.Body ) )
            using( LogSearcher searcher = LogSearcher.Create( indexPaths ) )
            {
                var docs = searcher.FilteredLogs( depth, scopedOnGroupId );
                await DocumentsToJson( writer, docs );
            }
        }

        [HttpPost]
        public async Task<string> UploadLog( IList<IFormFile> files )
        {
            SHA1Value finalResult;
            using( TemporaryFile temporaryFile = new() )
            {
                using( var stream = files[0].OpenReadStream() )
                {
                    using( var tempStream = System.IO.File.OpenWrite( temporaryFile.Path ) )
                    using( SHA1Stream sHA512Stream = new( stream, true, true ) )
                    {
                        await sHA512Stream.CopyToAsync( tempStream );
                        finalResult = sHA512Stream.GetFinalResult();
                    }
                }
                NormalizedPath storagePath = "saveLog";
                string shaString = finalResult.ToString();
                NormalizedPath logFolder = storagePath.AppendPart( shaString );
                NormalizedPath logPath = logFolder.AppendPart( "log.ckmon" );
                Directory.CreateDirectory( logFolder );
                System.IO.File.Move( temporaryFile.Path, logPath, true );
                using( LogIndexer indexer = LogIndexer.Create( logFolder.AppendPart( "indexes" ) ) )
                using( LogReader reader = LogReader.Open( logPath ) )
                {
                    indexer.IndexLogs( reader.ToEnumerable().ComputeState() );
                }
                temporaryFile.Detach();
            }
            return finalResult.ToString();
        }
    }
}
