using System;
using System.IO;
using Gtk;
using GtkSourceView;
using covgtk;
using System.Collections.Generic;

public partial class MainWindow: Gtk.Window
{	

	Dictionary<string,SourceView> sourceFiles = new Dictionary<string, SourceView> ();

	SourceLanguageManager sourceManager = new SourceLanguageManager ();

	public const string VisitedOnceBG = "#aaffaa";
	public const string VisitedMoreBG = "#88cc88";


	public void RenderCoverage (string filename, SourceBuffer buf)
	{
		buf.Text = File.ReadAllText (filename);
		buf.ApplyTag ("visited_once", buf.GetIterAtLine (10), buf.GetIterAtLine (14));
		buf.ApplyTag ("visited_more", buf.GetIterAtLine (14), buf.GetIterAtLine (15));
	}

	public void OpenSourceFile (string filename)
	{
		SourceLanguage language = sourceManager.GetLanguage ("c-sharp");
		var buf = new SourceBuffer (language);
		TextTag visitedOnce = new TextTag ("visited_once") { Background = VisitedOnceBG };
		TextTag visitedMore = new TextTag ("visited_more") { Background = VisitedMoreBG };
		buf.TagTable.Add (visitedOnce);
		buf.TagTable.Add (visitedMore);
		buf.HighlightSyntax = true;

		var sv = new SourceView (buf);
		sv.Editable = false;
		sv.ShowLineNumbers = true;
		
		var sw = new ScrolledWindow ();
		
		sw.Add (sv);

		var fname = System.IO.Path.GetFileName (filename);

		CloserTabLabel.InsertTabPage (notebook1, sw, fname);

		RenderCoverage (filename, buf);
	}

	public MainWindow (): base (Gtk.WindowType.Toplevel)
	{
		Build ();

		OpenSourceFile ("../../MainWindow.cs");
		OpenSourceFile ("../../Program.cs");

		this.ShowAll ();

	}
	
	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}
}
