using Nuke.Common.Tooling;
using Nuke.WebDeploy;
using System;

namespace CodeCakeBuilder.NetFrameWorkRunner
{
    public class Program
    {
        public static void Main()
        {
            string deployToken = Environment.GetEnvironmentVariable( "DEPLOY_PASSWORD" );
            string siteName = "cklogviewerwebapp";
            WebDeployTasks.WebDeploy( s =>
                 s.SetSourcePath( "CodeCakeBuilder/Releases/CK.LogViewer.WebApp.Server" )
                 .SetSiteName( siteName )
                 .SetPublishUrl( "https://" + siteName + ".scm.azurewebsites.net:443/msdeploy.axd?site=" + siteName )
                 .SetUsername( "$" + siteName )
                 .SetPassword( deployToken )
            );
        }
    }
}
