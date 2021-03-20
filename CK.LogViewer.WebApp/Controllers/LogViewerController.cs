using CK.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace CK.LogViewer.WebApp.Controllers
{
    [ApiController]
    [Route( "[controller]" )]
    public class LogViewerController : ControllerBase
    {
        readonly IActivityMonitor _m;

        public LogViewerController( IActivityMonitor m )
        {
            _m = m;
        }

        [HttpGet]
        public async Task GetLogJson()
        {
            string path = @$"{Directory.GetCurrentDirectory()}\2016-01-20 18h59.18.2215043.ckmon";
            using( FileLogViewer logViewer = new( path ) )
            {
                HttpContext.Response.ContentType = "application/json";
                var writer = new Utf8JsonWriter( HttpContext.Response.Body );
                logViewer.NaiveJSONDump( writer );
                await writer.FlushAsync();

            }
        }

        [HttpGet( "{logName}" )]
        public async Task GetExistLogJson( string logName )
        {
            string path = @$"{Directory.GetCurrentDirectory()}\saveLog\{logName}.ckmon";
            using( FileLogViewer logViewer = new( path ) )
            {
                HttpContext.Response.ContentType = "application/json";
                var writer = new Utf8JsonWriter( HttpContext.Response.Body );
                logViewer.NaiveJSONDump( writer );
                await writer.FlushAsync();
            }

        }

        [HttpPost]
        public async Task UploadLog( IList<IFormFile> files )
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

            using( FileLogViewer logViewer = new( $@"{Directory.GetCurrentDirectory()}\saveLog\{finalResult}.ckmon" ) )
            {
                HttpContext.Response.ContentType = "application/json";
                var writer = new Utf8JsonWriter( HttpContext.Response.Body );
                logViewer.NaiveJSONDump( writer );
                await writer.FlushAsync();
            }

        }
    }
}
