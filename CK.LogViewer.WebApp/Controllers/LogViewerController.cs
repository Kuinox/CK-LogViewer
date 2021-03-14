using CK.Core;
using CK.Monitoring;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

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

        [HttpGet]
        public async Task GetLogJson( int depth, int preloadDepth )
        {
            string path = @$"{Directory.GetCurrentDirectory()}\2016-01-20 18h59.18.2215043.ckmon";
            var writer = new Utf8JsonWriter( HttpContext.Response.Body );
            HttpContext.Response.ContentType = "application/json";
            JSONLogVisitor logViewer = new( writer, depth, LogReader.Open( path ) );
            logViewer.Visit();
            await writer.FlushAsync();
        }
    }
}
