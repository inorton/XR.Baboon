using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using XR.Mono.Cover;

namespace covsrchtml
{
    internal class Program
    {
        /// <summary>
        /// usage: cov-srchtml.exe COVERAGEDB SRCDIR OUTPUTDIR
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            if (args.Length != 3 || !File.Exists(args[0]) || !Directory.Exists(args[1]))
            {
                Usage();
                Environment.Exit(1);
            }

            var outputDir = args[2];
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            var coverageData = new CodeRecordData();
            coverageData.Open(args[0]);
            var codeRecords = coverageData.Load();

            var dirNames = new HashSet<string>();
            var fileNodes = new List<string>();

            GenerateCoverageColourisedSources(
                new DirectoryInfo(args[1]).FullName, 
                outputDir, 
                codeRecords.ToLookup(x => x.SourceFile), 
                dirNames, 
                fileNodes);
            
            GenerateSourceTree(outputDir, CreateDirNodeJson(dirNames), fileNodes);
            GenerateIndex(outputDir);
            
            coverageData.Close();
        }

        private static void GenerateIndex(string outputDir)
        {
            using (var writer = new StreamWriter(outputDir + "/index.html"))
            {
                writer.WriteLine(
                    @"<!DOCTYPE html>
<html>
<head>
<meta charset='utf-8'>
<title>Coverage Report</title>
</head>
<frameset cols='20%,80%'>
<frame src='tree.html' name='treeFrame' title='Source files'></frame>
<frame name='sourceFrame'></frame>
</frameset>
</html>
");
            }
        }

        private static void GenerateSourceTree(string outputDir, IEnumerable<string> dirNodes, IEnumerable<string> fileNodes)
        {
            using (var writer = new StreamWriter(outputDir + "/tree.html"))
            {
                writer.WriteLine(
                    @"<!DOCTYPE html>
<html>
<head>
<meta charset='utf-8'>
<link rel='stylesheet' href='https://cdnjs.cloudflare.com/ajax/libs/jstree/3.3.4/themes/default/style.min.css' />
<style>.nm-code{font-family:sans-serif;font-size:80%;}</style>
</head>
<body>
<div class='nm-code' style='vertical-align:top' id='jstree'></div>
<script src='https://cdnjs.cloudflare.com/ajax/libs/jquery/3.2.1/jquery.min.js'></script>
<script src='https://cdnjs.cloudflare.com/ajax/libs/jstree/3.3.4/jstree.min.js'></script>
<script>
  $(function () {
    $('#jstree').jstree({ 'core' : {
      'multiple': false,
      'animation': 0,
      'data' : [
"
                    + string.Join(",\n", dirNodes) + ",\n"
                    + string.Join(",\n", fileNodes) + @"
      ]
    }});
  });
$('#jstree').on('changed.jstree', function (e, data) {
  console.log(data.selected);
  if (data.selected[0].endsWith('.cs')) {
    parent.sourceFrame.location.href=data.selected[0].substring(1) + '.html';
  }
});
</script>
</body>
</html>");
            }
        }

        private static IEnumerable<string> CreateDirNodeJson(ISet<string> dirs)
        {
            var nodes = new List<string>();
            foreach (var dir in dirs.ToList())
            {
                string parent = null;
                var path = "";

                foreach (var comp in dir.Split('/').Skip(1))
                {
                    path += $"/{comp}";
                    if (path == dir || dirs.Add(path))
                    {
                        nodes.Add(String.Format(
                                "{{'id':'{0}','parent':'{1}','text':'{2}','state':{{'opened':true}}}}", path, parent ?? "#",
                                comp)
                            .Replace('\'', '"'));
                    }
                    parent = path;
                }
            }
            return nodes;
        }

        private static void GenerateCoverageColourisedSources(
            string srcDir, 
            string outputDir, 
            ILookup<string, CodeRecord> codeRecordLookup, 
            ISet<string> dirNames,
            ICollection<string> fileNodes)
        {
            var srcFiles = new DirectoryInfo(srcDir).EnumerateFiles("*.cs", SearchOption.AllDirectories);
            foreach (var srcFileInfo in srcFiles)
            {
                var srcFileName = srcFileInfo.FullName;
                var dirName = srcFileInfo.Directory.FullName;

                if (!srcFileName.StartsWith(srcDir)
                    || srcFileName.EndsWith("/Properties/AssemblyInfo.cs")
                    || dirName.Contains("/obj/"))
                {
                    continue;
                }

                var relSrcFile = srcFileName.Substring(srcDir.Length);
                var relDirName = dirName.Substring(srcDir.Length);

                var covFile = outputDir + relSrcFile + ".html";
                var fileRecords = codeRecordLookup[srcFileName].ToList();
                GenerateCoverageColourisedFile(srcFileName, relSrcFile, covFile, fileRecords);

                var totalLines = fileRecords.SelectMany(x => x.GetLines()).Distinct().Count();
                var coveredLines = fileRecords.SelectMany(x => x.GetHitCounts().Keys).Distinct().Count();
                var covPct = totalLines == 0 ? 0 : 100 * coveredLines / totalLines;

                dirNames.Add(relDirName);
                fileNodes.Add(String.Format(
                        "{{'id':'{0}','parent':'{1}','text':'{2} ({3}%)'}}",
                        relSrcFile, relDirName, srcFileInfo.Name, covPct)
                    .Replace('\'', '"'));
            }
        }

        private static void GenerateCoverageColourisedFile(
            string srcFile, 
            string relSrcFile, 
            string outFile,
            IEnumerable<CodeRecord> codeRecords)
        {
            Directory.CreateDirectory(new FileInfo(outFile).DirectoryName);

            using (var reader = new StreamReader(srcFile))
            using (var writer = new StreamWriter(outFile))
            {
                writer.WriteLine("<!DOCTYPE html>");
                writer.WriteLine($"<html>\n<head><title>{relSrcFile}</title></head>");
                writer.WriteLine(SrcHtmlStyle);
                writer.WriteLine("<body>\n<table cellspacing='0' cellpadding='0'>");
                
                var n = 0;
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    n++;
                    var lineRecs = codeRecords.Where(rec => rec.GetLines().Contains(n));
                    var hits = -1;
                    var goodbad = "";
                    if (lineRecs.Any())
                    {
                        hits = lineRecs.Sum(r => r.GetHits(n));
                        goodbad = hits == 0 ? "bad" : "good";
                    }
                    var hitsClass = hits == -1 ? "" : " hit-count-" + goodbad;
                    var codeClass = hits == -1 ? "" : " nm-cov-" + goodbad;
                    var hitsTxt = hits == -1 ? "&nbsp;" : $"{hits}";
                    
                    var escLine = HttpUtility.HtmlEncode(line);
                    writer.WriteLine(
                        $"<tr><td class='num'>{n}&nbsp;</td>" +
                        $"<td class='num{hitsClass}'>{hitsTxt}&nbsp;</td>" +
                        $"<td><pre><span class='code{codeClass}'>{escLine}</span></pre></td></tr>");
                }
                
                writer.WriteLine($"</table>\n</body>");
            }
        }

        static void Usage()
        {
            Console.Error.WriteLine("Usage: cov-srchtml COVERAGEDB SRCDIR OUTPUTDIR");
        }
        
        private const string SrcHtmlStyle = @"
<style>
.num {
  text-align:right;
  padding-right:3px;
  font-size:80%;
}
.hit-count-good {
  background-color:#73b973;
}
.hit-count-bad {
  background-color:#ff7373;
}
.code {
  font-family:monospace;
}
.nm-cov-good {
  background-color:#a2d0a2;
}
.nm-cov-bad {
  background-color:#ffa2a2;
}
pre {
  margin:0px;
}
body {
  font-family:sans-serif;
}
</style>";
    }
}