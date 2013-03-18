using System;
using Mono.Debugger.Soft;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using XR.Mono.Cover;

namespace covem
{

	class MainClass
	{
		static Regex filter = new Regex ("^covem.");
		static List<VisitedCodeEvent> log = new List<VisitedCodeEvent> ();
		static VirtualMachine virtualMachine;
		static List<BreakpointEventRequest> breakpoints = new List<BreakpointEventRequest> ();
		static StepEventRequest stepreq = null;

		static string GetFullMethodName (MethodMirror m)
		{
			return string.Format ("{0}.{1}", m.DeclaringType.FullName, m.Name);
		}

		public static void FilterEvent (Event evt)
		{
			if (evt == null)
				return;
			var ment = evt as MethodEntryEvent;
			var mout = evt as MethodExitEvent;
			var mexp = evt as ExceptionEvent;
	
			MethodMirror source = null;

			if (ment != null)
				source = ment.Method;
			if (mout != null)
				source = mout.Method;

			if (source == null && mexp == null)
				return;

			var frame = evt.Thread.GetFrames ().FirstOrDefault ();

			if (frame != null) {


				switch (evt.EventType) {
				case EventType.MethodEntry:

					var caller = evt.Thread.GetFrames ().Skip (1).FirstOrDefault ();
					if (caller != null) {

						var at = new VisitedCodeEvent () { 
								MethodName = GetFullMethodName (caller.Method),
								Type = EventType.UserBreak,
								SourceFile = caller.FileName,
								LineNumber = caller.LineNumber
							};
						log.Add (at);
						if (at.LineNumber < 1) {
							var tmp = caller.Method.LineNumbers.FirstOrDefault ();
							if (tmp > 0)
								at.LineNumber = tmp;
						}

					}

					var me = new VisitedCodeEvent () {
							MethodName = GetFullMethodName(frame.Method),
							Type = evt.EventType,
							LineNumber = frame.LineNumber,
							SourceFile = frame.FileName,
						};
					log.Add (me);

					// ok, we are calling a method, if we care about this namespace, set a breakpoint on every line of the method body
					if (filter.IsMatch (me.MethodName)) {


						virtualMachine.Resume ();

						//foreach (var o in oss) {
						// var location = frame.Method.LocationAtILOffset( o );
						//var beq = virtualMachine.SetBreakpoint (frame.Method, o);
						//breakpoints.Add (beq);

						//}
					}

					break;

				case EventType.Step:
					stepreq = null;
					break;

				case EventType.UserBreak:
					var bp = evt as BreakpointEvent;
					if (bp != null) {
						//Console.Error.WriteLine( bp.
					}
					break;

				case EventType.Exception:
					var mex = new VisitedCodeEvent () {
						MethodName = GetFullMethodName(frame.Method),
						Type = evt.EventType,
						LineNumber = frame.LineNumber,
						SourceFile = frame.FileName,
					};
					log.Add (mex);

					break;

				case EventType.MethodExit:
					var mx = new VisitedCodeEvent () {
							MethodName = GetFullMethodName(frame.Method),
							Type = evt.EventType,
							LineNumber = frame.LineNumber,
							SourceFile = frame.FileName,
						};
					log.Add (mx);

					break;
				default:
					break;
				}

			}


			return;
		}

		public static void Main (string[] args)
		{

			if (args.Length == 0) {
				virtualMachine = VirtualMachineManager.Launch (new String[] { "covem.exe", "x" });

				virtualMachine.EnableEvents (new EventType[] {
					EventType.Exception,
					EventType.UserBreak,
					EventType.ThreadStart,
					EventType.ThreadDeath,
					EventType.MethodEntry, 
					EventType.MethodExit });

				virtualMachine.Process.Start ();
				//virtualMachine.ClearAllBreakpoints ();
				//var oss = frame.Method.ILOffsets;
				if (stepreq == null) {
					var sr = virtualMachine.CreateStepRequest (virtualMachine.GetThreads ().First ());
					sr.Depth = StepDepth.Over;
					sr.Size = StepSize.Line;
					sr.Enabled = true;
					stepreq = sr;
				}

				EventSet es = null;

				do {
					es = virtualMachine.GetNextEventSet ();
					if (es != null) {

						foreach (var et in es.Events) {
							Console.Error.WriteLine (et);

							FilterEvent (et);
						}
						try {

							virtualMachine.Resume ();

						} catch (Exception ex) {
							//break;
						}
					} else { 
						System.Threading.Thread.Sleep (50);
					}

				} while ( !virtualMachine.Process.HasExited );


				foreach (var e in log) {
					if (filter.IsMatch (e.MethodName)) {

						Console.Error.WriteLine ("{0}.{1} {2} {3} {4}:{5}", e.Logged.ToString ("s"), e.Logged.Millisecond,
						                    e.LogSymbol,
					                        e.MethodName,
					                        e.SourceFile,
					                        e.LineNumber);

					}
				}

				Console.Error.WriteLine ("done");

			} else {
				var f = new Foo ();
				try {
					f.DoStuff ();
					throw new NullReferenceException ("xxx");
				} catch (NullReferenceException) {
					Console.Error.WriteLine ("fooooo");
				}
			}
		}
	}

	public class Foo
	{
		public void DoStuff ()
		{
			Console.WriteLine ("Stuff..");
		}
	}
}
