using System;
using Gtk;

namespace XR.Baboon
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

        public string CloseKeyData { get; set; }

        public static CloserTabLabel InsertTabPage (Notebook book, Widget page, string label)
        {
            var tab = new CloserTabLabel () { Text = label };
            tab.Closer.Pressed += (sender, e) => {
                MainWindow.OnCloseSourceFile (tab.CloseKeyData);
                book.Remove (page); };
            book.InsertPage (page, tab, book.NPages);
            tab.ShowAll ();

            return tab;
        }
    }
}

