using CK.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace CK.LogViewer.WebApp.Controllers
{
    public class HomeController : ControllerBase
    {
        readonly IActivityMonitor _m;
        public IWebHostEnvironment HostingEnv { get; }

        
        public HomeController( IActivityMonitor m , IWebHostEnvironment env )
        {
            _m = m;
            HostingEnv = env;
        }


        [HttpGet]
        public IActionResult RedirectIndex()
        {
            return new PhysicalFileResult(
                Path.Combine( HostingEnv.WebRootPath, "index.html" ),
                new MediaTypeHeaderValue( "text/html" )
            );
        }
    }
}
