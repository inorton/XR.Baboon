using System;
using System.IO;
using Gtk;
using System.Collections.Generic;
using XR.Mono.Cover;
using System.Linq;

namespace XR.Baboon
{
	public partial class MainWindow: Gtk.Window
	{
		TreeItemManager treeManager = new TreeItemManager ();

		public const string VisitedOnceBG = "#aaffaa";
		public const string VisitedMoreBG = "#88cc88";
        public const string NotVisitedBG = "#ffcccc";


		public void RenderCoverage (TextBuffer buf, CodeRecord rec)
		{
			foreach (var line in rec.GetLines()) {
				var hits = rec.GetHits (line);
                string tag = "visited_never";
                if (hits > 0) {
                    tag = hits == 1 ? "visited_once" : "visited_more";
                }
                buf.ApplyTag (tag, buf.GetIterAtLine (line - 1), buf.GetIterAtLine (line));
			}
		}

		static List<string> openFiles = new List<string> ();

		public static void OnCloseSourceFile (string file)
		{
			if (openFiles.Contains (file))
				openFiles.Remove (file);
		}

		Dictionary<string,TextView> sourceviews = new Dictionary<string, TextView> ();
		FilesystemMap fsmap = new FilesystemMap ();

		public void OpenSourceFile (List<CodeRecord> recs)
		{
			if (recs.Count == 0)
				return;

			var filename = recs [0].SourceFile;
			var origfile = filename;
			var fbname = System.IO.Path.GetFileName (filename);
			if (openFiles.Contains (filename))
				return;

			if (fsmap.SourceMap.ContainsKey (filename)) {
				filename = fsmap.SourceMap [filename];
			}

			while (!File.Exists (filename)) {
				var fc = new FileChooserDialog ("Locate source file " + origfile,
					         this, FileChooserAction.SelectFolder,
					         "Cancel", ResponseType.Cancel,
					         "Select", ResponseType.Apply);
				fc.Filter = new Gtk.FileFilter (){ Name = fbname };
				fc.Filter.AddPattern (fbname);

				fc.Response += (o, args) => {
					Console.Error.WriteLine (fc.Filename);
					fc.Hide ();

				};

				fc.Run ();

				if (fc.Filename != null) {
					filename = System.IO.Path.Combine (fc.Filename, fbname);
				} else {
					return;
				}
			}
			fsmap.AddMapping (origfile, filename);


			openFiles.Add (origfile);

			var buf = new TextBuffer (new TextTagTable ());

			TextTag visitedOnce = new TextTag ("visited_once") { Background = VisitedOnceBG };
			TextTag visitedMore = new TextTag ("visited_more") { Background = VisitedMoreBG };
            TextTag visitedNever = new TextTag ("visited_never") { Background = NotVisitedBG };
			buf.TagTable.Add (visitedOnce);
			buf.TagTable.Add (visitedMore);
            buf.TagTable.Add (visitedNever);
			// buf.HighlightSyntax = true;
			buf.Text = System.IO.File.ReadAllText (filename);

			var page = new SourcePage ();

			var sv = new TextView (buf);

			sv.Editable = false;

			var fp = System.IO.Path.GetFullPath (filename);

			page.Window.Add (sv);
			page.SetHeadingText (fp);
			page.SetSubHeadingText ("");


			var fname = System.IO.Path.GetFileName (filename);

			var tab = CloserTabLabel.InsertTabPage (notebook1, page, fname);
			tab.CloseKeyData = filename;



			page.ShowAll ();

			int total_lines = 0;
			int covered_lines = 0;

			var text_lines = File.ReadAllLines (filename);
			int line = 1;
			foreach (var text_line in text_lines) {
				buf.Text += String.Format ("{0:-4} {1}\n", line, text_line);
			}

			buf.Text = File.ReadAllText (filename);


			foreach (var rec in recs) {
				RenderCoverage (buf, rec);
				total_lines += rec.GetLines ().Length;
				covered_lines += rec.GetHits ();
			}

			double cov = (1.0 * covered_lines) / total_lines;

			page.SetCoverage (cov);



			notebook1.Page = notebook1.NPages - 1;

			sourceviews [filename] = sv;

		}

		public MainWindow () : base (Gtk.WindowType.Toplevel)
		{
			Build ();

			itemtree.Model = treeManager.TStore;
			var namerender = new CellRendererText ();
			var covrender = new CellRendererText ();
			
			var namecol = new TreeViewColumn ();
			namecol.Title = "Name";
			namecol.PackStart (namerender, true);
			namecol.AddAttribute (namerender, "text", 0);
			namecol.SetCellDataFunc (namerender, CodeRecordCellRenderFuncs.RenderName);
			
			
			var covcol = new TreeViewColumn ();
			covcol.Title = "%";
			covcol.PackStart (covrender, true);
			covcol.AddAttribute (covrender, "text", 1);
			covcol.SetCellDataFunc (covrender, CodeRecordCellRenderFuncs.RenderCoverage);
			
			itemtree.AppendColumn (covcol);
			itemtree.AppendColumn (namecol);

			itemtree.RowActivated += OnItemtreeRowActivated;

			this.ShowAll ();
		}


