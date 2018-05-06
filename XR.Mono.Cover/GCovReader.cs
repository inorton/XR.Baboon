using System;
using System.IO;
using System.Collections.Generic;

namespace XR.Mono.Cover
{
    /// <summary>
    /// Read gcda and gcno files into a CodeRecordData
    /// </summary>
    public class GCovReader
    {        
        public String BuildRoot {
            get;
            private set;
        }

        // a mapping of known gcno files to their related gcda files.
        Dictionary<String, String> gcfilemaps;

        public List<CodeRecord> Records {
            get; private set;
        }

        public GCovReader ()
        {
            Records = new List<CodeRecord> ();
            gcfilemaps = new Dictionary<string, string>();
        }

        /// <summary>
        /// Scan the specified filepath for gcno and gcda files
        /// </summary>
        /// <param name="filepath">Filepath.</param>
        public void Scan(String filepath) {
            this.ScanForNotes (filepath);
            this.ScanForData ();
        }

        /// <summary>
        /// Scan for gcno files.
        /// </summary>
        /// <param name="filepath">Filepath.</param>
        public void ScanForNotes(String filepath) {
            BuildRoot = Path.GetFullPath(filepath);
            var files = Directory.GetFiles (filepath, "*.gcno", SearchOption.AllDirectories);
            foreach (string filename in files) {
                gcfilemaps.Add (filename, null);
            }
        }

        /// <summary>
        /// Scan filepath for gcda files and try to associate them with
        /// gcno files found with ScanForNotes().
        /// </summary>
        public void ScanForData() {
            // look at all of our known notes and try to find a gcda file
            foreach (string notesfile in new List<string>(gcfilemaps.Keys)) {
                // strip off ".gcno" and replace with ".gcda"
                // TODO cope with gcc + gcov on windows?
                var datafile = notesfile.Substring(0, notesfile.LastIndexOf(".gcno"));
                datafile += ".gcda";
                if (File.Exists (datafile)) {
                    gcfilemaps [notesfile] = datafile;
                }
            }
        }

        /// <summary>
        /// Run gcov on the files we've found
        /// </summary>
        public void ProcessGCovData() {
            var sorted = new List<string>(gcfilemaps.Keys);
            sorted.Sort ();
            foreach (string notesfile in sorted) {
                using (var scratch = new TempDir ()) {
                    var gcov_lines = RunGCov (scratch.TempPath, notesfile, gcfilemaps [notesfile]);
                    var cov_file = GetGCovFile (gcov_lines);
                    if (cov_file != null) {
                        var cov_file_path = Path.Combine (scratch.TempPath, cov_file);
                        if (File.Exists (cov_file_path)) {
                            ParseGCovFile (cov_file_path);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Run gcov in a temp folder and return it's stdout
        /// </summary>
        /// <returns>The G cov.</returns>
        /// <param name="workdir">Workdir.</param>
        /// <param name="notes">Notes.</param>
        /// <param name="datafile">Datafile.</param>
        public List<string> RunGCov(string workdir, string notes, string datafile) {
            var result = new List<String> ();
            var process = new System.Diagnostics.Process ();
            var psi = new System.Diagnostics.ProcessStartInfo ("gcov");
            // grumble, shell quotes grr
            psi.Arguments = String.Format("-l -s -ifm {0} -o {1}", notes, Path.GetDirectoryName(notes));
            psi.WorkingDirectory = workdir;
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            process.StartInfo = psi;
            process.OutputDataReceived += (sender, args) => result.Add(args.Data);
            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit ();
            return new List<string>(result);
        }

        /// <summary>
        /// Gets the Gcov file path from gcov stdout
        /// </summary>
        /// <returns>The G cov file.</returns>
        /// <param name="stdout">Stdout.</param>
        public String GetGCovFile(List<String> stdout){            
            foreach (string line in stdout) {
                if (line != null) {
                    if (line.StartsWith ("Creating ")) {
                        var words = line.Split (new char[] { ' ' }, 2);
                        // last word is a filename
                        if (words.Length == 2) {
                            var filename = words [1].Replace ("'", "");
                            return filename;
                        }
                    }
                }
            }
            return null;
        }

        public void ParseGCovFile(string filepath) {
            CodeRecord record = new CodeRecord ();
            Dictionary<string, string> fileDetails = new Dictionary<string, string> ();
            var lines = File.ReadAllLines (filepath);
            foreach (var line in lines) {
                var parts = line.Split (new char[] { ':' }, 3);
                // counts, line-num, details
                if (parts.Length == 3) {
                    var line_num = parts [1].Trim ();
                    var linenum = ParseInt (line_num);
                    if (linenum == 0) {                        
                        // key:value
                        var details = parts [2].Split (new char[]{ ':' }, 2);
                        fileDetails [details [0]] = details [1];

                    } else {
                        // coverage and line data
                        var hitcount = parts[0].Trim();

                        if (!hitcount.Equals ("-")) {
                            // is an executable line
                            if (hitcount.Contains ("#")) {
                                record.AddLines (new int[] { linenum });
                            } else {
                                var hits = ParseInt (hitcount);

                                if (hits > 0 && linenum > 0) {
                                    record.AddLines (new int[]{ linenum });
                                    record.SetHits (linenum, hits);
                                }
                            }
                        }
                    }
                }
            }

            if (fileDetails.ContainsKey("Source")){
                record.SourceFile = fileDetails ["Source"];

                if (!Path.IsPathRooted (record.SourceFile)) {
                    // relative path, make it absolute according to the notes file path
                    record.SourceFile = Path.Combine(BuildRoot, record.SourceFile);
                }

                // make a relative path for the "assembly/class name"
                var relpath = GetRelativePath (BuildRoot, record.SourceFile);

                record.Assembly = record.SourceFile;
                record.Name = Path.GetFileName (record.SourceFile);
                record.ClassName = Path.GetDirectoryName(relpath);

                record.FullMethodName = relpath;

                // add to the data 
                Records.Add(record);
            }
        }

        static int ParseInt(string text) {
            int num = -1;
            Int32.TryParse (text, out num);
            return num;
        }

        public static string GetRelativePath(String first, String second) {

            if (!first.EndsWith (Path.DirectorySeparatorChar.ToString ()))
                first += Path.DirectorySeparatorChar;

            var tmpuri = new Uri (first);
            var rel = tmpuri.MakeRelativeUri (new Uri(second));
            return rel.ToString ();
        }
    }
}