using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

public class Program
{
    public static async Task Main( string[] args )
    {
        CancellationTokenSource cts = new( 500 );
        await CreateHostBuilder( args ).Build().RunAsync(cts.Token);
    }

    public static IHostBuilder CreateHostBuilder( string[] args ) =>
        Host.CreateDefaultBuilder( args )
            .UseCKMonitoring()
            .ConfigureServices( (hostContext, services) =>
            {
                services.AddHostedService<Worker>();
            } );
}
