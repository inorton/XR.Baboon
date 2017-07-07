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
            var metadata = coverageData.LoadMeta();

            var lookup = codeRecords.ToLookup(x => x.SourceFile);

            var srcDir = new DirectoryInfo(args[1]).FullName;
            var srcFiles = new DirectoryInfo(srcDir).EnumerateFiles("*.cs", SearchOption.AllDirectories);
            foreach (var srcFile in srcFiles)
            {
                if (!srcFile.FullName.StartsWith(srcDir)
                    || srcFile.FullName.EndsWith("/Properties/AssemblyInfo.cs")
                    || srcFile.Directory.FullName.EndsWith("/obj/Debug"))
                {
                    continue;
                }
                var relSrcFile = srcFile.FullName.Substring(srcDir.Length);
                var covFile = outputDir + relSrcFile + ".html";

                GenerateCoverageColourisedFile(srcFile.FullName, relSrcFile, covFile, lookup[srcFile.FullName].ToList());
            }

            coverageData.Close();
        }

        private static void GenerateCoverageColourisedFile(string srcFile, string relSrcFile, string outFile,
            IEnumerable<CodeRecord> codeRecords)
        {
//            Console.WriteLine($"{srcFile} -> {outFile}");

            Directory.CreateDirectory(new FileInfo(outFile).DirectoryName);
            
            using (var reader = new StreamReader(srcFile))
            using (var writer = new StreamWriter(outFile))
            {
                var n = 1;
                
                writer.WriteLine($"<html>\n<head><title>{relSrcFile}</title></head>");
                writer.WriteLine("<style>.nm-code{font-family:monospace} .nm-cov-good{background-color:green} .nm-cov-not-good{}</style>");
                writer.WriteLine("<body class='nm-code'>\n<table>");
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var c = codeRecords.Sum(x => x.GetHits(n));
                    var escLine = HttpUtility.HtmlEncode(line);
                    // 
                    var covClass = c > 0 ? "nm-cov-good" : "nm-cov-not-good";
                    writer.WriteLine($"<tr><td>{n}</td><td>{c}</td><td class='{covClass}'><pre>{escLine}</pre></td></tr>");
                    n++;
                }
                writer.WriteLine($"</table>\n</body>");
            }
        }

        static void Usage()
        {
            Console.Error.WriteLine("Usage: cov-html COVERAGEDB SRCDIR OUTPUTDIR");
        }
    }
}