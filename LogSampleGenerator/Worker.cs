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
                CKTrait tag = ActivityMonitor.Tags.StackTrace;
                CKTraitContext ctc = ActivityMonitor.Tags.Context;
                CKTrait trait1 = ctc.FindOrCreate( "foo" );
                CKTrait trait2 = ctc.FindOrCreate( "bar" );
                m1.Trace( trait1, "Hello" );
                m1.Debug( trait2, "Hello" );
                m1.Info( "Hello" );
                m1.Warn( trait1.Union( trait2 ), "Hello\nmultiline\ntest" );
                m1.Error( trait1.Except( trait2 ), "Hello" );
                m1.Fatal( "Hello" );
                m1.OpenInfo( "test" );
                await Task.Delay( 1000, stoppingToken );
            }
        }
    }
}
