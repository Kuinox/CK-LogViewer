using CK.Core;
using Microsoft.AspNetCore.Http;
using CK.Monitoring;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using CK.Text;
using CK.LogViewer.Enumerable;
using System;

namespace CK.LogViewer.WebApp.Controllers
{
    [ApiController]
    [Route( "/api/[controller]" )]
    public class LogViewerController : ControllerBase
    {
        readonly IActivityMonitor _m;

        public LogViewerController( IActivityMonitor m ) => _m = m;

        [HttpGet( "{logName}" )]
        public async Task GetLogJson( string logName, [FromQuery] int depth = -1, int groupOffset = -1 )
        {
            await using( Utf8JsonWriter writer = new( HttpContext.Response.Body ) )
            using( LogReader logReader = LogReader.Open( "saveLog/" + logName + "/log.ckmon", groupOffset < 0 ? 0 : groupOffset ) )
            {
                HttpContext.Response.ContentType = "application/json";
                var logsToWrite = logReader
                    .ToEnumerable()
                    .AddState();
                if( depth >= 0 )
                {
                    logsToWrite = logsToWrite.FilterDepth( depth + 1 );
                }

                if( groupOffset >= 0 )
                {
                    logsToWrite = logsToWrite.TakeOnlyCurrentGroupContent();
                }
                logsToWrite.WriteTo( writer );
                await writer.FlushAsync();
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
                temporaryFile.Detach();
            }
            return finalResult.ToString();
        }
    }
}
