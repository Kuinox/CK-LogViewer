using Cake.Common;
using Cake.Common.IO;
using Cake.Common.Tools.DotNetCore;
using Cake.Common.Tools.DotNetCore.Publish;
using Cake.Common.Tools.InnoSetup;
using Cake.Core;
using Cake.Core.Diagnostics;

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

                   Cake.DotNetCorePublish( "CK.LogViewer.WebApp", new DotNetCorePublishSettings()
                   {
                       OutputDirectory = globalInfo.ReleasesFolder.AppendPart( "CK.LogViewer.WebApp" ).ToString()
                   } );
                   Cake.DotNetCorePublish( "CK.LogViewer.Desktop", new DotNetCorePublishSettings()
                   {
                       OutputDirectory = globalInfo.ReleasesFolder.AppendPart( "CK.LogViewer.Desktop" ).ToString()
                   } );

                   Cake.InnoSetup( "CodeCakeBuilder/InnoSetup/innosetup.iss", new InnoSetupSettings()
                   {
                       OutputDirectory = globalInfo.ReleasesFolder.ToString()
                   } );
               } );
        }

    }
}
