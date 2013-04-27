using System;
using System.IO;
using Mono.Debugger.Soft;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

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
        public VirtualMachine VirtualMachine { get; private set; }

        Dictionary<string, CodeRecord> records = new Dictionary<string, CodeRecord> ();

        List<Regex> typeMatchers = new List<Regex> ();

        //List<AssemblyMirror> logAssemblies = new List<AssemblyMirror> ();

        public CodeRecordData DataStore { get; set; }

        public CoverHost (params string[] args)
        {
            VirtualMachine = VirtualMachineManager.Launch (args);
            VirtualMachine.EnableEvents (
				EventType.VMDeath,
				EventType.TypeLoad
            );

        }

        Dictionary<string,List<BreakPoint>> bps = new Dictionary<string, List<BreakPoint>> ();
        Dictionary<BreakpointEventRequest,BreakPoint> rbps = new Dictionary<BreakpointEventRequest, BreakPoint> ();

        public bool CheckTypeLoad (Event evt)
        {
            var tl = evt as TypeLoadEvent;
            if (tl != null) {
                //Console.Error.WriteLine("tlr = "+ tl.Type.FullName);
                foreach (var rx in typeMatchers) {
                    if (rx.IsMatch (tl.Type.FullName)) {
                       
                        var meths = tl.Type.GetMethods ();
                        // make a record for all methods defined by this type
                        foreach (var m in meths) {
                            CodeRecord rec;
                            if (!records.TryGetValue (m.FullName, out rec)) {
                                //Console.Error.WriteLine("adding {0}",m.FullName);
                                rec = new CodeRecord () { 
										ClassName = m.DeclaringType.CSharpName,
                                        Assembly = m.DeclaringType.Assembly.GetName().FullName,
										Name = m.Name,
										FullMethodName = m.FullName,
										Lines = new List<int>( m.LineNumbers ),
										SourceFile = m.SourceFile,
									};

                                if (!bps.ContainsKey (m.FullName)) {
                                    bps [m.FullName] = new List<BreakPoint> ();
                                    // add a break on each line
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
                }
            }
            return tl != null;
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
                //Console.Error.WriteLine( met.Method.FullName );
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
                    rec.Hit (bp.Location.LineNumber);

                }
            }
            return bpe != null;
        }

        public void Cover (params string[] typeMatchPatterns)
        {


            foreach (var t in typeMatchPatterns) {
                var r = new Regex (t);
                typeMatchers.Add (r);
            }

            try {

                var b = VirtualMachine.CreateMethodEntryRequest ();
                b.Enable ();

                Resume ();

                do {
                    var evts = VirtualMachine.GetNextEventSet ();
                    foreach (var e in evts.Events) {

                        if (CheckBreakPointRequest (e))
                            continue;

                        if (CheckMethodRequests (e))
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
                    if (VirtualMachine.TargetProcess.HasExited)
                        break;
                } while ( true );
            } catch (Exception ex) {
                if (File.Exists ("covhost.error"))
                    File.Delete ("covhost.error");
                using (var f = new StreamWriter("covhost.error")) {
                    f.Write (ex.ToString ());
                }
            } finally {
                SaveData ();

                DataStore.Close ();
            }
        }

        public void SaveData ()
        {
            // record stats
            foreach (var rec in records.Values) {
                DataStore.RegisterCalls (rec);
                DataStore.RegisterHits (rec);
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
                    if (r.Lines.Count > 0) {
                        f.WriteLine (r);
                        foreach (var l in r.Lines.Distinct()) {
                            var hits = (from x in r.LineHits where x == l select x).Count ();
                            f.WriteLine ("{0}:{1:0000} {2}", r.SourceFile, l, hits);
                        }
                    }
                }
            }
        }
    }

}

