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

			foreach (var r in records) {
				total_lines += r.Lines.Distinct ().Count ();
				covered_lines += r.LineHits.Distinct ().Count ();
				TStore.AppendValues (iter, r);
			}
			ns.Coverage = 1.0 * covered_lines / total_lines;

		}

	}
}

