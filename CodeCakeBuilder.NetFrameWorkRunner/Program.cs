using Microsoft.Web.Deployment;
using System;
using System.IO;

namespace CodeCakeBuilder.NetFrameWorkRunner
{
    public class Program
    {
        public static void Main()
        {
            string deployToken = Environment.GetEnvironmentVariable( "DEPLOY_PASSWORD" );
            string siteName = "cklogviewerwebapp";
            string sourcepath = Path.GetFullPath( "../CodeCakeBuilder/Releases/CK.LogViewer.WebApp.Server" );
            var sourceOptions = new DeploymentBaseOptions();
            var destinationOptions = new DeploymentBaseOptions
            {
                AuthenticationType = "basic",
                ComputerName = "https://" + siteName + ".scm.azurewebsites.net:443/msdeploy.axd?site=" + siteName,
                UserName = "$" + siteName,
                Password = deployToken
            };
            destinationOptions.Trace += ( _, trace ) => Console.WriteLine( trace.EventLevel + " " + trace.Message );

            var syncOptions = new DeploymentSyncOptions();
            var sourceProvider = DeploymentWellKnownProvider.IisApp;
            var destinationProvider = DeploymentWellKnownProvider.IisApp;
            using( var deploymentObject = DeploymentManager.CreateObject( sourceProvider, sourcepath, sourceOptions ) )
            {
                deploymentObject.SyncTo( destinationProvider, siteName, destinationOptions, syncOptions );
            }
        }
    }
}
