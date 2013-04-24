using System;

namespace XR.Baboon
{
    [System.ComponentModel.ToolboxItem(true)]
    public partial class AssemblyItem : Gtk.Bin
    {
        public AssemblyItem ()
        {
            this.Build ();
        }

        public void SetAssembly ( string asm )
        {
            this.assemblyName.Markup = "<b>" + asm + "</b>";
        }

        public void SetOriginalSourceFolder ( string folder )
        {
            if ( !System.IO.Directory.Exists( folder ) ) {
                this.sourceFolderPath.Markup = "<span fgcolor='red'>" + folder + "</span>";
            } else {
                this.sourceFolderPath.Markup = folder;
            }
        }

        public void SetRemappedSourceFolder( string folder )
        {
            this.newFolderPath.SetFilename( folder );
        }
    }
}

