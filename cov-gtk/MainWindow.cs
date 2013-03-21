using System;
using Gtk;
using GtkSourceView;
using covgtk;

public partial class MainWindow: Gtk.Window
{	
	public MainWindow (): base (Gtk.WindowType.Toplevel)
	{
		Build ();

		var manager = new SourceLanguageManager ();
		SourceLanguage language = manager.GetLanguage ("c-sharp");

		var buf = new SourceBuffer (language);

		var tag = new TextTag ("green_bg");
		tag.Background = "#00ff00";

		buf.TagTable.Add (tag);

		buf.HighlightSyntax = true;

		var sv = new SourceView (buf);
		sv.Editable = false;
		sv.ShowLineNumbers = true;

		var sw = new ScrolledWindow ();

		buf.Text = @" 

using System;

public class Fooo {

  public static void Run() {
    Console.WriteLine(""go!"");
  }

}

";



		var iter1 = buf.GetIterAtLineOffset (8, 0);
		var iter2 = buf.GetIterAtLineOffset (9, 0);
		buf.ApplyTag (tag, iter1, iter2);

		sw.Add (sv);


		CloserTabLabel.InsertTabPage (notebook1, sw, "foo.cs");

		this.ShowAll ();

	}
	
	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}
}
