using System;
using Mono.Debugger.Soft;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace covem
{
	public class VisitedCodeEvent
	{
		public VisitedCodeEvent ()
		{
			Logged = DateTime.Now;
		}

		public int Thread { get; set; }

		public string MethodName { get; set; }

		public EventType Type { get; set; }

		public string SourceFile { get; set; }

		public int LineNumber { get; set; }

		public DateTime Logged { get; private set; }

		public string LogSymbol { 
			get {
				switch ( Type ) {
				case EventType.MethodEntry:
					return ">>";
				case EventType.MethodExit:
					return " <";
				case EventType.Exception:
					return "ex";
				case EventType.ThreadStart:
					return "t>";
				case EventType.ThreadDeath:
					return "t<";
				case EventType.UserBreak:
					return "b>"; // method caller location
				default:
					return "  ";
				}
			}
		}
	}

	class MainClass
	{
		static Regex filter = new Regex ("^covem.");
		static List<VisitedCodeEvent> log = new List<VisitedCodeEvent> ();

		static string GetFullMethodName( MethodMirror m )
		{
			return string.Format("{0}.{1}", m.DeclaringType.FullName, m.Name );
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
						if ( at.LineNumber < 1 ){
							var tmp = caller.Method.LineNumbers.FirstOrDefault();
							if ( tmp > 0 ) at.LineNumber = tmp;
						}
					}

					var me = new VisitedCodeEvent () {
							MethodName = GetFullMethodName(frame.Method),
							Type = evt.EventType,
							LineNumber = frame.LineNumber,
							SourceFile = frame.FileName,
						};
					log.Add (me);

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
				var v = VirtualMachineManager.Launch (new String[] { "covem.exe", "x" });

				v.EnableEvents (new EventType[] {
					EventType.Exception,
					EventType.ThreadStart,
					EventType.ThreadDeath,
					EventType.MethodEntry, 
					EventType.MethodExit });

				v.Process.Start ();

				EventSet es = null;

				do {
					es = v.GetNextEventSet ();
					if (es != null) {

						foreach (var et in es.Events) {
							if ( et is ExceptionEvent ) {
								Console.Error.WriteLine(et);
							}
							FilterEvent (et);
						}
						try {
							v.Resume ();
						} catch {
							break;
						}
					} else { 
						System.Threading.Thread.Sleep (50);
					}

				} while ( !v.Process.HasExited );


				foreach ( var e in log ) {
					if ( filter.IsMatch( e.MethodName ) ) {

						Console.Error.WriteLine("{0}.{1} {2} {3} {4}:{5}", e.Logged.ToString("s"), e.Logged.Millisecond,
						                    e.LogSymbol,
					                        e.MethodName,
					                        e.SourceFile,
					                        e.LineNumber );

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
