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
namespaces covered, create a file called PROGRAM.covconf 
containing a list of namespaces to cover (one regex per line)
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

            // we look for a file in the same folder as the EXE named EXE.covcfg
            if ( File.Exists( program + ".covcfg" ) ) {
                using ( var f = File.OpenText( program + ".covcfg" ) ) {
                    string l = null;
                    do {
                        l = f.ReadLine();
                        if ( !string.IsNullOrWhiteSpace( l ) ) {
                            patterns.Add(l);
                        }
                    } while ( l != null );
                }
            }

            CoverHost.RenameBackupFile( program + ".covdb" );
            CoverHost.RenameBackupFile( program + ".covreport" );

            covertool = CoverHostFactory.CreateHost ( program  + ".covdb", program, args.ToArray() );
            debugee = covertool.VirtualMachine.Process;
            ThreadPool.QueueUserWorkItem( (x) => SignalHandler(), null );
            ThreadPool.QueueUserWorkItem( (x) => PumpStdin(x), null );
			covertool.Cover (patterns.ToArray ());
			covertool.Report ( program + ".covreport" );

            return covertool.VirtualMachine.Process.ExitCode;
		}
	}
}
