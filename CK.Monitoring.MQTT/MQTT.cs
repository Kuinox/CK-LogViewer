using CK.Core;
using CK.MQTT;
using CK.MQTT.Client;
using CK.MQTT.Packets;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Channels;

namespace CK.Monitoring.MQTT
{
    public class MQTT : IGrandOutputHandler
    {
        MQTTConfiguration _config;
        IMqtt3Client? _client;
        readonly Guid _instanceGuid = Guid.NewGuid();

        public MQTT( MQTTConfiguration config )
        {
            _config = config ?? throw new ArgumentNullException( "config" );
        }

        bool _launchLogViewer = true;

        public async ValueTask HandleAsync( IActivityMonitor m, IMulticastLogEntry entry )
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

        public async ValueTask<bool> ActivateAsync( IActivityMonitor m )
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
            await _client.ConnectAsync( null );
            _launchLogViewer = _config.LaunchLogViewer;
            return true;
        }

        public ValueTask<bool> ApplyConfigurationAsync( IActivityMonitor m, IHandlerConfiguration c )
        {
            if( c is not MQTTConfiguration cfg ) return new ValueTask<bool>( false );
            _config = cfg; //TODO: reconnect to new broker
            return new( true );
        }

        public async ValueTask DeactivateAsync( IActivityMonitor m )
        {
            await _client!.DisconnectAsync( true );
        }

        public ValueTask OnTimerAsync( IActivityMonitor m, TimeSpan timerSpan ) => new();
    }
}
