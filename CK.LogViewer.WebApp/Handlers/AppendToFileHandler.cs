using CK.Core;
using CK.LogViewer.WebApp.Configuration;
using CK.LogViewer.WebApp.Model;
using CK.Monitoring;
using CK.MQTT;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CK.LogViewer.WebApp.Handlers
{
    public class AppendToFileHandler
    {
        readonly IOptions<LogPersistanceConfig> _logPersistanceConfig;
        readonly MQTTEmitterHandler _mqttEmitterHandler;

        public AppendToFileHandler(
            IOptions<LogPersistanceConfig> logPersistanceConfig,
            MQTTEmitterHandler mqttEmitterHandler
        )
        {
            _logPersistanceConfig = logPersistanceConfig;
            _mqttEmitterHandler = mqttEmitterHandler;
        }
        static readonly ReadOnlyMemory<byte> _fileHeader;
        static AppendToFileHandler()
        {
            List<byte> tmp = new();
            tmp.AddRange( LogReader.FileHeader );
            tmp.AddRange( BitConverter.GetBytes( LogReader.CurrentStreamVersion ) );
            _fileHeader = tmp.ToArray();
        }
        public async Task Handle( IActivityMonitor m, IncomingLog notification, CancellationToken cancellationToken )
        {
            string dir = Path.Combine( _logPersistanceConfig.Value.StreamLogFolder, notification.InstanceGuid.ToString() );
            string appFolder = Environment.GetFolderPath( Environment.SpecialFolder.CommonApplicationData, Environment.SpecialFolderOption.Create );
            dir = Path.Combine( appFolder, dir );
            Directory.CreateDirectory( dir );
            string logFile = Path.Combine( dir, "log.ckmon" );
            using( FileStream fs = File.Open( logFile, FileMode.Append ) )
            {
                long position = fs.Position;
                if( position == 0 )
                {
                    await fs.WriteAsync( _fileHeader, cancellationToken );
                }
                using( CKBinaryWriter bw = new( fs ) )
                {
                    notification.LogEntry.WriteLogEntry( bw );
                }

                IncomingLogWithPosition log = new()
                {
                    InstanceGuid = notification.InstanceGuid,
                    LogEntry = new MulticastLogEntryWithOffsetImpl( new MulticastLogEntryWithOffset( notification.LogEntry, position ) ),
                    Topic = notification.Topic
                };
                await _mqttEmitterHandler.Handle( m, log, cancellationToken );
            }
        }
    }
}
