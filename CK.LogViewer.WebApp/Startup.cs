using CK.LogViewer.WebApp.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CK.LogViewer.WebApp
{
    public class Startup
    {
        public Startup( IConfiguration configuration )
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices( IServiceCollection services )
        {
            services.AddHttpClient();
            services.AddControllers();
            services
               .AddMvcCore( o =>
               {
                   o.EnableEndpointRouting = false;
               } );
            services.AddResponseCompression();
            services.Configure<LogViewerConfig>( Configuration.GetSection( "LogViewerConfig" ) );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure( IApplicationBuilder app, IWebHostEnvironment env )
        {
            app.UseStaticFiles();
            app.UseFileServer();
            app.UseMvc( builder =>
            {
                builder.MapRoute( "default", "{controller}/{action}/{id?}" );
            }
            );
            app.UseResponseCompression();
        }
    }
}
