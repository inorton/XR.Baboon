using System;
using Mono.Debugger.Soft;
using System.Collections.Generic;

namespace XR.Mono.Cover
{

	public class MethodData {
		public List<int> Lines { get; set; }
		public Dictionary<int,int> LineHits { get; set; }
		public string SourceFile { get; set; }
		public string ClassName { get; set; }
		public string MethodName { get; set; }

		public int CallCount { 
			get {
				if ( CallTimes == null ) return 0;
				return CallTimes.Count;
			}
		}
		public List<TimeSpan> CallTimes { get; set; }
	}

}
