using CK.Core;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

public class Worker : IHostedService
{
    public Task StartAsync( CancellationToken cancellationToken )
    {
        ActivityMonitor m1 = new();
        using( m1.OpenInfo( "Test" ) )
        {
            for( int i = 0; i < 100; i++ )
            {
                ActivityMonitor m2 = new();
                m2.Info( "Hello" );
            }
        }
        return Task.CompletedTask;
    }

    public Task StopAsync( CancellationToken cancellationToken )
    {
        return Task.CompletedTask;
    }
}
