using CK.Core;
using CK.MQTT;
using CK.MQTT.Client;
using System.Diagnostics;
using System.Threading.Channels;

namespace CK.Monitoring.MQTT
{
    public class MQTT : AsyncBackgroundHandler
    {
        MQTTConfiguration _config;
        IMqtt3Client _client;
        readonly Guid _instanceGuid = Guid.NewGuid();

        Channel<IMulticastLogEntry> _messagesToProcess;

        public MQTT( MQTTConfiguration config )
        {
            _config = config ?? throw new ArgumentNullException( "config" );
        }
        protected override async Task DoHandleAsync( IActivityMonitor m, IMulticastLogEntry entry )
        {
            using( MemoryStream mem = new() )
            using( CKBinaryWriter bw = new( mem ) )
            {
                entry.WriteLogEntry( bw );
                await _client.PublishAsync( m, $"ck-log/{_instanceGuid}", QualityOfService.ExactlyOnce, false, mem.ToArray() );
            }
        }

        protected override bool DoActivate( IActivityMonitor m )
        {
            _client = MqttClient.Factory.CreateMQTT3Client( new( _config.ConnectionString ), ( IActivityMonitor? m, DisposableApplicationMessage msg, CancellationToken token ) =>
            {
                m?.Info( $"Receveid message on topic '{msg.Topic}', length {msg.Payload}." );
                msg.Dispose();
                return new ValueTask();
            } );
            _client.ConnectAsync( null ).GetAwaiter().GetResult();
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
            _client.DisconnectAsync( m, true, true, default ).GetAwaiter().GetResult();
        }

        public override void OnTimer( IActivityMonitor m, TimeSpan timerSpan )
        {
        }
    }
}
