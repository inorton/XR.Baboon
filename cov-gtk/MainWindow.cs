using System;
using System.IO;
using Gtk;
using GtkSourceView;
using System.Collections.Generic;
using XR.Mono.Cover;
using System.Linq;

namespace XR.Baboon
{
	public partial class MainWindow: Gtk.Window
	{	

		SourceLanguageManager sourceManager = new SourceLanguageManager ();
		TreeItemManager treeManager = new TreeItemManager ();

		public const string VisitedOnceBG = "#aaffaa";
		public const string VisitedMoreBG = "#88cc88";


		public void RenderCoverage (SourceBuffer buf, CodeRecord rec)
		{
            foreach ( var hit in rec.LineHits ){
                var hittag = rec.GetHits(hit) == 1 ? "visited_once"  : "visited_more";
                buf.ApplyTag (hittag, buf.GetIterAtLine (hit-1), buf.GetIterAtLine (hit));
            }
		}

        static HashSet<string> openFiles = new HashSet<string>();

        public static void OnCloseSourceFile(string file)
        {
            if ( openFiles.Contains( file ) )
                openFiles.Remove( file );
        }

		public void OpenSourceFile (List<CodeRecord> recs)
		{
            if ( recs.Count == 0 ) return;

			var filename = recs[0].SourceFile;

            if ( openFiles.Contains(filename) ) return;
            openFiles.Add(filename);

			SourceLanguage language = sourceManager.GetLanguage ("c-sharp");
			var buf = new SourceBuffer (language);
			TextTag visitedOnce = new TextTag ("visited_once") { Background = VisitedOnceBG };
			TextTag visitedMore = new TextTag ("visited_more") { Background = VisitedMoreBG };
			buf.TagTable.Add (visitedOnce);
			buf.TagTable.Add (visitedMore);
			buf.HighlightSyntax = true;
            buf.Text = System.IO.File.ReadAllText( filename );

			var page = new SourcePage ();

			var sv = new SourceView (buf);
			// sv.ScrollToIter (buf.GetIterAtLineOffset (22, 0), 1.1, false, 0.0, 0.0);

			sv.Editable = false;
			sv.ShowLineNumbers = true;

			var fp = System.IO.Path.GetFullPath (filename);

			page.Window.Add (sv);
			page.SetHeadingText (fp);
			page.SetSubHeadingText ("");



			var fname = System.IO.Path.GetFileName (filename);

			var tab = CloserTabLabel.InsertTabPage (notebook1, page, fname);
                tab.CloseKeyData = filename;

            page.ShowAll();

            int total_lines = 0;
            int covered_lines = 0;
            buf.Text = File.ReadAllText (filename);
            foreach ( var rec in recs ){
                RenderCoverage (buf, rec);
                total_lines += rec.Lines.Count;
                covered_lines += rec.LineHits.Distinct().Count();
            }

            double cov = (1.0 * covered_lines)/total_lines;

            page.SetCoverage (cov);

            notebook1.Page = notebook1.NPages - 1;
		}

		public MainWindow (): base (Gtk.WindowType.Toplevel)
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
            
			this.ShowAll ();
		}

        List<CodeRecord> records;

		public void Load (List<CodeRecord> code)
		{
            records = code;
            var recs = new Dictionary<string, List<CodeRecord>>();
            foreach ( var rec in code ) {
                if ( rec.Lines.Count == 0 ) continue;
                if ( !recs.ContainsKey( rec.ClassName ) )
                    recs[rec.ClassName] = new List<CodeRecord>();

                recs[rec.ClassName].Add( rec );
            }

            var types = recs.Keys.ToArray();
            Array.Sort( types );

            foreach ( var t in types ) {
                treeManager.AddMethods (t, recs[t]);
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
            var rec = treeManager.GetItem( args.Path, 0 );
            if ( rec != null ){
                if ( rec.SourceFile != null ){
                    var toopen = from r in records where r.SourceFile == rec.SourceFile select r;
                    var tmp = new List<CodeRecord>( toopen );
                    OpenSourceFile( tmp );
                }
            }
        }
	}
}
