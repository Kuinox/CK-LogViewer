using CK.LogViewer.WebApp.Configuration;
using CK.LogViewer.WebApp.Handlers;
using CK.LogViewer.WebApp.Services;
using CK.MQTT;
using CK.MQTT.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
            var splitted = mqttConfig.ConnectionString.Split( ':' );
            var channel = new TcpChannel( splitted[0], int.Parse( splitted[1] ) );
            var config = new Mqtt3ClientConfiguration()
            {
                KeepAliveSeconds = 0,
                DisconnectBehavior = DisconnectBehavior.AutoReconnect,
                Credentials = new MqttClientCredentials()
            };
            var client = new MqttClientAgent( ( s ) => new LowLevelMqttClient( ProtocolConfiguration.Mqtt3, config, s, channel ) );
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
