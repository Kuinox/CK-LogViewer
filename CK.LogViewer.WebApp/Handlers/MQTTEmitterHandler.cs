using CK.Core;
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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

        public async Task Handle(IActivityMonitor m, IncomingLogWithPosition notification, CancellationToken cancellationToken )
        {
            Handler? handler;
            bool createdNewHandler = false;
            lock( _handlers )
            {
                if( !_handlers.TryGetValue( notification.InstanceGuid, out handler ) )
                {
                    createdNewHandler = true;
                    handler = new Handler( notification.InstanceGuid, _mqttConfig.Value.LogBufferSize, _mqttClient );
                    _handlers[notification.InstanceGuid] = handler;
                }
            }
            if( createdNewHandler )
            {
                const string embeddedDir = "CK.LogViewer.Embedded";
                var assembly = Assembly.GetExecutingAssembly();
                var curr = assembly.Location;
                var versionInfo =FileVersionInfo.GetVersionInfo( assembly.Location );
                var climbing = new Stack<string>();
                while( true )
                {
                    curr = Path.GetDirectoryName( curr );
                    if( curr == null )
                    {
                        m.Error( "Could not find the CK.LogViewer.Embedded folder. Is your installation fine ?" );
                    }
                    if( Directory.GetDirectories( curr! ).Any( s => Path.GetFileName( s ) == embeddedDir ) )
                    {
                        break;
                    }
                    climbing.Push( Path.GetFileName( curr ) );
                }

                curr = Path.Combine( curr, embeddedDir );
                if( climbing.Count > 0 )
                {
                    climbing.Pop();
                    curr = Path.Combine( curr, Path.Combine( climbing.ToArray() ) );
                }
                curr = Path.Combine( curr, embeddedDir + ".dll" );
                curr = Path.GetFullPath( curr );
                Process.Start( new ProcessStartInfo()
                {
                    FileName = "dotnet",
                    ArgumentList = { curr, $"http://localhost:8748/?v={versionInfo.ProductVersion}#{notification.InstanceGuid}" },
                    UseShellExecute = false,
                    CreateNoWindow = true
                } );
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
