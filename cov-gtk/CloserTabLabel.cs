using System;
using Gtk;

namespace covgtk
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CloserTabLabel : Gtk.Bin
	{
		public CloserTabLabel ()
		{
			this.Build ();
		}

		public string Text {
			get {
				return this.label.Text;
			}
			set {
				this.label.Text = value;
			}
		}

		public Button Closer {
			get {
				return this.closer;
			}
		}

		public static void InsertTabPage (Notebook book, Widget page, string label)
		{
			var tab = new CloserTabLabel () { Text = label };
			tab.Closer.Pressed += (sender, e) => {
				book.Remove (page); };
			book.InsertPage (page, tab, 0);
		}
	}
}

