
// This file has been generated by the GUI designer. Do not modify.
namespace XR.Baboon
{
	public partial class MainWindow
	{
		private global::Gtk.UIManager UIManager;
		
		private global::Gtk.Action openAction;
		
		private global::Gtk.Action remapSourcePaths;
		
		private global::Gtk.VBox vbox1;
		
		private global::Gtk.Toolbar toolbar1;
		
		private global::Gtk.HPaned hpaned1;
		
		private global::Gtk.ScrolledWindow GtkScrolledWindow;
		
		private global::Gtk.TreeView itemtree;
		
		private global::Gtk.Notebook notebook1;

		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget XR.Baboon.MainWindow
			this.UIManager = new global::Gtk.UIManager ();
			global::Gtk.ActionGroup w1 = new global::Gtk.ActionGroup ("Default");
			this.openAction = new global::Gtk.Action ("openAction", null, null, "gtk-open");
			w1.Add (this.openAction, null);
			this.remapSourcePaths = new global::Gtk.Action ("remapSourcePaths", global::Mono.Unix.Catalog.GetString ("Set Source Paths"), null, "gtk-preferences");
			this.remapSourcePaths.ShortLabel = global::Mono.Unix.Catalog.GetString ("Set Source Paths");
			w1.Add (this.remapSourcePaths, null);
			this.UIManager.InsertActionGroup (w1, 0);
			this.AddAccelGroup (this.UIManager.AccelGroup);
			this.Name = "XR.Baboon.MainWindow";
			this.Title = global::Mono.Unix.Catalog.GetString ("XR Baboon!");
			this.WindowPosition = ((global::Gtk.WindowPosition)(4));
			this.DefaultWidth = 700;
			this.DefaultHeight = 440;
			// Container child XR.Baboon.MainWindow.Gtk.Container+ContainerChild
			this.vbox1 = new global::Gtk.VBox ();
			this.vbox1.Name = "vbox1";
			this.vbox1.Spacing = 6;
			// Container child vbox1.Gtk.Box+BoxChild
			this.UIManager.AddUiFromString ("<ui><toolbar name='toolbar1'><toolitem name='openAction' action='openAction'/><toolitem name='remapSourcePaths' action='remapSourcePaths'/></toolbar></ui>");
			this.toolbar1 = ((global::Gtk.Toolbar)(this.UIManager.GetWidget ("/toolbar1")));
			this.toolbar1.Name = "toolbar1";
			this.toolbar1.ShowArrow = false;
			this.vbox1.Add (this.toolbar1);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.toolbar1]));
			w2.Position = 0;
			w2.Expand = false;
			w2.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.hpaned1 = new global::Gtk.HPaned ();
			this.hpaned1.CanFocus = true;
			this.hpaned1.Name = "hpaned1";
			this.hpaned1.Position = 240;
			// Container child hpaned1.Gtk.Paned+PanedChild
			this.GtkScrolledWindow = new global::Gtk.ScrolledWindow ();
			this.GtkScrolledWindow.Name = "GtkScrolledWindow";
			this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
			this.itemtree = new global::Gtk.TreeView ();
			this.itemtree.CanFocus = true;
			this.itemtree.Name = "itemtree";
			this.itemtree.EnableSearch = false;
			this.itemtree.HeadersVisible = false;
			this.GtkScrolledWindow.Add (this.itemtree);
			this.hpaned1.Add (this.GtkScrolledWindow);
			global::Gtk.Paned.PanedChild w4 = ((global::Gtk.Paned.PanedChild)(this.hpaned1 [this.GtkScrolledWindow]));
			w4.Resize = false;
			// Container child hpaned1.Gtk.Paned+PanedChild
			this.notebook1 = new global::Gtk.Notebook ();
			this.notebook1.CanFocus = true;
			this.notebook1.Name = "notebook1";
			this.notebook1.CurrentPage = -1;
			this.notebook1.Scrollable = true;
			this.notebook1.BorderWidth = ((uint)(3));
			this.hpaned1.Add (this.notebook1);
			this.vbox1.Add (this.hpaned1);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.hpaned1]));
			w6.Position = 1;
			this.Add (this.vbox1);
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.Show ();
			this.DeleteEvent += new global::Gtk.DeleteEventHandler (this.OnDeleteEvent);
			this.openAction.Activated += new global::System.EventHandler (this.OpenCoverageFile);
			this.remapSourcePaths.Activated += new global::System.EventHandler (this.OnRemapAssemblySource);
		}
	}
}
