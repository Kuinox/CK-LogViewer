// See https://aka.ms/new-console-template for more information
using CSemVer;
using Octokit;
using System.Collections.Immutable;
using System.Diagnostics;

GitHubClient client = new( new ProductHeaderValue( "CK-LogViewer-AutoUpdater" ) );
IReadOnlyList<Release> repo = await client.Repository.Release.GetAll( "Kuinox", "CK-LogViewer" );
Release? release = repo
    .Select( s => (Release: s, Version: CSVersion.Parse( s.TagName )) )
    .Where( s => s.Version.IsStable )
    .MaxBy(s=>s.Version).Release;
if( release == null ) return;//No new version.

ReleaseAsset installer = release.Assets.Single( s => s.Name.EndsWith( ".exe" ) );
HttpClient httpClient = new();
HttpResponseMessage response = await httpClient.GetAsync( installer.BrowserDownloadUrl );
string installerPath = "installer.exe";
using( FileStream saveInstaller = File.OpenWrite(installerPath))
using( Stream downloadStream = await response.Content.ReadAsStreamAsync() )
{
    downloadStream.CopyTo( saveInstaller );
}

Process.Start( new ProcessStartInfo()
{
    FileName = installerPath,
    Arguments = "/VERYSILENT",
    CreateNoWindow = true
} );
// We must exit immediatly after.

