using CK.LogViewer.Desktop;
using CK.LogViewer.Enumerable;
using CK.Monitoring;
using CKLogViewer;
using CSemVer;
using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
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
        var toText = false;
        var argsSet = new HashSet<string>( args );
        var withFlag = argsSet.Where( s => s.StartsWith( "--" ) );
        if( withFlag.Any() )
        {
            if( argsSet.Contains( "--toText" ) )
            {
                toText = true;
                argsSet.Remove( "--toText" );
            }
        }

        string path = argsSet.Single();
        if( !File.Exists( path ) )
        {
            Console.Error.WriteLine( "No file exist at the given path." );
            return 3;
        }

        path = Path.GetFullPath( path );

        if( toText )
        {
            MulticastLogEntryTextBuilder builder = new( false, false );
            using( var reader = LogReader.Open( path ) )
            using( var writer = new StreamWriter( path + ".ckmon" ) )
            {
                foreach( var item in reader.ToEnumerable() )
                {
                    await writer.WriteLineAsync( builder.FormatEntryString( item ) );
                }
            }
            return 0;
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

        var assembly = Assembly.GetExecutingAssembly();
        var fileVersionInfo = FileVersionInfo.GetVersionInfo( assembly.Location );
        var currentVersionStr = fileVersionInfo.ProductVersion?.Split( "/" )[0];
        var currentVersion = currentVersionStr == null ? null : CSVersion.Parse( currentVersionStr );
        var uri = $"{serverAdress}?v={currentVersion}#{hash}";
        string? curr = assembly.Location;
        const string embeddedDir = "CK.LogViewer.Embedded";
        var climbing = new Stack<string>();
        while( true )
        {
            curr = Path.GetDirectoryName( curr );
            if( curr == null )
            {
                Console.WriteLine( "Could not find the CK.LogViewer.Embedded folder. Is your installation fine ?" );
                return 1;
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
            ArgumentList = { curr, uri },
            UseShellExecute = false
        } );
        await CheckForUpdate( currentVersion );
        return 0;
    }



    private static async Task CheckForUpdate( CSVersion? currentVersion )
    {
        if( currentVersion == null )
        {
            Interaction.MsgBox( "Could not detect running version. Please update manually CK-LogViewer", "CK-LogViewer" );
            return;
        }

        GitHubClient client = new( new ProductHeaderValue( "CK-LogViewer-AutoUpdater" ) );
        IReadOnlyList<Release> repo = await client.Repository.Release.GetAll( "Kuinox", "CK-LogViewer" );
        (Release release, CSVersion version) = repo
            .Select( s => (Release: s, Version: CSVersion.TryParse( s.TagName )) )
            .Where( s => s.Version?.IsStable ?? false )
            .Where( s => s.Release.Assets.Any( s => s.Name.EndsWith( ".exe" ) ) )
            .OrderByDescending( s => s.Version )
            .FirstOrDefault();

        if( release == null ) return; // No new version.
        if( currentVersion >= version ) return;
        MsgBoxResult result = Interaction.MsgBox( "A new version is available Do you want to install it ?", "CK-LogViewer", MsgBoxStyle.OkCancel );
        if( result == MsgBoxResult.Cancel ) return;

        ReleaseAsset installer = release.Assets.Single( s => s.Name.EndsWith( ".exe" ) );
        HttpClient httpClient = new();
        HttpResponseMessage response = await httpClient.GetAsync( installer.BrowserDownloadUrl );

        string installerPath = Path.Combine( Path.GetTempPath(), "CK-LogViewer-Installer.exe" );
        File.Delete( installerPath );
        using( FileStream saveInstaller = File.OpenWrite( installerPath ) )
        using( Stream downloadStream = await response.Content.ReadAsStreamAsync() )
        {
            downloadStream.CopyTo( saveInstaller );
        }

        Process.Start( new ProcessStartInfo()
        {
            FileName = installerPath,
            Arguments = "/VERYSILENT",
            UseShellExecute = false
        } );
        Environment.Exit( 0 );
    }



    public enum MsgBoxResult
        : int
    {
        Abort = 3,
        Cancel = 2,
        Ignore = 5,
        No = 7,
        Ok = 1,
        Retry = 4,
        Yes = 6
    }

    //[System.Flags]
    //public enum MsgBoxStyle
    //    : int
    //{
    //    AbortRetryIgnore = 2,
    //    ApplicationModal = 0,
    //    Critical = 0x10,
    //    DefaultButton1 = 0,
    //    DefaultButton2 = 0x100,
    //    DefaultButton3 = 0x200,
    //    Exclamation = 0x30,
    //    Information = 0x40,
    //    MsgBoxHelp = 0x4000,
    //    MsgBoxRight = 0x80000,
    //    MsgBoxRtlReading = 0x100000,
    //    MsgBoxSetForeground = 0x10000,
    //    OkCancel = 1,
    //    OkOnly = 0,
    //    Question = 0x20,
    //    RetryCancel = 5,
    //    SystemModal = 0x1000,
    //    YesNo = 4,
    //    YesNoCancel = 3
    //}


    [System.Flags]
    public enum MsgBoxStyle
    {
        /// <summary>
        /// OK button only (default). This member is equivalent to the Visual Basic constant <see langword="vbOKOnly" />.</summary>
        OkOnly = 0,
        /// <summary>
        /// OK and Cancel buttons. This member is equivalent to the Visual Basic constant <see langword="vbOKCancel" />.</summary>
        OkCancel = 1,
        /// <summary>
        /// Abort, Retry, and Ignore buttons. This member is equivalent to the Visual Basic constant <see langword="vbAbortRetryIgnore" />.</summary>
        AbortRetryIgnore = 2,
        /// <summary>
        /// Yes, No, and Cancel buttons. This member is equivalent to the Visual Basic constant <see langword="vbYesNoCancel" />.</summary>
        YesNoCancel = AbortRetryIgnore | OkCancel, // 0x00000003
        /// <summary>
        /// Yes and No buttons. This member is equivalent to the Visual Basic constant <see langword="vbYesNo" />.</summary>
        YesNo = 4,
        /// <summary>
        /// Retry and Cancel buttons. This member is equivalent to the Visual Basic constant <see langword="vbRetryCancel" />.</summary>
        RetryCancel = YesNo | OkCancel, // 0x00000005
        /// <summary>Critical message. This member is equivalent to the Visual Basic constant <see langword="vbCritical" />.</summary>
        Critical = 16, // 0x00000010
        /// <summary>Warning query. This member is equivalent to the Visual Basic constant <see langword="vbQuestion" />.</summary>
        Question = 32, // 0x00000020
        /// <summary>Warning message. This member is equivalent to the Visual Basic constant <see langword="vbExclamation" />.</summary>
        Exclamation = Question | Critical, // 0x00000030
        /// <summary>Information message. This member is equivalent to the Visual Basic constant <see langword="vbInformation" />.</summary>
        Information = 64, // 0x00000040
        /// <summary>First button is default. This member is equivalent to the Visual Basic constant <see langword="vbDefaultButton1" />.</summary>
        DefaultButton1 = 0,
        /// <summary>Second button is default. This member is equivalent to the Visual Basic constant <see langword="vbDefaultButton2" />.</summary>
        DefaultButton2 = 256, // 0x00000100
        /// <summary>Third button is default. This member is equivalent to the Visual Basic constant <see langword="vbDefaultButton3" />.</summary>
        DefaultButton3 = 512, // 0x00000200
        /// <summary>Application modal message box. This member is equivalent to the Visual Basic constant <see langword="vbApplicationModal" />.</summary>
        ApplicationModal = 0,
        /// <summary>System modal message box. This member is equivalent to the Visual Basic constant <see langword="vbSystemModal" />.</summary>
        SystemModal = 4096, // 0x00001000
        /// <summary>Help text. This member is equivalent to the Visual Basic constant <see langword="vbMsgBoxHelp" />.</summary>
        MsgBoxHelp = 16384, // 0x00004000
        /// <summary>Right-aligned text. This member is equivalent to the Visual Basic constant <see langword="vbMsgBoxRight" />.</summary>
        MsgBoxRight = 524288, // 0x00080000
        /// <summary>Right-to-left reading text (Hebrew and Arabic systems). This member is equivalent to the Visual Basic constant <see langword="vbMsgBoxRtlReading" />.</summary>
        MsgBoxRtlReading = 1048576, // 0x00100000
        /// <summary>Foreground message box window. This member is equivalent to the Visual Basic constant <see langword="vbMsgBoxSetForeground" />.</summary>
        MsgBoxSetForeground = 65536, // 0x00010000
    }


    // https://docs.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-messagebox
    internal class UnsafeNativeMethods
    {
        [System.Runtime.InteropServices.DllImport( "user32.dll" )]
        internal static extern MsgBoxResult MessageBox( System.IntPtr hWnd, string text, string caption, MsgBoxStyle options );
    }


    public class Interaction
    {
        public static MsgBoxResult MsgBox( string text, string caption, MsgBoxStyle options )
        {
            return UnsafeNativeMethods.MessageBox( IntPtr.Zero, text, caption, options );
        }


        public static MsgBoxResult MsgBox( string text, string? caption )
        {
            return MsgBox( text, caption ?? "", MsgBoxStyle.OkOnly );
        }


        public static MsgBoxResult MsgBox( string text ) => MsgBox( text, null );


    } // End Class Interaction
}
