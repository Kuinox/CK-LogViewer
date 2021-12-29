using CK.Core;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

public class Worker : BackgroundService
{
    protected override async Task ExecuteAsync( CancellationToken stoppingToken )
    {
        ActivityMonitor m1 = new();
        using( m1.OpenInfo( "Test" ) )
        {
            while( !stoppingToken.IsCancellationRequested )
            {
                m1.Info( "Hello" );
                await Task.Delay( 1000, stoppingToken );
            }
        }
    }
}
