using System;

namespace XR.Baboon
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
            this.heading.Text = txt;
		}

		public void SetSubHeading (string txt)
		{
			this.subheading.Text = txt;
		}

		public void SetCoverage (double frac)
		{
			this.percentage.Fraction = frac;
			this.percentage.Text = string.Format ("{0:00.0}%", frac * 100);
		}
	}
}

