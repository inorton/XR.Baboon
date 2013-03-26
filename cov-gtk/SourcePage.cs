using System;
using Gtk;

namespace XR.Baboon
{
    [System.ComponentModel.ToolboxItem(true)]
    public partial class SourcePage : Gtk.Bin
    {
        public SourcePage ()
        {
            this.Build ();
            if (this.header == null)
                throw new NullReferenceException ("header");
        }

        public ScrolledWindow Window {
            get {
                return scroller;
            }
        }

        public void SetHeadingText (string txt)
        {
            header.SetHeading (txt);
        }

        public void SetSubHeadingText (string txt)
        {
            header.SetSubHeading (txt);
        }

        public void SetCoverage (double frac)
        {
            header.SetCoverage (frac);
        }
    }
}

