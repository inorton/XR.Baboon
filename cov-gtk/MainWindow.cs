using System;
using System.IO;
using Gtk;
using GtkSourceView;
using covgtk;
using System.Collections.Generic;
using XR.Mono.Cover;

namespace XR.Baboon
{
	public partial class MainWindow: Gtk.Window
	{	

		Dictionary<string,SourceView> sourceFiles = new Dictionary<string, SourceView> ();

		SourceLanguageManager sourceManager = new SourceLanguageManager ();
		TreeItemManager treeManager = new TreeItemManager ();

		public const string VisitedOnceBG = "#aaffaa";
		public const string VisitedMoreBG = "#88cc88";


		public void RenderCoverage (string filename, SourceBuffer buf)
		{
			buf.Text = File.ReadAllText (filename);
			buf.ApplyTag ("visited_once", buf.GetIterAtLine (10), buf.GetIterAtLine (14));
			buf.ApplyTag ("visited_more", buf.GetIterAtLine (14), buf.GetIterAtLine (15));

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

			var page = new SourcePage ();

			var sv = new SourceView (buf);
			// sv.ScrollToIter (buf.GetIterAtLineOffset (22, 0), 1.1, false, 0.0, 0.0);

			sv.Editable = false;
			sv.ShowLineNumbers = true;

			var fp = System.IO.Path.GetFullPath (filename);

			page.Window.Add (sv);
			page.SetHeadingText (fp);
			page.SetSubHeadingText ("bllaaaa");

			page.SetCoverage (0.33);

			var fname = System.IO.Path.GetFileName (filename);

			CloserTabLabel.InsertTabPage (notebook1, page, fname);

			RenderCoverage (filename, buf);
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
			
			itemtree.AppendColumn (namecol);
			
			var covcol = new TreeViewColumn ();
			covcol.Title = "%";
			covcol.PackStart (covrender, true);
			covcol.AddAttribute (covrender, "text", 1);
			covcol.SetCellDataFunc (covrender, CodeRecordCellRenderFuncs.RenderCoverage);
			
			itemtree.AppendColumn (covcol);

			this.ShowAll ();
		}

		public void Load (List<CodeRecord> code)
		{
			


			var x = new CodeRecord () { Name = "Test", ClassName = "Foo.Bar.Baz" };
			x.Lines.Add (1);
			x.Lines.Add (2);
			x.LineHits.Add (2);
			var list = new List<CodeRecord> { x };
			treeManager.AddMethods ("Foo.Bar.Baz", list);
			itemtree.ExpandAll ();

			//OpenSourceFile ("../../MainWindow.cs");
			//OpenSourceFile ("../../Program.cs");

		}
	
		protected void OnDeleteEvent (object sender, DeleteEventArgs a)
		{
			Application.Quit ();
			a.RetVal = true;
		}
	}
}
