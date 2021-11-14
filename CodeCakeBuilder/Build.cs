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
using Cake.WebDeploy;
using Octokit;

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

                globalInfo.GetDotnetSolution().Clean();
                globalInfo.GetNPMSolution().Clean();
                Cake.CleanDirectories( globalInfo.ReleasesFolder );

                globalInfo.GetDotnetSolution().Build();
                globalInfo.GetNPMSolution().Build();
                string webappFolder = globalInfo.ReleasesFolder.AppendPart( "CK.LogViewer.WebApp" ).ToString();
                Cake.DotNetCorePublish( "CK.LogViewer.WebApp", new DotNetCorePublishSettings()
                .AddVersionArguments( globalInfo.BuildInfo, ( cfg ) =>
                {
                    cfg.OutputDirectory = webappFolder;
                } ) );
                Cake.DotNetCorePublish( "CK.LogViewer.Desktop", new DotNetCorePublishSettings()
                 .AddVersionArguments( globalInfo.BuildInfo, ( cfg ) =>
                {
                    cfg.OutputDirectory = globalInfo.ReleasesFolder.AppendPart( "CK.LogViewer.Desktop" ).ToString();
                } ) );

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
                    client.Repository.Release.Create( "Kuinox", "CK-LogViewer", new NewRelease(version)
                    {
                        Body = ":tada:",
                        Name = version,
                        TargetCommitish = globalInfo.BuildInfo.CommitSha,
                        Prerelease = globalInfo.BuildInfo.Version.IsPrerelease,
                        Draft = false
                    } );
                }
                string siteName = "cklogviewerwebapp";
                string deployToken = Environment.GetEnvironmentVariable( "DEPLOY_PASSWORD" );

                if( deployToken == null )
                {
                    Console.WriteLine( $"Skipping website deploy because token 'DEPLOY_PASSWORD' is missing." );
                }
                else
                {
                    Cake.DeployWebsite( new DeploySettings()
                    {
                        SourcePath = webappFolder,
                        SiteName = siteName,
                        ComputerName = "https://" + siteName + ".scm.azurewebsites.net:443/msdeploy.axd?site=" + siteName,
                        Username = "$" + siteName,
                        Password = deployToken
                    } );
                }
            } );
        }
    }
}
