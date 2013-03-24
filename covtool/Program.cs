using System;
using System.Linq;
using XR.Mono.Cover;
using System.Collections.Generic;

namespace Covtool
{
	class MainClass
	{
		public static void Main (string[] vargs)
		{
			var patterns = new List<string> ();
			var args = new List<string> ();

			bool donePatterns = false;
			foreach (var x in vargs) {
				if (!donePatterns) {
					if (x != "--") {
						patterns.Add (x);
					} else {
						donePatterns = true;
					}
				} else {
					args.Add (x);
				}

			}

			var ct = CoverHostFactory.CreateHost (args.First (), args.Skip (1).ToArray ());
			ct.Cover (patterns.ToArray ());
			ct.Report ();
		}
	}
}
