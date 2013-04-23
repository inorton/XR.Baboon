using System;
using System.IO;
using Gtk;
using GtkSourceView;
using System.Collections.Generic;
using XR.Mono.Cover;
using System.Linq;

namespace XR.Baboon
{
    public partial class MainWindow: Gtk.Window
    {	

        SourceLanguageManager sourceManager = new SourceLanguageManager ();
        TreeItemManager treeManager = new TreeItemManager ();

        public const string VisitedOnceBG = "#aaffaa";
        public const string VisitedMoreBG = "#88cc88";


        public void RenderCoverage (SourceBuffer buf, CodeRecord rec)
        {
            foreach (var hit in rec.LineHits) {
                var hittag = rec.GetHits (hit) == 1 ? "visited_once" : "visited_more";
                buf.ApplyTag (hittag, buf.GetIterAtLine (hit - 1), buf.GetIterAtLine (hit));
            }
        }

        static List<string> openFiles = new List<string> ();

        public static void OnCloseSourceFile (string file)
        {
            if (openFiles.Contains (file))
                openFiles.Remove (file);
        }

        Dictionary<string,SourceView> sourceviews = new Dictionary<string, SourceView> ();
        FilesystemMap fsmap = new FilesystemMap();

        public void OpenSourceFile (List<CodeRecord> recs)
        {
            if (recs.Count == 0)
                return;

            var filename = recs [0].SourceFile;
            var origfile = filename;
            var fbname = System.IO.Path.GetFileName(filename);
            if (openFiles.Contains (filename))
                return;

            if ( fsmap.SourceMap.ContainsKey( filename ) ){
                filename = fsmap.SourceMap[filename];
            }

            while (!File.Exists(filename)) {
                var fc = new FileChooserDialog("Locate source file " + origfile.Substring( origfile.Length - 40, 40 ),
                                               this, FileChooserAction.SelectFolder,
                                               "Cancel", ResponseType.Cancel,
                                               "Select", ResponseType.Apply);
                fc.Filter = new Gtk.FileFilter(){ Name = fbname };
                fc.Filter.AddPattern( fbname );

                fc.Response += (o, args) => {
                    Console.Error.WriteLine( fc.Filename );
                    fc.Hide();

                };

                fc.Run();

                if ( fc.Filename != null ){
                    filename = System.IO.Path.Combine( fc.Filename, fbname );
                } else {
                    return;
                }
            }
            fsmap.AddMapping( origfile, filename );


            openFiles.Add (origfile);

            SourceLanguage language = sourceManager.GetLanguage ("c-sharp");
            var buf = new SourceBuffer (language);
            TextTag visitedOnce = new TextTag ("visited_once") { Background = VisitedOnceBG };
            TextTag visitedMore = new TextTag ("visited_more") { Background = VisitedMoreBG };
            buf.TagTable.Add (visitedOnce);
            buf.TagTable.Add (visitedMore);
            buf.HighlightSyntax = true;
            buf.Text = System.IO.File.ReadAllText (filename);

            var page = new SourcePage ();

            var sv = new SourceView (buf);
            // sv.ScrollToIter (buf.GetIterAtLineOffset (22, 0), 1.1, false, 0.0, 0.0);

            sv.Editable = false;
            sv.ShowLineNumbers = true;

            var fp = System.IO.Path.GetFullPath (filename);

            page.Window.Add (sv);
            page.SetHeadingText (fp);
            page.SetSubHeadingText ("");



            var fname = System.IO.Path.GetFileName (filename);

            var tab = CloserTabLabel.InsertTabPage (notebook1, page, fname);
            tab.CloseKeyData = filename;



            page.ShowAll ();

            int total_lines = 0;
            int covered_lines = 0;
            buf.Text = File.ReadAllText (filename);
            foreach (var rec in recs) {
                RenderCoverage (buf, rec);
                total_lines += rec.Lines.Count;
                covered_lines += rec.LineHits.Distinct ().Count ();
            }

            double cov = (1.0 * covered_lines) / total_lines;

            page.SetCoverage (cov);



            notebook1.Page = notebook1.NPages - 1;

            sourceviews [filename] = sv;

        }

        public MainWindow (): base (Gtk.WindowType.Toplevel)
        {
            Build ();

            itemtree.Model = treeManager.TStore;
            var namerender = new CellRendererText ();
            var covrender = new CellRendererText ();
			
            var namecol = new TreeViewColumn ();
            namecol.Title = "Name";
            namecol.PackStart (namerender, true);
            namecol.AddAttribute (namerender, "text", 0);
            namecol.SetCellDataFunc (namerender, CodeRecordCellRenderFuncs.RenderName);
			
			
            var covcol = new TreeViewColumn ();
            covcol.Title = "%";
            covcol.PackStart (covrender, true);
            covcol.AddAttribute (covrender, "text", 1);
            covcol.SetCellDataFunc (covrender, CodeRecordCellRenderFuncs.RenderCoverage);
			
            itemtree.AppendColumn (covcol);
            itemtree.AppendColumn (namecol);

            itemtree.RowActivated += OnItemtreeRowActivated;

            this.ShowAll ();
        }


        List<CodeRecord> records;

        public void Load (List<CodeRecord> code)
        {
            records = code;
            var recs = new Dictionary<string, List<CodeRecord>> ();
            foreach (var rec in code) {
                if (rec.Lines.Count == 0)
                    continue;
                if (!recs.ContainsKey (rec.ClassName))
                    recs [rec.ClassName] = new List<CodeRecord> ();

                recs [rec.ClassName].Add (rec);
            }

            var types = recs.Keys.ToArray ();
            Array.Sort (types);

            foreach (var t in types) {
                treeManager.AddMethods (t, recs [t]);
            }

            itemtree.ExpandAll ();

        }
	
        protected void OnDeleteEvent (object sender, DeleteEventArgs a)
        {
            Application.Quit ();
            a.RetVal = true;
        }

        protected void OnItemtreeRowActivated (object o, RowActivatedArgs args)
        {
            var rec = treeManager.GetItem (args.Path, 0);
            if (rec != null) {
                if (rec.SourceFile != null) {
                    var toopen = from r in records where r.SourceFile == rec.SourceFile select r;
                    var tmp = new List<CodeRecord> (toopen);
                    OpenSourceFile (tmp);
                    SourceView sv = null;

                    string localfile = null;
                    if ( fsmap.SourceMap.TryGetValue( rec.SourceFile, out localfile ) ){

                        // assuming it is open, scroll to the thing we clicked
                        if (sourceviews.TryGetValue (localfile, out sv)) {
                            var tm = new TextMark (rec.FullMethodName, true);
                            var iter = sv.Buffer.GetIterAtLine (rec.Lines [0] - 1);

                            sv.Buffer.AddMark (tm, iter);

                            sv.ScrollToMark (tm, 0.3, true, 0.2, 0.2);
                            sv.Buffer.PlaceCursor (iter);
                        }
                    }
                }
            }
        }
        protected void OpenCoverageFile(object sender, EventArgs e)
        {
            var fb = new FileChooserDialog( "Load a coverage file", this, FileChooserAction.Open, "Cancel", ResponseType.Cancel, "Open", ResponseType.Accept );
            //fb.Filter = new FileFilter() { Name = "coverage files" };
            //fb.Filter.AddPattern( "*.xcov" );
            fb.Response += (o, args) => fb.Hide();
            fb.Run();
            var records = fb.Filename;
            if ( !string.IsNullOrEmpty(records) && File.Exists(records) ) 
            {
                var dh = new CodeRecordData();
                dh.Open( records );
                var crs = dh.Load();
                Load( crs );
            }
        }

    }
}
