using System;
using System.Linq;
using System.Collections.Generic;

namespace XR.Baboon
{
    public partial class RemapSourceFoldersDialog : Gtk.Dialog
    {
        public RemapSourceFoldersDialog ()
        {
            this.Build ();
        }

        List<AssemblyItem> assemblies = new List<AssemblyItem>();

        public void AddAssembly( string asmname, string origPath, string mappedPath )
        {
            if ( origPath == null ) throw new ArgumentNullException("origPath");
            var item = new AssemblyItem();
            assemblies.Add(item);
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

        public string GetPathOfAssembly( string assembly )
        {
            var asm = from x in assemblies where x.AssemblyName == assembly select x.NewFolder;
            return asm.FirstOrDefault();
        }
    }
}

