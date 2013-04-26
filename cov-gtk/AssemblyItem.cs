using System;

namespace XR.Baboon
{
    [System.ComponentModel.ToolboxItem(true)]
    public partial class AssemblyItem : Gtk.Bin
    {
        public AssemblyItem ()
        {
            this.Build ();
            this.newFolderPath.FileSet += (sender, e) => {
                NewFolder = newFolderPath.Filename;
            };
        }

        public void SetAssembly ( string asm )
        {
            AssemblyName = asm;
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

        public string AssemblyName { get; private set; }

        public string NewFolder { get; private set; }
    }
}