		List<CodeRecord> records;

		public void Load (List<CodeRecord> code)
		{
			records = code;
			var recs = new Dictionary<string, List<CodeRecord>> ();
			foreach (var rec in code) {
				if (rec.GetLines ().Length == 0)
					continue;
				if (!recs.ContainsKey (rec.ClassName))
					recs [rec.ClassName] = new List<CodeRecord> ();

				recs [rec.ClassName].Add (rec);
			}

			var types = recs.Keys.ToArray ();
			Array.Sort (types);

			foreach (var t in types) {
				treeManager.AddMethods (t, recs [t]);
			}

			itemtree.ExpandAll ();

		}

		protected void OnDeleteEvent (object sender, DeleteEventArgs a)
		{
			Application.Quit ();
			a.RetVal = true;
		}

		protected void OnItemtreeRowActivated (object o, RowActivatedArgs args)
		{
			var rec = treeManager.GetItem (args.Path, 0);
			if (rec != null) {
				if (rec.SourceFile != null) {
					var toopen = from r in records
					             where r.SourceFile == rec.SourceFile
					             select r;
					var tmp = new List<CodeRecord> (toopen);
					OpenSourceFile (tmp);
					TextView sv = null;

					string localfile = null;
					if (fsmap.SourceMap.TryGetValue (rec.SourceFile, out localfile)) {

						// assuming it is open, scroll to the thing we clicked
						if (sourceviews.TryGetValue (localfile, out sv)) {
							var tm = new TextMark (rec.FullMethodName, true);
							var iter = sv.Buffer.GetIterAtLine (rec.GetLines () [0] - 1);

							sv.Buffer.AddMark (tm, iter);
							sv.ScrollToMark (tm, 0.3, true, 0.2, 0.2);
							sv.ScrollMarkOnscreen (tm);
							sv.Buffer.PlaceCursor (iter);
						}
					}
				}
			}
		}

		protected void OpenCoverageFile (object sender, EventArgs e)
		{
			var fb = new FileChooserDialog ("Load a coverage file", this, FileChooserAction.Open, "Cancel", ResponseType.Cancel, "Open", ResponseType.Accept);
			//fb.Filter = new FileFilter() { Name = "coverage files" };
			//fb.Filter.AddPattern( "*.xcov" );
			fb.Response += (o, args) => fb.Hide ();
			fb.Run ();
			var records = fb.Filename;
			if (!string.IsNullOrEmpty (records) && File.Exists (records)) {
				var dh = new CodeRecordData ();
				dh.Open (records);
				var crs = dh.Load ();
				Load (crs);
			}
		}

		protected void OnRemapAssemblySource (object sender, EventArgs e)
		{
			if (records == null || records.Count == 0)
				return;

			var rd = new RemapSourceFoldersDialog ();

			rd.Response += (o, args) => {
				rd.Hide ();
			};

			var asmlist = (from x in records
			               where true
			               select x.Assembly).Distinct ().ToArray ();
			Dictionary<string, string> oldpaths = new Dictionary<string, string> ();
			foreach (var asm in asmlist) {
				var recs = from x in records
				           where x.Assembly == asm
				           select x;
				var asmrecs = recs.ToArray ();
				var parentpath = fsmap.FindMainFolder (asm, asmrecs);
				oldpaths [asm] = parentpath;
				rd.AddAssembly (asm, parentpath, null);

			}

			var rt = rd.Run ();

			if (rt == (int)(ResponseType.Ok)) {
				Dictionary<string, string> newpaths = new Dictionary<string, string> ();
				foreach (var asm in asmlist) {
					var p = rd.GetPathOfAssembly (asm);
					newpaths [asm] = p;
				}

				foreach (var rec in records) {
					if (newpaths.ContainsKey (rec.Assembly)) {
						var oldp = oldpaths [rec.Assembly];
						var newp = newpaths [rec.Assembly];
						if (!string.IsNullOrEmpty (rec.SourceFile)) {
							var newf = newp + "/" + rec.SourceFile.Substring (oldp.Length); 
							newf = newf.Replace ("//", "/");
							if (File.Exists (newf)) {
								if (newf.Length > 3) {
									rec.SourceFile = newf;
								}
							}
						}
					}
				}
			}
		}

        protected void openGcovSelector (object sender, EventArgs e)
        {
            var fb = new FileChooserDialog ("Select a folder containg a gcov build", this, 
                FileChooserAction.SelectFolder, "Cancel", 
                ResponseType.Cancel, "Select", ResponseType.Accept);
            fb.Response += (o, args) => fb.Hide ();
            fb.Run ();

            var topdir = fb.Filename;
            if (!string.IsNullOrEmpty (topdir) && Directory.Exists (topdir)) {

                var scanner = new GCovReader ();
                scanner.Scan (topdir);
                scanner.ProcessGCovData ();
                Load(scanner.Records);
            }
        }


	}
}
