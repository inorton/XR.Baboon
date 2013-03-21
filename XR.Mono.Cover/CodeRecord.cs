using System;
using Mono.Debugger.Soft;
using System.Collections.Generic;
using System.Linq;

namespace XR.Mono.Cover
{

	public class CodeRecord {
		public List<int> Lines { get; set; }
		public List<int> LineHits { get; set; }
		public string SourceFile { get; set; }
		public string ClassName { get; set; }
		public string Name { get; set; }
		public string FullMethodName { get; set; }
		public int CallCount { get; set; }

		public double Coverage {
			get {
				var hits =  LineHits.Distinct();
				return (hits.Count() * 1.0) / Lines.Distinct().Count();
			}
		}

		public override string ToString() 
		{
			return String.Format("{0}:{1},Calls={2},Coverage={3:00.0}%,{4}", ClassName, Name, CallCount, 100 * Coverage, FullMethodName );
		}


	}

}
