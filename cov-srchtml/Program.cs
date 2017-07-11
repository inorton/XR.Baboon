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

            var lookup = codeRecords.ToLookup(x => x.SourceFile);

            var srcDir = new DirectoryInfo(args[1]).FullName;

            var dirNames = new HashSet<string>();
            var fileNodes = new List<string>();

            var srcFiles = new DirectoryInfo(srcDir).EnumerateFiles("*.cs", SearchOption.AllDirectories);
            foreach (var srcFileInfo in srcFiles)
            {
                var srcFileName = srcFileInfo.FullName;
                var dirName = srcFileInfo.Directory.FullName;

                if (!srcFileName.StartsWith(srcDir)
                    || srcFileName.EndsWith("/Properties/AssemblyInfo.cs")
                    || dirName.EndsWith("/obj/Debug"))
                {
                    continue;
                }

                var relSrcFile = srcFileName.Substring(srcDir.Length);
                var relDirName = dirName.Substring(srcDir.Length);

                var covFile = outputDir + relSrcFile + ".html";
                var fileRecords = lookup[srcFileName].ToList();
                GenerateCoverageColourisedFile(srcFileName, relSrcFile, covFile, fileRecords);

                if (srcFileName.EndsWith("/ZuoraRestApi.cs") || srcFileName.EndsWith("/ZuoraApi.cs"))
                {
                    Console.WriteLine(srcFileName);
                    foreach (var rec in fileRecords)
                    {
                        Console.WriteLine(rec);
                        Console.WriteLine("lines [" + string.Join(",", rec.GetLines()) + "]");
                        Console.WriteLine("hits  [" + string.Join(",", rec.GetHitCounts()) + "]");
                    }
                    Console.WriteLine("all lines [" + string.Join(",", fileRecords.SelectMany(x => x.GetLines())) + "]");
                    Console.WriteLine();
                }

                var totalLines = fileRecords.SelectMany(x => x.GetLines()).Distinct().Count();
                var coveredLines = fileRecords.SelectMany(x => x.GetHitCounts().Keys).Distinct().Count();
                var covPct = totalLines == 0 ? 0 : 100 * coveredLines / totalLines;
                
                dirNames.Add(relDirName);
                fileNodes.Add(String.Format(
                        "{{'id':'{0}','parent':'{1}','text':'{2} ({3}%)'}}", 
                        relSrcFile, relDirName, srcFileInfo.Name, covPct)
                    .Replace('\'', '"'));
            }

            var dirNodes = new List<string>();
            foreach (var dir in dirNames.ToList())
            {
                string parent = null;
                var path = "";

                foreach (var comp in dir.Split('/').Skip(1))
                {
                    path += $"/{comp}";
                    if (path == dir || dirNames.Add(path))
                    {
                        dirNodes.Add(String.Format(
                                "{{'id':'{0}','parent':'{1}','text':'{2}','state':{{'opened':true}}}}", path, parent ?? "#", comp)
                            .Replace('\'', '"'));
                    }
                    parent = path;
                }
            }

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
<table>
<tr>
<td class='nm-code' style='vertical-align:top'>
<div id='jstree'></div>
</td>
<td style='width:80%;vertical-align:top'>
<iframe id='cov-code' style='width:100%'><p>Code goes here</p></iframe>
</td>
</tr>
</table>
<script src='https://cdnjs.cloudflare.com/ajax/libs/jquery/3.2.1/jquery.min.js'></script>
<script src='https://cdnjs.cloudflare.com/ajax/libs/jstree/3.3.4/jstree.min.js'></script>
<script src='https://cdnjs.cloudflare.com/ajax/libs/iframe-resizer/3.5.14/iframeResizer.min.js'></script>
<script>
  iFrameResize({log:true}, '#cov-code');
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
    $('#cov-code').attr('src', data.selected[0].substring(1) + '.html');
  }
});
</script>
</body>
</html>");
            }

            coverageData.Close();
        }

        private static void GenerateCoverageColourisedFile(string srcFile, string relSrcFile, string outFile,
            IEnumerable<CodeRecord> codeRecords)
        {
            Directory.CreateDirectory(new FileInfo(outFile).DirectoryName);

            using (var reader = new StreamReader(srcFile))
            using (var writer = new StreamWriter(outFile))
            {
                writer.WriteLine("<!DOCTYPE html>");
                writer.WriteLine($"<html>\n<head><title>{relSrcFile}</title></head>");
                writer.WriteLine(SrcHtmlStyle);
                writer.WriteLine("<script src='https://cdnjs.cloudflare.com/ajax/libs/iframe-resizer/3.5.14/iframeResizer.contentWindow.min.js'></script>");
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
                        $"<td class='code{codeClass}'><pre>{escLine}</pre></td></tr>");
                }
                
                writer.WriteLine($"</table>\n</body>");
            }
        }

        static void Usage()
        {
            Console.Error.WriteLine("Usage: cov-html COVERAGEDB SRCDIR OUTPUTDIR");
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