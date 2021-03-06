using Cake.Common;
using Cake.Common.IO;
using Cake.Common.Tools.DotNetCore;
using Cake.Common.Tools.DotNetCore.Publish;
using Cake.Common.Tools.InnoSetup;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Common.Tools.GitReleaseManager;
using System;
using Cake.Common.Tools.GitReleaseManager.Create;
using System.IO;
using System.Linq;
using System.IO.Compression;
using SimpleGitVersion;
using System.Collections.Generic;
using Octokit;
using System.Net;
using System.Net.Http;

namespace CodeCake
{
    public partial class Build : CodeCakeHost
    {
        public Build()
        {
            Cake.Log.Verbosity = Verbosity.Diagnostic;

            StandardGlobalInfo globalInfo = CreateStandardGlobalInfo()
                                                .AddDotnet()
                                                .AddNPM()
                                                .SetCIBuildTag();
            Task( "Default" ).Does( () =>
            {
                globalInfo.TerminateIfShouldStop();
                if(!globalInfo.IsValid)
                {
                    Console.WriteLine("Invalid version. Exiting.");
                    return;
                }
                globalInfo.GetDotnetSolution().Clean();
                globalInfo.GetNPMSolution().Clean();
                Cake.CleanDirectories( globalInfo.ReleasesFolder );

                globalInfo.GetDotnetSolution().Build();
                globalInfo.GetNPMSolution().Build();
                string toPublish = "CK.Monitoring.MQTT";
                globalInfo.GetDotnetSolution().Pack();
                var artifacts = globalInfo.GetArtifactPushList();

                string webappServer = globalInfo.ReleasesFolder.AppendPart( "CK.LogViewer.WebApp.Server" ).ToString();
                string webappDesktop = globalInfo.ReleasesFolder.AppendPart( "CK.LogViewer.WebApp.Desktop" ).ToString();
                string desktopApp = globalInfo.ReleasesFolder.AppendPart( "CK.LogViewer.Desktop" ).ToString();
                Cake.DotNetCorePublish( "CK.LogViewer.WebApp", new DotNetCorePublishSettings()
                .AddVersionArguments( globalInfo.BuildInfo, ( cfg ) =>
                {
                    cfg.OutputDirectory = webappServer;
                } ) );
                Cake.DotNetCorePublish( "CK.LogViewer.WebApp", new DotNetCorePublishSettings()
                .AddVersionArguments( globalInfo.BuildInfo, ( cfg ) =>
                {
                    cfg.OutputDirectory = webappDesktop;
                } ) );
                Cake.DotNetCorePublish( "CK.LogViewer.Desktop", new DotNetCorePublishSettings()
                 .AddVersionArguments( globalInfo.BuildInfo, ( cfg ) =>
                {
                    cfg.OutputDirectory = desktopApp;
                } ) );
                File.Delete( webappServer + "/appsettings.Desktop.json" );
                File.Delete( webappDesktop + "/appsettings.Server.json" );

                Cake.InnoSetup( "CodeCakeBuilder/InnoSetup/innosetup.iss", new InnoSetupSettings()
                {
                    OutputDirectory = globalInfo.ReleasesFolder.ToString(),
                    Defines = new Dictionary<string, string>
                    {
                        { "MyAppVersion", "v"+ globalInfo.BuildInfo.Version.ToString() }
                    }
                } );


                string installer = Path.GetFullPath( Directory.GetFiles( globalInfo.ReleasesFolder ).Single( s => s.EndsWith( ".exe" ) ) );
                string token = Environment.GetEnvironmentVariable( "GitHubPAT" );
                if( token == null )
                {
                    Console.WriteLine( $"Skipping release creation because token 'GitHubPAT' is missing." );
                }
                else
                {
                    GitHubClient client = new GitHubClient( new ProductHeaderValue( "CodeCakeBuilder" ) );
                    var tokenAuth = new Credentials( token );
                    client.Credentials = tokenAuth;
                    string version = "v" + globalInfo.BuildInfo.Version.ToString();
                    Release release = client.Repository.Release.Create( "Kuinox", "CK-LogViewer", new NewRelease( version )
                    {
                        Body = ":tada:",
                        Name = version,
                        TargetCommitish = globalInfo.BuildInfo.CommitSha,
                        Prerelease = globalInfo.BuildInfo.Version.IsPrerelease,
                        Draft = false
                    } ).GetAwaiter().GetResult();
                    using( FileStream installerStream = File.OpenRead( installer ) )
                    {
                        client.Repository.Release.UploadAsset(
                            release, new ReleaseAssetUpload( Path.GetFileName( installer ), "application/octet-stream", installerStream, default )
                        ).GetAwaiter().GetResult();
                    }
                }

                string deployToken = Environment.GetEnvironmentVariable( "DEPLOY_PASSWORD" );
                NetworkCredential creds = new NetworkCredential( "CKLogViewerWebApp\\$CKLogViewerWebApp", deployToken );
                foreach( string file in Directory.GetFiles( webappServer, "*", SearchOption.AllDirectories ) )
                {
                    string uploadPath = Path.GetRelativePath( webappServer, file );
                    DeleteFile( creds, uploadPath );
                    UploadFile( creds, file, uploadPath );
                }
            } );
        }

        static void DeleteFile( NetworkCredential creds, string uploadPath )
        {
            Console.WriteLine( $"Deleting {uploadPath}." );
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create( "ftp://waws-prod-par-013.ftp.azurewebsites.windows.net/site/wwwroot/" + uploadPath.Replace( '\\', '/' ) );
                request.Credentials = creds;
                request.EnableSsl = true;
                request.Method = WebRequestMethods.Ftp.DeleteFile;
                using( FtpWebResponse response = (FtpWebResponse)request.GetResponse() )
                {
                    Console.WriteLine( $"Delete status: {response.StatusDescription}" );
                }
            }
            catch( Exception ex )
            {
                Console.WriteLine( ex.Message );
            }

        }

        static void UploadFile( NetworkCredential creds, string filePath, string uploadPath )
        {
            Console.WriteLine( $"Uploading {filePath} to {uploadPath}." );
            FtpWebRequest request = CreateWebRequest(
                "ftp://waws-prod-par-013.ftp.azurewebsites.windows.net/site/wwwroot/" + uploadPath.Replace( '\\', '/' ),
                creds
            );

            request.Method = WebRequestMethods.Ftp.UploadFile;
            using( FileStream fileStream = File.OpenRead( filePath ) )
            using( Stream requestStream = request.GetRequestStream() )
            {
                fileStream.CopyTo( requestStream );
            }

            using( FtpWebResponse response = (FtpWebResponse)request.GetResponse() )
            {
                Console.WriteLine( $"Upload File Complete, status {response.StatusDescription}" );
            }
        }

        static FtpWebRequest CreateWebRequest( string uri, NetworkCredential creds )
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create( uri );
            request.Credentials = creds;
            request.EnableSsl = true;
            request.ConnectionGroupName = "FTPCCB";
            request.KeepAlive = true;
            return request;
        }
    }
}
