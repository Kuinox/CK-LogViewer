using Cake.Common.IO;
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

            globalInfo.TerminateIfShouldStop();

            globalInfo.GetDotnetSolution().Clean();
            globalInfo.GetNPMSolution().Clean();
            Cake.CleanDirectories( globalInfo.ReleasesFolder );

            globalInfo.GetDotnetSolution().Build();
            globalInfo.GetNPMSolution().Build();
        }

    }
}
