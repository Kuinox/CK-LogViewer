using CK.Core;
using CK.Monitoring;
using CK.Monitoring.Handlers;
using System;

namespace LogSampleGenerator
{
    class Program
    {
        static void Main( string[] args )
        {
            GrandOutputConfiguration cfg = new();
            var binCfg = new BinaryFileConfiguration
            {
                Path = "LogOutput"
            };
            cfg.Handlers.Add( binCfg );
            GrandOutput.EnsureActiveDefault( cfg );

            ActivityMonitor m = new();
            CKTraitContext ctx = ActivityMonitor.Tags.Context;
            CKTrait tagA = ctx.FindOrCreate( "tagA" );
            CKTrait tagB = ctx.FindOrCreate( "tagB" );
            CKTrait tagC = ctx.FindOrCreate( "tagC" );

            m.Info( "Log with no tag" );
            m.Info( "Log with tag A", tagA );
            m.Info( "Log with tag B", tagB );
            m.Info( "Log with tag C", tagC );
            m.Info( "Log with tag A & B", tagA.Union( tagB ) );
            m.Info( "Log with no tag B & C", tagB.Union( tagC ) );
            m.Info( "Log with no tag A & B & C", tagA.Union( tagB.Union( tagC ) ) );
        }
    }
}
