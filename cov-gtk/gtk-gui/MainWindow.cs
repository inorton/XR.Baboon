
// This file has been generated by the GUI designer. Do not modify.

public partial class MainWindow
{
	private global::Gtk.Notebook notebook1;
	
	protected virtual void Build ()
	{
		global::Stetic.Gui.Initialize (this);
		// Widget MainWindow
		this.Name = "MainWindow";
		this.Title = global::Mono.Unix.Catalog.GetString ("MainWindow");
		this.WindowPosition = ((global::Gtk.WindowPosition)(4));
		// Container child MainWindow.Gtk.Container+ContainerChild
		this.notebook1 = new global::Gtk.Notebook ();
		this.notebook1.CanFocus = true;
		this.notebook1.Name = "notebook1";
		this.notebook1.CurrentPage = -1;
		this.notebook1.EnablePopup = true;
		this.notebook1.Scrollable = true;
		this.notebook1.BorderWidth = ((uint)(3));
		this.Add (this.notebook1);
		if ((this.Child != null)) {
			this.Child.ShowAll ();
		}
		this.DefaultWidth = 400;
		this.DefaultHeight = 300;
		this.Show ();
		this.DeleteEvent += new global::Gtk.DeleteEventHandler (this.OnDeleteEvent);
	}
}
