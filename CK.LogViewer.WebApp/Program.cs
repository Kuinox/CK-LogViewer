using CK.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CK.LogViewer.WebApp
{
    public class Program
    {
        public static void Main( string[] args )
        {
            ActivityMonitor.DefaultFilter = LogFilter.Debug;
            bool isService = !(Debugger.IsAttached || args.Contains( "--console" ));
            if( isService )
            {
                string binDir = GetAppDir();
                Environment.CurrentDirectory = binDir;
            }
            CreateHostBuilder( isService, args ).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder( bool isService, string[] args )
        {
            var host = Host.CreateDefaultBuilder( args );
            if( isService )
            {
                host = host.UseWindowsService();
            }
            return host.UseMonitoring()
                .ConfigureAppConfiguration( ( context, config ) =>
                {
                    config
                       .AddJsonFile( "appsettings.json", optional: false, reloadOnChange: true )
                       .AddJsonFile( $"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true )
                       .AddJsonFile( $"appsettings.Desktop.json", optional: true );
                } )
                .ConfigureWebHostDefaults( webBuilder =>
                 {
                     webBuilder.UseStartup<Startup>();
                 } );

        }
        static string GetAppDir()
        {
            return Path.GetDirectoryName( Path.GetFullPath( Assembly.GetExecutingAssembly().Location ) )!;
        }
    }
}
