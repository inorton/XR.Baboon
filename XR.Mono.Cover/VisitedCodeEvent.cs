using System;
using Mono.Debugger.Soft;

namespace XR.Mono.Cover
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
				switch (Type) {
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

}
