using System;
using Mono.Debugger.Soft;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace XR.Mono.Cover
{

    public class CoverHost
    {
        public VirtualMachine VirtualMachine { get; private set; }

        Dictionary<string, CodeRecord> records = new Dictionary<string, CodeRecord> ();

        Dictionary<long,StepEventRequest> stepreqs = new Dictionary<long, StepEventRequest> ();

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
                                    records.Add (m.FullName, rec);
                                    DataStore.RegisterMethod( rec );
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

        void SetupThreadStep (MethodEntryEvent meth)
        {
            if (!stepreqs.ContainsKey (meth.Thread.Id)) {
                var s = VirtualMachine.CreateStepRequest (meth.Thread);
                s.Filter = StepFilter.DebuggerHidden;
                s.Size = StepSize.Line;
                s.Depth = StepDepth.Into;
                stepreqs [meth.Thread.Id] = s;
                s.Enabled = true;
            }
        }

        public bool CheckMethodEntryRequest (Event evt)
        {
            var met = evt as MethodEntryEvent;
            if (met != null) {
                SetupThreadStep (met);
                CodeRecord rec = null;
                //Console.Error.WriteLine( met.Method.FullName );
                if (records.TryGetValue (met.Method.FullName, out rec)) {
                    rec.CallCount++;
                    if (rec.Lines.Count > 0) 
                        rec.Hit (rec.Lines [0]);

                }
            }
            return met != null;
        }

        public bool CheckStepRequest (Event evt)
        {
            var step = evt as StepEvent;
            if (step != null) {

                CodeRecord rec = null;
                if (records.TryGetValue (step.Method.FullName, out rec)) {
                    var loc = step.Thread.GetFrames () [0].Location;
                    if (loc.LineNumber > 0) {
                        rec.Hit (loc.LineNumber);
                        //Console.Error.WriteLine( loc );
                        //System.Threading.Thread.Sleep(1000);
                    }
                }
            }
            return step != null;
        }

        public void Cover (params string[] typeMatchPatterns)
        {
            foreach (var t in typeMatchPatterns) {
                var r = new Regex (t);
                typeMatchers.Add (r);
            }

            //bool firstMethod = true;
            var b = VirtualMachine.CreateMethodEntryRequest ();
            b.Enable ();

            //StepEventRequest s;

            Resume ();
            try {
                do {
                    var evts = VirtualMachine.GetNextEventSet ();
                    foreach (var e in evts.Events) {

                        if (CheckMethodRequests (e))
                            continue;
                        if (CheckStepRequest (e))
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
            } finally {

                // record stats
                foreach ( var rec in records.Values ) {
                    DataStore.RegisterCalls( rec );
                    DataStore.RegisterHits( rec );
                }

                DataStore.Close();
            }
        }

        public void Resume ()
        {
            VirtualMachine.Resume ();
        }

        public void Report ()
        {
            var rv = records.Values.ToArray ();
            Array.Sort (rv, (CodeRecord x, CodeRecord y) => {
                var xa = string.Format (x.ClassName + ":" + x.Name);
                var ya = string.Format (y.ClassName + ":" + y.Name);

                return xa.CompareTo (ya);
            });

            foreach (var r in rv) {
                if (r.Lines.Count > 0) {
                    Console.WriteLine (r);
                    foreach (var l in r.Lines.Distinct()) {
                        var hits = (from x in r.LineHits where x == l select x).Count ();
                        Console.WriteLine ("{0}:{1:0000} {2}", r.SourceFile, l, hits);
                    }
                }
            }
        }
    }

}

