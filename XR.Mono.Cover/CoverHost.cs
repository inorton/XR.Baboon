using System;
using System.IO;
using Mono.Debugger.Soft;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading;

namespace XR.Mono.Cover
{

    class BreakPoint
    {
        public BreakpointEventRequest Request { get; set; }
        public Location Location { get; set; }
        public CodeRecord Record { get; set; }
    }


    public class CoverHost
    {
        public TextWriter LogFile { get; set; }

        public VirtualMachine VirtualMachine { get; private set; }

        Dictionary<string, CodeRecord> records = new Dictionary<string, CodeRecord> ();

        List<Regex> typeMatchers = new List<Regex> ();

        List<string> assemblyMatchers = new List<string>();

        public CodeRecordData DataStore { get; set; }


        long logcount = 0;

        public void Log( string fmt, params object[] args )
        {
            if ( LogFile != null ){
                LogFile.WriteLine( fmt, args );
                logcount++;

                if ( logcount % 10 == 0 ) {
                    LogFile.Flush();
                }
            }
        }

        public static CoverHost Singleton { get; private set;}

        string[] cmdargs;

        public CoverHost (params string[] args)
        {
            cmdargs = args;
            VirtualMachine = VirtualMachineManager.Launch (args, new LaunchOptions() { AgentArgs = "suspend=y" } );
            VirtualMachine.EnableEvents (
                EventType.VMStart,
				EventType.VMDeath,
                EventType.VMDisconnect,
				EventType.AssemblyLoad,
                EventType.TypeLoad
            );
            Singleton = this;
        }

        Dictionary<string,List<BreakPoint>> bps = new Dictionary<string, List<BreakPoint>> ();
        Dictionary<BreakpointEventRequest,BreakPoint> rbps = new Dictionary<BreakpointEventRequest, BreakPoint> ();

        HashSet <string> loadedTypes = new HashSet<string>();
        HashSet <string> loadedAssemblies = new HashSet<string>();

        bool MatchType( Type type )
        {
            foreach ( var rx in typeMatchers )
            {
                if ( rx.IsMatch( type.FullName ) ) return true;
            }
            return false;
        }

        void MarkAssembly( AssemblyMirror a )
        {
            var asmname = a.GetName();
            var name = asmname.FullName;
            if ( loadedAssemblies.Contains( name ) ) return;
            bool loadAll = false;
            if ( assemblyMatchers.Contains( asmname.Name ) ) loadAll = true;

            var afile = a.Location;
            Log ("load assembly {0} from {1}", name, afile);

            if ( File.Exists( afile ) ){
                Log ("inspecting {0} for types", afile );
                try {
                    var asm = System.Reflection.Assembly.ReflectionOnlyLoadFrom( afile );
                    loadedAssemblies.Add( name );
                    if ( asm != null ){
                        Log ("loaded {0}", afile);
                        foreach ( var t in asm.GetTypes() )
                        {
                            if ( loadAll || MatchType( t ) ) 
                            {
                                Log ("matched type {0}", t.FullName);
                                MarkLoadedType( a, t );
                            }
                        }
                    }
                } catch ( System.Reflection.ReflectionTypeLoadException ) {
                    Log ("warning: could not load types from {0}", afile );
                } catch ( SystemException ) {
                    Log ("warning: possible ExecutionEngineException with {0}", afile);
                } finally {
                    loadedAssemblies.Add( name ); // don't try again
                }

            } else {
                Log ("assembly file {0} missing", afile);
            }

        }

        void MarkLoadedType( AssemblyMirror a, Type t )
        {
            if ( loadedTypes.Contains( t.FullName ) ) return;

            var tm = a.GetType( t.FullName );

            Log("adding reflected matched type {0}",tm.FullName);

            MarkType( tm );

        }

        void MarkType( TypeMirror t )
        {
            MarkAssembly( t.Assembly );

            if ( loadedTypes.Contains( t.FullName ) ) return;

            Log("adding matched type {0}",t.FullName);

            loadedTypes.Add( t.FullName );

            var meths = t.GetMethods ();
            // make a record for all methods defined by this type
            foreach (var m in meths) {
                CodeRecord rec;
                if (!records.TryGetValue (m.FullName, out rec)) {
                    Log("adding matched method {0}",m.FullName);
                    rec = new CodeRecord () { 
                        ClassName = m.DeclaringType.CSharpName,
                        Assembly = m.DeclaringType.Assembly.GetName().FullName,
                        Name = m.Name,
                        FullMethodName = m.FullName,
                        SourceFile = m.SourceFile,
                    };
                    rec.AddLines( m.LineNumbers.ToArray() );
                    
                    if (!bps.ContainsKey (m.FullName)) {
                        bps [m.FullName] = new List<BreakPoint> ();
                        // add a break on each line
                        Log("adding {0} breakpoints", m.Locations.Count);
                        foreach (var l in m.Locations) {
                            var bp = VirtualMachine.CreateBreakpointRequest (l);
                            var b = new BreakPoint () { Location = l, Record = rec, Request = bp };
                            bps [m.FullName].Add (b);
                            rbps [bp] = b;
                            bp.Enabled = true;
                        }
                    }
                    
                    records.Add (m.FullName, rec);
                    DataStore.RegisterMethod (rec);
                } 
            }



        }

        public bool CheckTypeLoad (Event evt)
        {
            var tl = evt as TypeLoadEvent;
            if (tl != null) {
                Log("TypeLoadEvent {0}", tl.Type.FullName);
                foreach (var rx in typeMatchers) {
                    if (rx.IsMatch (tl.Type.FullName)) {
                        MarkType( tl.Type );
                    }
                }
            }
            return tl != null;
        }

