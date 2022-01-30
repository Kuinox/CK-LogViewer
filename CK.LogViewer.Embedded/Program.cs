using PhotinoNET;
using System;
using System.Drawing;

namespace CK.LogViewer.Embedded;

public class Program
{
    [STAThread]
    static void Main( string[] args )
    {
        string path = args.Length > 0 ? args[0] : @"C:\Users\Kuinox\Desktop\log.ckmon";
        // Creating a new PhotinoWindow instance with the fluent API
        var window = new PhotinoWindow()
            .SetTitle( "CK-LogViewer" )
            // Resize to a percentage of the main monitor work area
            .SetUseOsDefaultSize( false )
            .SetSize( new Size( 900, 800 ) )
            .Center()
            .SetResizable( true )
            .Load( new Uri( path ) ); // Can be used with relative path strings or "new URI()" instance to load a website.

        window.WaitForClose(); // Starts the application event loop
    }
}
