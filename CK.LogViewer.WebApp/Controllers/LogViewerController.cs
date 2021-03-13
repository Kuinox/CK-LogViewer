using CK.Core;
using Microsoft.AspNetCore.Http;
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
            FileLogViewer logViewer = new( path );
            HttpContext.Response.ContentType = "application/json";
            var writer = new Utf8JsonWriter( HttpContext.Response.Body );
            logViewer.NaiveJSONDump( writer );
            await writer.FlushAsync();
        }

        [HttpPost]
        public async Task UploadLog( IList<IFormFile> files )
        {
            FileLogViewer logViewer = new( files[0].OpenReadStream() );
            HttpContext.Response.ContentType = "application/json";
            var writer = new Utf8JsonWriter( HttpContext.Response.Body );
            logViewer.NaiveJSONDump( writer );
            await writer.FlushAsync();
        }
    }
}
