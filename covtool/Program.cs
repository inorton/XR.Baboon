using System;
using System.IO;
using System.Linq;
using System.Net;
using Mono.Unix;
using Mono.Unix.Native;
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
            Console.WriteLine(@"Usage: covem PROGRAM ARGUMENTS
       covem -a PROGRAM ADDRESS PORT

Launching PROGRAM on covem:
 PROGRAM should be the path to your c# exe (not a shell script)

 Pass covem the same options you would pass to your program.

Attaching to an existent process:
 Pass -a as the first argument.
 PROGRAM should be a convenient string to identify your program.
 ADDRESS should be an IP address the process listening to.
 PORT should be a TCP port the process listening to.

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

Generating simple line coverage information instead of hit counting
can be enabled in the configuration file like this:

$HitCount=false


A static method may be invoked when baboon is ready by telling the
IL name of the method and the name of a thread like this:

$InvokeMethod=Namespace.TypeName.MethodName
$InvokeThread=ThreadName

This is particularly useful to delay the execution of the code you are
interested in before attaching baboon.

baboon can be terminated by calling a method specified with its IL name like this:

$TerminatorMethod=Namespace.TypeName.MethodName

This is particularly useful to terminate baboon but the attached process.

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

        private static void CancelEventHandler(object sender, ConsoleCancelEventArgs args)
        {
            covertool?.Terminate();
        }

        public static void SignalHandler()
        {
            var sigint = new UnixSignal ( Signum.SIGINT );
            var sigusr2 = new UnixSignal( Signum.SIGUSR2 );
            var sigs = new UnixSignal[] { sigint, sigusr2 };
            var covertool = MainClass.covertool;

            while (covertool != null) {
                if ( UnixSignal.WaitAny( sigs, new TimeSpan(0,1,0) ) == 0 ) {
                    covertool.Terminate();
                } else {
                    covertool.SaveData();
                }

                covertool = Volatile.Read(ref MainClass.covertool);
            }
        }

        static Process debugee = null;
        static CoverHost covertool = null;

		public static int Main (string[] vargs)
		{
            var attach = false;
            var index = 0;
            string invokeMethod = null;
            string invokeThread = null;
            string terminatorMethod = null;

            while (index < vargs.Length)
            {
                if ( Regex.IsMatch( vargs[index], "-h$|-help$" ) ) return Usage();
                if ( !Regex.IsMatch( vargs[index], "-a$|-attach$" ) ) break;

                attach = true;
                index++;
            }

            // the first thing is the mono EXE we are running, everything else are args passed to it
            // we do no argument processing at all.

            var program = vargs[index];
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
                        if (l.StartsWith ( "$HitCount=" ) ) {
                            l = l.Substring("$HitCount=".Length);
                            bool.TryParse (l, out hitCount);
                            continue;
                        }
                        if ( l.StartsWith ( "$InvokeMethod=" ) ) {
                            invokeMethod = l.Substring("$InvokeMethod=".Length);
                            continue;
                        }
                        if ( l.StartsWith ( "$InvokeThread=" ) ) {
                            invokeThread = l.Substring("$InvokeThread=".Length);
                            continue;
                        }
                        if ( l.StartsWith ( "$TerminatorMethod=" ) ) {
                            terminatorMethod = l.Substring("$TerminatorMethod=".Length);
                            continue;
                        }
                        patterns.Add (l);
                    } while ( l != null );
                }
            }

            CoverHost.RenameBackupFile( cfgfile + ".covdb" );
            CoverHost.RenameBackupFile( cfgfile + ".covreport" );

            CoverHost covertool;
            Timer timer;

            if ( attach ) {
                if ( !IPAddress.TryParse(vargs[index + 1], out var ip) ) return Usage();
                if ( !int.TryParse(vargs[index + 2], out var port) ) return Usage();

                covertool = CoverHostFactory.CreateHost ( cfgfile  + ".covdb", new IPEndPoint( ip, port ) );
            } else {
                if (!System.IO.File.Exists(program)) return Usage();

                var args = vargs.Skip(index);
                covertool = CoverHostFactory.CreateHost ( cfgfile  + ".covdb", program, args.ToArray() );
                debugee = covertool.VirtualMachine.Process;
                ThreadPool.QueueUserWorkItem( (x) => PumpStdin(x), null );
            }

            covertool.DataStore.SaveMeta( "config", cfgfile );
            covertool.HitCount = hitCount;
            covertool.TerminatorMethod = terminatorMethod;

            MainClass.covertool = covertool;

            if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX) {
                ThreadPool.QueueUserWorkItem( (x) => SignalHandler(), null );
                timer = null;
            } else {
                Console.CancelKeyPress += CancelEventHandler;
                timer = new Timer( state => MainClass.covertool?.SaveData(), null, 0, 1000 );
            }

            try
            {
                covertool.Cover (invokeMethod, invokeThread, patterns.ToArray ());
            } finally {
                MainClass.covertool = null;

                if (timer != null) {
                    timer.Dispose();
                    Console.CancelKeyPress -= CancelEventHandler;
                }
            }

            covertool.Report ( cfgfile + ".covreport" );

            return attach ? 0 : covertool.VirtualMachine.Process.ExitCode;
		}
	}
}
