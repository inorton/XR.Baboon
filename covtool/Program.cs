using System;
using System.IO;
using System.Linq;
using Mono.Unix;
using XR.Mono.Cover;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Threading;

namespace Covtool
{
	class MainClass
	{
        public static int Usage()
        {
            Console.WriteLine("Usage: covem PROGRAM ARGUMENTS");
            Console.WriteLine(@"
PROGRAM should be the path to your c# exe (not a shell script)

Pass covem the same options you would pass to your program.

By default coverage will not be recorded. To choose the 
namespaces covered, create a text file containing a list of types 
to cover (one regex per line) and save it in the same folder as the 
c# exe file with the same name but with '.covcfg' appended. Eg:

testsubject.exe
testsubject.exe.covcfg

If it is not convenient to place the config file in the same folder or
with the same name, you can pass baboon the BABOON_CFG environment
variable with a full path to your config file. Eg:

BABOON_CFG=/home/inb/tests.cfg /usr/local/bin/covem ./server.exe --run

Configuration files should usually be written to match the beginning 
of a type name like so:

^System.Data.Sqlite
^MyProject.Program
^XR.HttpFileStream


");
            Console.WriteLine();
            Console.WriteLine("results are saved in PROGGRAM.covdb");
            Console.WriteLine();
            return 1;
        }

        static void PumpStdin( object unused ) {
            using ( Stream input = Console.OpenStandardInput() ) {
                int rc = 0;
                var buf = new byte[512];
                do {

                    try {
                        rc = input.Read( buf, 0, buf.Length );
                        if ( rc > 0 ) {
                            debugee.StandardInput.BaseStream.Write( buf, 0, rc );
                        }
                    } catch { 
                        rc = 0;
                    }
                } while (( !debugee.HasExited ) && rc > 0 );
            }
        }

        public static void SignalHandler()
        {
            var sig = new UnixSignal( Mono.Unix.Native.Signum.SIGUSR2 );
            var sigs = new UnixSignal[] { sig };

            do {
                UnixSignal.WaitAny( sigs, new TimeSpan(0,1,0) );
                covertool.SaveData();
            } while ( debugee != null && !debugee.HasExited );
        }

        static Process debugee = null;
        static CoverHost covertool = null;

		public static int Main (string[] vargs)
		{
            if ( vargs.Length == 0 ) return Usage();
            if ( Regex.IsMatch( vargs[0], "-h$|-help$" ) ) return Usage();
            if (!System.IO.File.Exists(vargs[0])) return Usage();

            // the first thing is the mono EXE we are running, everything else are args passed to it
            // we do no argument processing at all.

            var program = vargs[0];
            var args = vargs.Skip(1);
            var patterns = new List<string> ();

            // we look in BABOON_CFG for a config file
            var env = Environment.GetEnvironmentVariable("BABOON_CFG");
            // else we look for a file in the same folder as the EXE named EXE.covcfg
            var cfgfile = program + ".covcfg";

            if ( !string.IsNullOrEmpty( env ) ){
                cfgfile = env;
            }

            bool hitCount = true;
            if ( File.Exists( cfgfile ) ) {
                using ( var f = File.OpenText( cfgfile ) ) {
                    string l = null;
                    do {
                        l = f.ReadLine();
                        if ( string.IsNullOrWhiteSpace( l ) ) {
                            continue;
                        }
                        if (l.StartsWith ( "!HitCount=" ) ) {
                            l = l.Substring("!HitCount=".Length);
                            bool.TryParse (l, out hitCount);
                            continue;
                        }
                        patterns.Add (l);
                    } while ( l != null );
                }
            }

            CoverHost.RenameBackupFile( cfgfile + ".covdb" );
            CoverHost.RenameBackupFile( cfgfile + ".covreport" );

            covertool = CoverHostFactory.CreateHost ( cfgfile  + ".covdb", program, args.ToArray() );
            covertool.DataStore.SaveMeta( "config", cfgfile );
            debugee = covertool.VirtualMachine.Process;
            ThreadPool.QueueUserWorkItem( (x) => SignalHandler(), null );
            ThreadPool.QueueUserWorkItem( (x) => PumpStdin(x), null );
            covertool.HitCount = hitCount;
            covertool.Cover (patterns.ToArray ());
            covertool.Report ( cfgfile + ".covreport" );

            return covertool.VirtualMachine.Process.ExitCode;
		}
	}
}
