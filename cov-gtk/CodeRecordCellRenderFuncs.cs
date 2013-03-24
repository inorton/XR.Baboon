using System;
using Gtk;
using XR.Mono.Cover;

namespace XR.Baboon
{
	public static class CodeRecordCellRenderFuncs
	{
		const string colorLessCoverage = "red";
		const string colorSomeCoverage = "orange";
		const string colorGoodCoverage = "darkgreen";


		public static void RenderName (TreeViewColumn col, CellRenderer cell, TreeModel model, TreeIter iter)
		{
			var txt = cell as CellRendererText;
			CodeRecord rc = (CodeRecord)model.GetValue (iter, 0);
			if (txt != null) {
				txt.Text = rc.Name;
				txt.Foreground = CellColor (rc.Coverage);

			}
		}

		public static string CellColor (double cov)
		{
			if (cov < 0.35) {
				return colorLessCoverage;
			} else {
				if (cov > 0.9) {
					return colorGoodCoverage;
				} else {
					return colorSomeCoverage;
				}
			}
		}

		public static void RenderCoverage (TreeViewColumn col, CellRenderer cell, TreeModel model, TreeIter iter)
		{
			var txt = cell as CellRendererText;
			CodeRecord rc = (CodeRecord)model.GetValue (iter, 0);
			if (txt != null) {
				txt.Foreground = CellColor (rc.Coverage);
				txt.Text = String.Format ("{0:00.0}%", rc.Coverage * 100.0);
			}
		}
	}
}

