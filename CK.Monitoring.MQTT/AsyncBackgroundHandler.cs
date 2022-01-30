using CK.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace CK.Monitoring.MQTT
{
    public abstract class AsyncBackgroundHandler : IGrandOutputHandler
    {
        Channel<IMulticastLogEntry>? _channel;
        Task? _backgroundTask;
        async Task BackgroundLoop( Channel<IMulticastLogEntry> channel )
        {
            ActivityMonitor m = new();
            m.AutoTags += ActivityMonitor.Tags.StackTrace;
            await foreach( IMulticastLogEntry entry in channel.Reader.ReadAllAsync() )
            {
                await DoHandleAsync( m, entry );
            }
        }

        protected abstract Task DoHandleAsync( IActivityMonitor m, IMulticastLogEntry entry );

        public bool Activate( IActivityMonitor m )
        {
            _channel = Channel.CreateUnbounded<IMulticastLogEntry>();
            _backgroundTask = BackgroundLoop( _channel );
            return DoActivate( m );
        }

        protected abstract bool DoActivate( IActivityMonitor m );

        public abstract bool ApplyConfiguration( IActivityMonitor m, IHandlerConfiguration c );

        public void Deactivate( IActivityMonitor m )
        {
            _channel!.Writer.Complete();
            _backgroundTask!.GetAwaiter().GetResult(); //TODO: need asynd for Deactivate.
            DoDeactivate( m );
        }

        protected abstract void DoDeactivate( IActivityMonitor m );

        public void Handle( IActivityMonitor m, GrandOutputEventInfo logEvent )
            => _channel!.Writer.TryWrite( logEvent.Entry );

        public abstract void OnTimer( IActivityMonitor m, TimeSpan timerSpan );
    }
}
