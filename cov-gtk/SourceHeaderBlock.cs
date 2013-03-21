using System;

namespace covgtk
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SourceHeaderBlock : Gtk.Bin
	{
		public SourceHeaderBlock ()
		{
			this.Build ();
		}

		public void SetHeading (string txt)
		{
			this.heading.Markup = string.Format ("<span size=\"large\">{0}</span>", txt);
		}
	}
}

