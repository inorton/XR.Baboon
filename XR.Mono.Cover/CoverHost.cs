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

		Dictionary<long,StepEventRequest> stepreqs = new Dictionary<long, StepEventRequest> ();

		List<Regex> typeMatchers = new List<Regex> ();

		public CoverHost (params string[] args)
		{
			VirtualMachine = VirtualMachineManager.Launch (args);
			VirtualMachine.EnableEvents (
				EventType.ThreadStart,
				EventType.ThreadDeath,
				EventType.VMDeath
			);


		}

		public void Cover (params string[] typeMatchPatterns)
		{
			bool firstMethod = true;
			var b = VirtualMachine.CreateMethodEntryRequest ();
			b.Enable ();

			StepEventRequest s;

			Resume ();
			do {
				var evts = VirtualMachine.GetNextEventSet ();
				foreach (var e in evts.Events) {

					var met = e as MethodEntryEvent;

					if (met != null) {
						//Console.WriteLine (met.Method.FullName);
						if (firstMethod) {
							s = VirtualMachine.CreateStepRequest (met.Thread);

							var asms = VirtualMachine.RootDomain.GetAssemblies ();
							foreach (var a in asms) {
								Console.WriteLine ("asm {0}", a.GetName ());
								if (a.GetName ().Name == "testsubject") {
									s.AssemblyFilter = new List<AssemblyMirror>{ a };
								}
							}



							s.Filter = StepFilter.DebuggerHidden;

							s.Size = StepSize.Line;
							s.Depth = StepDepth.Into;
							s.Enabled = true;
							stepreqs [met.Thread.Id] = s;
						}
						firstMethod = false;
					}

					var step = e as StepEvent;
					if (step != null) {
						var loc = step.Thread.GetFrames () [0].Location;

						Console.WriteLine ("{0}:{1} ", loc.SourceFile, loc.LineNumber);
						Console.WriteLine (loc.Method.Name);
						foreach ( var l in loc.Method.LineNumbers.Distinct() ){
							Console.WriteLine(" {0}", l );
						}
					}

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
		}


		public void Resume ()
		{
			VirtualMachine.Resume ();
		}
	}

}

