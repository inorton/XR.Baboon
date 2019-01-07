using System;
using System.IO;
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

            var rv = new CoverHost ( args.ToArray() );
            PrepareHost( rv, covfile );
			return rv;
		}

		public static CoverHost CreateHost (string covfile, System.Net.IPEndPoint ip)
		{
            if (covfile == null)
                throw new ArgumentNullException ("covfile");

			if (ip == null)
				throw new ArgumentNullException ("ip");

            var rv = new CoverHost ( ip );
            PrepareHost( rv, covfile );
			return rv;
        }

        private static void PrepareHost (CoverHost host, string covfile)
        {
            if (covfile == null)
                throw new ArgumentNullException ("covfile");

            var data = new CodeRecordData();
            data.Open( covfile );

            var logfile = covfile + ".log";
            var log = new StreamWriter( logfile );
            data.SaveMeta("logfile", logfile);
            data.SaveMeta("dbfile", covfile);
            data.SaveMeta("testmachine", Environment.MachineName );

            host.DataStore = data;
            host.LogFile = log;
        }
	}
}
