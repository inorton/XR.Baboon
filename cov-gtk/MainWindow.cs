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

		Dictionary<string,SourceView> sourceFiles = new Dictionary<string, SourceView> ();

		SourceLanguageManager sourceManager = new SourceLanguageManager ();
		TreeItemManager treeManager = new TreeItemManager ();

		public const string VisitedOnceBG = "#aaffaa";
		public const string VisitedMoreBG = "#88cc88";


		public void RenderCoverage (string filename, SourceBuffer buf, CodeRecord rec)
		{
			buf.Text = File.ReadAllText (filename);

            foreach ( var hit in rec.LineHits ){
                var hittag = rec.GetHits(hit) == 1 ? "visited_once"  : "visited_more";
                buf.ApplyTag (hittag, buf.GetIterAtLine (hit), buf.GetIterAtLine (hit+1));
                
            }
		}

		public void OpenSourceFile (CodeRecord rec)
		{
			var filename = rec.SourceFile;
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

			page.SetCoverage (rec.Coverage);

			var fname = System.IO.Path.GetFileName (filename);

			CloserTabLabel.InsertTabPage (notebook1, page, fname);

            page.ShowAll();

			RenderCoverage (filename, buf, rec);
            
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

		public void Load (List<CodeRecord> code)
		{

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
                    OpenSourceFile( rec );
                }
            }
        }
	}
}
