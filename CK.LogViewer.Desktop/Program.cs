using CK.LogViewer.Desktop;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

public class Program
{
    public static async Task<int> Main( string[] args )
    {
        if( args.Length == 0 )
        {
            Console.Error.WriteLine( "The path of the .ckmon file must be supplied." );
            return 1;
        }
        if( args.Length > 1 )
        {
            Console.Error.WriteLine( "Only one arg is accepted: the path of the .ckmon file." );
            return 2;
        }
        string path = args.Single();
        if( !File.Exists( path ) )
        {
            Console.Error.WriteLine( "No file exist at the given path." );
            return 3;
        }
        string assemblyPath = Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location )!;
        string configPath = Path.Combine( assemblyPath, "appsettings.json" );
        string configTxt = await File.ReadAllTextAsync( configPath );
        Config? config = JsonSerializer.Deserialize<Config>( configTxt );
        if( config == null )
        {
            Console.Error.WriteLine( "Could not parse the config file appsettings.json." );
            return 4;
        }

        Uri serverAdress = new( config.ServerAddress );
        string api = new Uri( serverAdress, "api/LogViewer" ).ToString();

        HttpClient httpClient = new();
        string hash;
        using( Stream stream = File.OpenRead( path ) )
        {

            HttpResponseMessage resp = await httpClient.PostAsync( api, new MultipartFormDataContent()
            {
                { new StreamContent( stream ), "files", Path.GetFileName(path) }
            } );
            hash = await resp.Content.ReadAsStringAsync();
        }

        Process.Start( new ProcessStartInfo( serverAdress + "#" + hash )
        {
            UseShellExecute = true
        } );
        return 0;
    }
}
