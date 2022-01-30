using CK.LogViewer.Enumerable;
using CK.LogViewer.WebApp.Configuration;
using CK.LogViewer.WebApp.Model;
using CK.LogViewer.WebApp.Services;
using CK.Monitoring;
using CK.MQTT;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace CK.LogViewer.WebApp.Handlers
{
    public class MQTTEmitterHandler
    {
        readonly IMqtt3Client _mqttClient;
        readonly IOptions<MQTTConfiguration> _mqttConfig;
        readonly Dictionary<Guid, Handler> _handlers = new();
        //TODO: autocleanup, this is a slow memory leak.
        //We need to get rid of the handlers if they are not active anymore.
        public MQTTEmitterHandler( IMqtt3Client mqttClient, IOptions<MQTTConfiguration> mqttConfig )
        {
            _mqttClient = mqttClient;
            _mqttConfig = mqttConfig;
        }

        public async Task Handle( IncomingLogWithPosition notification, CancellationToken cancellationToken )
        {
            Handler? handler;
            lock(_handlers)
            {
                if(!_handlers.TryGetValue( notification.InstanceGuid, out handler ) )
                {
                    handler = new Handler( notification.InstanceGuid, _mqttConfig.Value.LogBufferSize, _mqttClient );
                    _handlers[notification.InstanceGuid] = handler;
                }
            }
            await handler.HandleAsync( notification, cancellationToken );
        }

        class Handler
        {
            readonly Guid _guid;
            readonly IMqtt3Client _mqttClient;
            readonly Channel<IMulticastLogEntryWithOffset> _entries;

            public Handler( Guid guid, int capacity, IMqtt3Client mqttClient )
            {
                _guid = guid;
                _mqttClient = mqttClient;
                _entries = Channel.CreateBounded<IMulticastLogEntryWithOffset>( capacity );
                _backgroundTask = ExecuteAsync( _cts.Token );
            }

            readonly CancellationTokenSource _cts = new();
            readonly Task _backgroundTask;

            public Task Stop()
            {
                _cts.Cancel();
                return _backgroundTask;
            }

            public ValueTask HandleAsync( IncomingLogWithPosition logEntry, CancellationToken cancellationToken )
                => _entries.Writer.WriteAsync( logEntry.LogEntry, cancellationToken );

            protected async Task ExecuteAsync( CancellationToken stoppingToken )
            {
                await foreach( EnumerableLogStatsExtensions.LogEntryWithState entry in
                    _entries.Reader.ReadAllAsync( stoppingToken ).ToObservable().AddState().ToAsyncEnumerable() )
                {
                    using( MemoryStream ms = new() )
                    using( Utf8JsonWriter writer = new( ms ) )
                    {
                        LogToJson.WriteLog( entry, writer );
                        await writer.FlushAsync( stoppingToken );
                        await await _mqttClient.PublishAsync( null, "logLive/" + _guid.ToString(), QualityOfService.ExactlyOnce, false, ms.ToArray() );
                    }
                }
            }
        }
    }
}