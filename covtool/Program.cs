using System;
using XR.Mono.Cover;

namespace covtool
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			var ct = CoverHostFactory.CreateHost (
				//"/opt/mono/lib/mono/2.0/nunit-console.exe", 
				"testsubject.exe");


			ct.Setup ();
		}
	}
}
