using System;
using Mono.Debugger.Soft;
using System.Collections.Generic;

namespace XR.Mono.Cover
{
	public class CoverHostFactory
	{
		public static CoverHost CreateHost (string covfile, string program, params string[] arguments)
		{
            if (covfile == null)
                throw new ArgumentNullException ("covfile");

			if (program == null)
				throw new ArgumentNullException ("program");

			var args = new List<string> {program};
			args.AddRange (arguments);

            var data = new CodeRecordData();
            data.Open( covfile );

            var rv = new CoverHost ( args.ToArray() ) { DataStore = data };
			return rv;
		}
	}
}
