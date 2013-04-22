using System;
using System.Linq;
using XR.Mono.Cover;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Covtool
{
	class MainClass
	{
		public static void Main (string[] vargs)
		{
            if ( vargs.Length == 0 || ( Regex.IsMatch( vargs[0], "-h$|-help$" ) ) ) {
                Console.Error.WriteLine("Usage: covtool RESULTFILE MATCH1..MATCHn -- PROGRAM ARGS");
                Console.Error.WriteLine();
                Console.Error.WriteLine("eg: $ covtool results.db ^XR.Baboon -- nunit-console.exe testsubject.exe");
                Console.Error.WriteLine();
                System.Environment.Exit(1);
            }

			var patterns = new List<string> ();
			var args = new List<string> ();

			bool donePatterns = false;
			foreach (var x in vargs.Skip(1)) 
            {
				if (!donePatterns) 
                {
					if (x != "--") 
                    {
						patterns.Add (x);
					} else {
						donePatterns = true;
					}
				} else {
					args.Add (x);
				}

			}

			var ct = CoverHostFactory.CreateHost (vargs[0], args[0], args.Skip (2).ToArray ());
			ct.Cover (patterns.ToArray ());
			//ct.Report ();
		}
	}
}
