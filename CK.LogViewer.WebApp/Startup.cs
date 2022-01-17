using CK.Core;
using CK.LogViewer.WebApp.Configuration;
using CK.LogViewer.WebApp.Handlers;
using CK.LogViewer.WebApp.Services;
using CK.MQTT;
using CK.MQTT.Client;
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
using System.Threading;
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
            MQTTConfiguration mqttConfig = Configuration.GetSection( "MQTT" ).Get<MQTTConfiguration>();
            services.Configure<MQTTConfiguration>( Configuration.GetSection( "MQTT" ) );
            IMqtt3Client client = MqttClient.Factory.CreateMQTT3Client( new( mqttConfig.ConnectionString ), 
                ( IActivityMonitor? m, DisposableApplicationMessage msg, CancellationToken token ) =>
            {
                // Will be replaced.
                return new ValueTask();
            } );
            services.AddSingleton( client );
            services.AddHttpClient();
            services.AddControllers();
            services
               .AddMvcCore( o =>
               {
                   o.EnableEndpointRouting = false;
               } );
            services.AddResponseCompression();
            services.AddSingleton<AppendToFileHandler>();
            services.AddSingleton<MQTTEmitterHandler>();
            services.AddHostedService<MqttService>();
            services.Configure<LogViewerConfig>( Configuration.GetSection( "LogViewerConfig" ) );
            services.Configure<LogPersistanceConfig>( Configuration.GetSection( "LogPersistanceConfig" ) );
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
