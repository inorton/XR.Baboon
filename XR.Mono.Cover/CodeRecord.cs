using System;
using Mono.Debugger.Soft;
using System.Collections.Generic;

namespace XR.Mono.Cover
{

	public class CodeRecord {
		public List<int> Lines { get; set; }
		public List<int> LineHits { get; set; }
		public string SourceFile { get; set; }
		public string ClassName { get; set; }
		public string MethodName { get; set; }

		public int CallCount { get; set; }
	}

}
