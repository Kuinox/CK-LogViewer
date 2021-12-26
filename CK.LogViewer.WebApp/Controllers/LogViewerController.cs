using CK.Core;
using Microsoft.AspNetCore.Http;
using CK.Monitoring;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using CK.LogViewer.Enumerable;
using System;
using System.Net.Http;
using Microsoft.Extensions.Options;
using CK.LogViewer.WebApp.Configuration;
using System.Security.Cryptography;

namespace CK.LogViewer.WebApp.Controllers
{
    [ApiController]
    [Route( "/api/[controller]" )]
    public class LogViewerController : ControllerBase
    {
        readonly IActivityMonitor _m;
        readonly IOptions<LogViewerConfig> _config;
        readonly HttpClient _httpClient;

        public LogViewerController( IActivityMonitor m, IHttpClientFactory httpClientFactory, IOptions<LogViewerConfig> config )
        {
            _m = m;
            _config = config;
            _httpClient = httpClientFactory.CreateClient();
        }

        [HttpGet( "{logName}" )]
        public async Task GetLogJson( string logName, [FromQuery] int depth = -1, int groupOffset = -1 )
        {
            await using( Utf8JsonWriter writer = new( HttpContext.Response.Body ) )
            using( LogReader logReader = LogReader.Open( _storagePath.AppendPart( logName ).AppendPart( "log.ckmon" ), groupOffset < 0 ? 0 : groupOffset ) )
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

        [HttpPost( "{logname}/upload" )]
        public async Task<IActionResult> UploadToPublicInstance( string logName )
        {
            if( logName.IndexOfAny( Path.GetInvalidFileNameChars() ) >= 0 ) return new BadRequestResult();
            string logPath = _storagePath.AppendPart( logName ).AppendPart( "log.ckmon" );
            if( !System.IO.File.Exists( logPath ) ) return NotFound();
            using( FileStream fs = System.IO.File.OpenRead( logPath ) )
            {
                HttpResponseMessage resp = await _httpClient.PostAsync( new Uri( _config.Value.PublicInstanceUri, "/api/LogViewer" ), new MultipartFormDataContent()
                {
                    { new StreamContent( fs ), "files", "log.ckmon"}
                } );
                if( !resp.IsSuccessStatusCode )
                {
                    throw new InvalidOperationException( resp.ReasonPhrase );
                }
                string response = await resp.Content.ReadAsStringAsync();
                string newUri = new Uri( _config.Value.PublicInstanceUri, "#" + response ).ToString();
                return Ok( newUri );
            }
        }

        readonly NormalizedPath _storagePath = "saveLog";

        [HttpPost]
        public async Task<IActionResult> UploadLog( IList<IFormFile> files )
        {
            if( files.Count == 0 ) return new BadRequestResult();
            byte[] finalResult;
            string shaString;
            using( TemporaryFile temporaryFile = new() )
            {
                using( var stream = files[0].OpenReadStream() )
                {
                    HashAlgorithm hashAlg = HashAlgorithm.Create( "SHA256" )!;
                    using( var tempStream = System.IO.File.OpenWrite( temporaryFile.Path ) )
                    using( CryptoStream sHA512Stream = new( stream, hashAlg, CryptoStreamMode.Read ) )
                    {
                        await sHA512Stream.CopyToAsync( tempStream );
                        finalResult = hashAlg.Hash!;
                    }
                }
                shaString = Convert.ToHexString( finalResult ).ToLower();
                NormalizedPath logFolder = _storagePath.AppendPart( shaString );
                NormalizedPath logPath = logFolder.AppendPart( "log.ckmon" );
                Directory.CreateDirectory( logFolder );
                System.IO.File.Move( temporaryFile.Path, logPath, true );
                temporaryFile.Detach();
            }
            return Ok( shaString );
        }
    }
}
