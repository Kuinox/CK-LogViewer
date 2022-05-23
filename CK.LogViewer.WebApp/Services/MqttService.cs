using CK.Core;
using CK.LogViewer.WebApp.Configuration;
using CK.LogViewer.WebApp.Handlers;
using CK.LogViewer.WebApp.Model;
using CK.Monitoring;
using CK.MQTT;
using CK.MQTT.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace CK.LogViewer.WebApp.Services
{
    public class MqttService : BackgroundService
    {
        readonly MqttClientAgent _mqtt3Client;
        readonly AppendToFileHandler _appendToFileHandler;
        readonly Channel<ApplicationMessage> _messageChannel;
        public MqttService( IOptions<MQTTConfiguration> config, MqttClientAgent mqtt3Client, AppendToFileHandler appendToFileHandler )
        {
            _mqtt3Client = mqtt3Client;
            _appendToFileHandler = appendToFileHandler;
            _messageChannel = Channel.CreateBounded<ApplicationMessage>( config.Value.LogBufferSize );
        }

        public override async Task StartAsync( CancellationToken cancellationToken )
        {
            _mqtt3Client.OnMessage.Async += async (m, msg) => await _messageChannel.Writer.WriteAsync( msg );
            await _mqtt3Client.ConnectAsync( null, cancellationToken: cancellationToken );
            await _mqtt3Client.SubscribeAsync( new Subscription( "ck-log/#", QualityOfService.ExactlyOnce ) );
            await base.StartAsync( cancellationToken );
        }

        protected override async Task ExecuteAsync( CancellationToken stoppingToken )
        {
            ActivityMonitor m = new();
            await foreach( ApplicationMessage item in _messageChannel.Reader.ReadAllAsync( stoppingToken ) )
            {
                try
                {
                    m.Trace( "Processing message..." );
                    ILogEntry? entry;
                    bool badEndOfFile;
                    using( MemoryStream ms = new( item.Payload.ToArray() ) )
                    using( CKBinaryReader reader = new( ms ) )
                    {
                        entry = LogEntry.Read( reader, LogReader.CurrentStreamVersion, out badEndOfFile );
                    }
                    if( badEndOfFile )
                    {
                        m.Error( $"Error while parsing a message. topic:{item.Topic}, payload:{item}" );
                        continue;
                    }
                    if( entry is null )
                    {
                        m.Error( "Error while parsing log entry." );
                        continue;
                    }

                    if( entry is not IMulticastLogEntry multicastLogEntry )
                    {
                        m.Error( "Only multicast log entry are accepted." );
                        continue;
                    }
                    await _appendToFileHandler.Handle( m, new()
                    {
                        Topic = item.Topic,
                        InstanceGuid = Guid.Parse( item.Topic.Split( '/' )[1] ),
                        LogEntry = multicastLogEntry
                    }, default );
                }
                catch( Exception ex )
                {
                    m.Error( ex );
                }

            }
        }

    }
}
