using CK.Core;
using CK.MQTT;
using CK.MQTT.Client;
using CK.MQTT.Packets;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Channels;

namespace CK.Monitoring.MQTT
{
    public class MQTT : AsyncBackgroundHandler
    {
        MQTTConfiguration _config;
        IMqtt3Client? _client;
        readonly Guid _instanceGuid = Guid.NewGuid();

        public MQTT( MQTTConfiguration config )
        {
            _config = config ?? throw new ArgumentNullException( "config" );
        }

        bool _launchLogViewer = true;

        protected override async Task DoHandleAsync( IActivityMonitor m, IMulticastLogEntry entry )
        {
            Task task;
            using( MemoryStream mem = new() )
            using( CKBinaryWriter bw = new( mem ) )
            {
                entry.WriteLogEntry( bw );
                task = await _client!.PublishAsync( new SmallOutgoingApplicationMessage( $"ck-log/{_instanceGuid}", QualityOfService.ExactlyOnce, false, mem.ToArray() ) );
            }
            if( _launchLogViewer )
            {
                _launchLogViewer = false;
                bool result = await task.WaitAsync( 5000 );
                if( !result )
                {
                    m.Error( "Waited 5 secs for an ack of the server but didn't had any response." );
                    return;
                }
                var assembly = Assembly.GetAssembly( typeof( LogViewer.Embedded.Program ) )!;
                var curr = Path.GetFullPath( assembly.Location );
                var versionInfo = FileVersionInfo.GetVersionInfo( assembly.Location );
                Process.Start( new ProcessStartInfo()
                {
                    FileName = "dotnet",
                    ArgumentList = { curr, $"http://localhost:8748/?v={versionInfo.ProductVersion}#{_instanceGuid}" },
                    UseShellExecute = false,
                    CreateNoWindow = true
                } );
            }
        }

        protected override bool DoActivate( IActivityMonitor m )
        {
            var splitted = _config.ConnectionString.Split( ':' );
            var channel = new TcpChannel( splitted[0], int.Parse( splitted[1] ) );
            var config = new Mqtt3ClientConfiguration()
            {
                KeepAliveSeconds = 0,
                DisconnectBehavior = DisconnectBehavior.AutoReconnect,
                Credentials = new MqttClientCredentials( Guid.NewGuid().ToString(), true )
            };
            _client = new MqttClientAgent( ( s ) => new LowLevelMqttClient( ProtocolConfiguration.Mqtt3, config, s, channel ) );
            _client.ConnectAsync( null ).GetAwaiter().GetResult();
            _launchLogViewer = _config.LaunchLogViewer;
            return true;
        }

        public override bool ApplyConfiguration( IActivityMonitor m, IHandlerConfiguration c )
        {
            if( c is not MQTTConfiguration cfg ) return false;
            _config = cfg; //TODO: reconnect to new broker
            return true;
        }

        protected override void DoDeactivate( IActivityMonitor m )
        {
            _client!.DisconnectAsync( true ).GetAwaiter().GetResult();
        }

        public override void OnTimer( IActivityMonitor m, TimeSpan timerSpan )
        {
        }
    }
}
