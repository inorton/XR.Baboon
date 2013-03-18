using System;
using Mono.Debugger.Soft;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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

		public void Setup (params string[] typeMatchPatterns)
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
						Console.WriteLine (met.Method.FullName);
						if (firstMethod) {
							s = VirtualMachine.CreateStepRequest (met.Thread);
							s.Filter = StepFilter.DebuggerHidden;
							s.Size = StepSize.Line;
							s.Depth = StepDepth.Into;
							s.Enabled = true;
						}
						firstMethod = false;
					}

					var step = e as StepEvent;
					if (step != null) {
						Console.WriteLine (step.Thread.GetFrames () [0].Location);
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