        public bool CheckAssemblyLoad( Event evt )
        {
            var al = evt as AssemblyLoadEvent;
            if ( al != null ) {
                MarkAssembly( al.Assembly );
                return true;
            }
            return false;
        }

        public bool CheckMethodRequests (Event evt)
        {
            return CheckMethodEntryRequest (evt);
        }


        public bool CheckMethodEntryRequest (Event evt)
        {
            var met = evt as MethodEntryEvent;
            if (met != null) {
                CodeRecord rec = null;
                Log( "call {0}", met.Method.FullName );
                if (records.TryGetValue (met.Method.FullName, out rec)) {
                    rec.CallCount++;
                    //if (rec.Lines.Count > 0) 
                    //    rec.Hit (rec.Lines [0]);

                }
            }
            return met != null;
        }

        public bool CheckBreakPointRequest (Event evt)
        {
            var bpe = evt as BreakpointEvent;
            if (bpe != null) {
                BreakPoint bp = null;
                if (rbps.TryGetValue (bpe.Request as BreakpointEventRequest, out bp)) {
                    CodeRecord rec = bp.Record;
                    lock ( DataStore )
                    {
                        rec.Hit (bp.Location.LineNumber);
                    
                        if ( bp.Location.LineNumber == bp.Record.GetFirstLine() ) {
                            rec.CallCount++;
                        }
                    }

                }
            }
            return bpe != null;
        }

        public bool AddAssemblyMatcher( string pattern )
        {
            if ( pattern.StartsWith("assembly:") ){
                var tmp = pattern.Split( new char[] {':'}, 2 );
                if ( tmp.Length == 2 ) {
                    var asmname = tmp[1];

                    if ( !assemblyMatchers.Contains(asmname) ) {
                        assemblyMatchers.Add( asmname );
                        DataStore.SaveMeta( String.Format("match:{0:000}", assemblyMatchers.Count), pattern);
                    }

                    return true;
                }
            }
            return false;
        }

        public void Cover (params string[] typeMatchPatterns)
        {
            DataStore.SaveMeta("commandline", string.Join(" ", cmdargs));
            DataStore.SaveMeta("started", DateTime.Now.ToString("s") );
            for ( int i = 0; i < typeMatchPatterns.Length; i++ )
            {
                var t = typeMatchPatterns[i];
                if ( !AddAssemblyMatcher(t) ) 
                {
                    DataStore.SaveMeta( String.Format("match:{0:000}", i), t);
                    var r = new Regex (t);
                    typeMatchers.Add (r);
                }
            }

            try {
                var b = VirtualMachine.CreateAssemblyLoadRequest();
                b.Enable();

                Resume ();

                do {
                    var evts = VirtualMachine.GetNextEventSet ();
                    foreach (var e in evts.Events) {

                        if (CheckBreakPointRequest (e))
                            continue;

                        //if (CheckMethodRequests (e))
                        //    continue;

                        if (CheckAssemblyLoad(e))
                            continue;

                        if (CheckTypeLoad (e))
                            continue;

                        if (e is VMDisconnectEvent)
                            return;
                    }

                    if (evts.Events.Length > 0) {
                        try {
                            Resume ();
                        } catch (InvalidOperationException) {

                        }
                    }
                    if (VirtualMachine.TargetProcess.HasExited) {
                        Log ( "debugee has exited" );
                        break;
                    }
                } while ( true );
            } catch (VMDisconnectedException) {
                Log ( "vm disconnected" );
            } catch (Exception ex) {
                Log ( "{0}", ex );
                if (File.Exists ("covhost.error"))
                    File.Delete ("covhost.error");
                using (var f = new StreamWriter("covhost.error")) {
                    f.Write (ex.ToString ());
                }
            } finally {

                if ( !VirtualMachine.Process.HasExited )
                    VirtualMachine.Process.Kill();

                Log("saving data");

                SaveData ( true );

                Log("quitting");
                LogFile.Flush();
                LogFile.Close();
            }
        }

        
        public void SaveData ( )
        {
            SaveData(false);
        }

        public void SaveData ( bool wait )
        {
            bool done = false;
            List<CodeRecord> recs = new List<CodeRecord>();
            // record stats
            lock ( DataStore ){
                recs.AddRange( records.Values );
            }

            ThreadPool.QueueUserWorkItem( (x) => {
                var list = recs as List<CodeRecord>;
                Log ("saving records in background");
                DataStore.RegisterHits( list, true );
                Log("save complete");
                done = true;
            }, recs );

            if ( wait ) {
                while ( !done ) Thread.Sleep(1000);
            }

        }

        public void Resume ()
        {
            VirtualMachine.Resume ();
        }

        public static void RenameBackupFile (string filename)
        {
            if (File.Exists (filename)) {
                var dt = File.GetCreationTime (filename)
                    .ToUniversalTime ().Subtract (new DateTime (1970, 1, 1));
                File.Move (filename, String.Format ("{0}.{1}", filename, (int)dt.TotalSeconds));
            }
        }

        public void Report (string filename)
        {
            RenameBackupFile (filename);

            using (var f = new StreamWriter( filename )) {
                var rv = records.Values.ToArray ();
                Array.Sort (rv, (CodeRecord x, CodeRecord y) => {
                    var xa = string.Format (x.ClassName + "\t:" + x.Name);
                    var ya = string.Format (y.ClassName + ":" + y.Name);

                    return xa.CompareTo (ya);
                });

                foreach (var r in rv) {
                    var lines = r.GetLines();
                    if (lines.Length > 0) {
                        f.WriteLine (r);
                        foreach (var l in lines) {
                            var hits = r.GetHits(l);
                            f.WriteLine ("{0}:{1:0000} {2}", r.SourceFile, l, hits);
                        }
                    }
                }
            }
        }
    }

}

