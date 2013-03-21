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

		Dictionary<string, CodeRecord> records = new Dictionary<string, CodeRecord>();

		Dictionary<long,StepEventRequest> stepreqs = new Dictionary<long, StepEventRequest> ();

		List<Regex> typeMatchers = new List<Regex> ();

		List<AssemblyMirror> logAssemblies = new List<AssemblyMirror>();

		public CoverHost ( params string[] args)
		{


			VirtualMachine = VirtualMachineManager.Launch (args);
			VirtualMachine.EnableEvents (
				EventType.VMDeath,
				EventType.TypeLoad
			);
		}

		public bool CheckTypeLoad( Event evt )
		{
			var tl = evt as TypeLoadEvent;
			if ( tl != null ){
				//Console.Error.WriteLine("tlr = "+ tl.Type.FullName);
				foreach ( var rx in typeMatchers ){
					if ( rx.IsMatch( tl.Type.FullName ) ){
						if ( !logAssemblies.Contains( tl.Type.Assembly ) ) {
							logAssemblies.Add( tl.Type.Assembly );
							UpdateStepFilter();
						}
					}
				}
			}
			return tl != null;
		}

		void UpdateStepFilter( ) {
			foreach ( var step in stepreqs.Values ){
				step.Enabled = false;
				step.AssemblyFilter = new List<AssemblyMirror>( logAssemblies );
				step.Enabled = step.AssemblyFilter.Count > 0 || (typeMatchers.Count < 1);
			}
		}

		public bool CheckMethodRequests( Event evt )
		{
			return CheckMethodEntryRequest( evt );
		}

		void SetupThreadStep( MethodEntryEvent meth )
		{
			if ( !stepreqs.ContainsKey( meth.Thread.Id ) ){
				var s = VirtualMachine.CreateStepRequest (meth.Thread);
				s.Filter = StepFilter.DebuggerHidden;
				s.Size = StepSize.Line;
				s.Depth = StepDepth.Into;
				stepreqs [meth.Thread.Id] = s;
				UpdateStepFilter();
			}
		}

		public bool CheckMethodEntryRequest( Event evt )
		{
			var met = evt as MethodEntryEvent;
			if (met != null) {
				SetupThreadStep( met );
				CodeRecord rec = null;
				//Console.Error.WriteLine( met.Method.FullName );
				if ( !records.TryGetValue( met.Method.FullName, out rec ) )
				{
					rec = new CodeRecord() { 
						ClassName = met.Method.DeclaringType.CSharpName,
						MethodName = met.Method.Name,
						Lines = new List<int>( met.Method.LineNumbers ),
						LineHits = new List<int>(),
						SourceFile = met.Method.SourceFile,
					};
					records.Add( met.Method.FullName, rec );
				} 
				rec.CallCount++;
				if ( rec.Lines.Count > 0 ) {
					rec.LineHits = new List<int>() { rec.Lines[0] };
				}
			}
			return met != null;
		}

		public bool CheckStepRequest( Event evt ) 
		{
			var step = evt as StepEvent;
			if (step != null) {

				CodeRecord rec = null;
				if ( records.TryGetValue( step.Method.FullName, out rec ) ){
					var loc = step.Thread.GetFrames () [0].Location;
					rec.LineHits.Add( loc.LineNumber );
					Console.Error.WriteLine( loc );
				}
			}
			return step != null;
		}

		public void Cover (params string[] typeMatchPatterns)
		{
			foreach ( var t in typeMatchPatterns ){
				var r = new Regex( t );
				typeMatchers.Add(r);
			}

			//bool firstMethod = true;
			var b = VirtualMachine.CreateMethodEntryRequest ();
			b.Enable ();

			//StepEventRequest s;

			Resume ();
			do {
				var evts = VirtualMachine.GetNextEventSet ();
				foreach (var e in evts.Events) {

					if ( CheckMethodRequests(e) ) continue;
					if ( CheckStepRequest(e) ) continue;
					if ( CheckTypeLoad(e) ) continue;

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

