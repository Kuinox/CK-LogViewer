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
using CK.LogViewer.Enumerable;

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

        [HttpGet( "{logName}/group/{groupOffset}" )]
        public async Task GetGroupJson( string logName, long groupOffset )
        {
            string path = @$"{Directory.GetCurrentDirectory()}\saveLog\{logName}.ckmon";
            using( LogReader logReader = LogReader.Open( path, dataOffset: groupOffset ) )
            {
                var writer = new Utf8JsonWriter( HttpContext.Response.Body );
                HttpContext.Response.ContentType = "application/json";
                logReader.ToEnumerable()
                    .AddState()
                    .TakeOnlyGroup()
                    .WriteTo( writer );
                await writer.FlushAsync();
            }
        }

        [HttpGet( "{logName}" )]
        public async Task GetLogJson( [FromQuery] int depth, string logName )
        {
            string path = @$"{Directory.GetCurrentDirectory()}\saveLog\{logName}.ckmon";
            using( LogReader logReader = LogReader.Open( path ) )
            {
                var writer = new Utf8JsonWriter( HttpContext.Response.Body );
                HttpContext.Response.ContentType = "application/json";

                logReader.ToEnumerable()
                    .AddState()
                    .FilterDepth( depth + 1 )
                    .FoldAtDepth( depth )
                    .WriteTo( writer );

                await writer.FlushAsync();
            }

        }

        [HttpPost]
        public async Task<string> UploadLog( IList<IFormFile> files )
        {
            SHA512Value finalResult;
            using( TemporaryFile temporaryFile = new TemporaryFile() )
            {
                using( var stream = files[0].OpenReadStream() )
                {
                    using( var tempStream = System.IO.File.OpenWrite( temporaryFile.Path ) )
                    using( SHA512Stream sHA512Stream = new SHA512Stream( stream, true, true ) )
                    {
                        await sHA512Stream.CopyToAsync( tempStream );
                        finalResult = sHA512Stream.GetFinalResult();
                    }
                }

                if( !Directory.Exists( $@"{Directory.GetCurrentDirectory()}\saveLog" ) )
                {
                    Directory.CreateDirectory( $@"{ Directory.GetCurrentDirectory()}\saveLog" );
                }

                System.IO.File.Move( temporaryFile.Path, $@"{Directory.GetCurrentDirectory()}\saveLog\{finalResult}.ckmon", true );
                temporaryFile.Detach();

            }

            return finalResult.ToString();

        }
    }
}
