using CK.Core;
using CK.MQTT;
using System.Threading.Channels;

namespace CK.Monitoring.MQTT
{
    public class MQTT : AsyncBackgroundHandler
    {
        MQTTConfiguration _config;
        IMqttClient _client;
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
                await _client.PublishAsync( m, $"log/{_instanceGuid}", QualityOfService.ExactlyOnce, false, mem.GetBuffer() );
            }
        }

        protected override bool DoActivate( IActivityMonitor m )
        {
            _client.ConnectAsync( m ).GetAwaiter().GetResult();
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
