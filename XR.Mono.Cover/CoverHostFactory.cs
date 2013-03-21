using System;
using Mono.Debugger.Soft;
using System.Collections.Generic;

namespace XR.Mono.Cover
{
	public class CoverHostFactory
	{
		public static CoverHost CreateHost (string program, params string[] arguments)
		{
			if (program == null)
				throw new ArgumentNullException ("program");

			var args = new List<string> {program};
			args.AddRange (arguments);
			var rv = new CoverHost ( args.ToArray ());
			return rv;
		}
	}
}
