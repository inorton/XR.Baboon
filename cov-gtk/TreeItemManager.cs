using System;
using System.Linq;
using Gtk;
using System.Collections.Generic;
using XR.Mono.Cover;

namespace XR.Baboon
{
	public class TreeItemManager
	{

		public TreeStore TStore = new TreeStore (typeof(CodeRecord));

		public void AddMethods (string type, List<CodeRecord> records)
		{
			NamespaceCodeRecord ns = new NamespaceCodeRecord () { Name = type };
			var total_lines = 0;
			var covered_lines = 0;

			var iter = TStore.AppendValues (ns);
            records.Sort( (a,b) => {
                return a.Name.CompareTo( b.Name );
            } );
			foreach (var r in records) {
				total_lines += r.Lines.Distinct ().Count ();
                covered_lines += r.GetHits();
				TStore.AppendValues (iter, r);
			}
			ns.Coverage = 1.0 * covered_lines / total_lines;
		}
        
        public CodeRecord GetItem( TreePath path, int col )
        {
            TreeIter iter;
            TStore.GetIter( out iter, path );
            return GetItem( iter ); 
        }
        

        public CodeRecord GetItem( TreeIter iter )
        {
            return TStore.GetValue( iter, 0 ) as CodeRecord;
        }

	}
}

