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

	TextTag visitedTag = new TextTag ("visited_green") { Background = "#ccffcc" };

	public void RenderCoverage (string filename, SourceBuffer buf)
	{
		buf.Text = File.ReadAllText (filename);
	}

	public void OpenSourceFile (string filename)
	{
		SourceLanguage language = sourceManager.GetLanguage ("c-sharp");
		var buf = new SourceBuffer (language);

		buf.TagTable.Add (visitedTag);
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
