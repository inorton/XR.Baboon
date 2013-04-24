using System;

namespace XR.Baboon
{
    public partial class RemapSourceFoldersDialog : Gtk.Dialog
    {
        public RemapSourceFoldersDialog ()
        {
            this.Build ();
        }

        public void AddAssembly( string asmname, string origPath, string mappedPath )
        {
            if ( origPath == null ) throw new ArgumentNullException("origPath");
            var item = new AssemblyItem();
            item.SetAssembly( asmname );
            item.SetOriginalSourceFolder( origPath );


            if ( System.IO.Directory.Exists( mappedPath ) ) {
                item.SetRemappedSourceFolder( mappedPath );
            } else {
                item.SetRemappedSourceFolder( "/" );
            }

            this.assemblyList.PackStart( item );
            this.assemblyList.ShowAll();
        }
    }
}

