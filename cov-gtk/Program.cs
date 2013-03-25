using System;
using Gtk;
using XR.Mono.Cover;

namespace XR.Baboon
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Application.Init ();
			MainWindow win = new MainWindow ();

            if ( args.Length > 0 )
            {
                var records = args[0];
                var dh = new CodeRecordData();
                dh.Open( records );
                var crs = dh.Load();
                win.Load( crs );
            }

			win.Show ();
			Application.Run ();
		}
	}
}
