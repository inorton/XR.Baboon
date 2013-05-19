using System;
using System.IO;

using XR.Mono.Cover;

namespace covhtml
{
    class MainClass
    {
        static void Usage()
        {
            Console.Error.WriteLine("Usage: cov-html COVERAGEDB PROJECTNAME");
            Console.Error.WriteLine("cov-html creates and populates a 'html' folder.");
            Environment.Exit(1);
        }

        public static void Main (string[] args)
        {
            // expect first arg to be a cov db
            if ( args.Length == 2 && File.Exists( args[0] ) ){
                try {

                    var index = new ReportIndex();
                    index.ProjectName = args[1];
                    index.LoadCoverage( args[0] );


                    if (!Directory.Exists("html"))
                        Directory.CreateDirectory("html");

                    File.WriteAllText( Path.Combine("html","index.html"), index.TransformText() );

                } catch ( Exception ex ) {
                    Console.Error.Write( ex.Message );

                }
            } else {
                Usage();
            }
        }
    }
}
