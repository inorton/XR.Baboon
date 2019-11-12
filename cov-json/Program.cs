using System;
using System.IO;
using Newtonsoft.Json;
using XR.Mono.Cover;

namespace covjson
{
    class MainClass
    {
        static void Usage()
        {
            Console.Error.WriteLine("Usage: cov-json COVERAGEDB PROJECTNAME");
            Console.Error.WriteLine("cov-json creates and populates a 'json' folder.");
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


                    if (!Directory.Exists("json"))
                        Directory.CreateDirectory("json");

                   // File.WriteAllText( Path.Combine("json","index.json"), index.TransformText() );
		File.WriteAllText( Path.Combine("json","cov.json"), JsonConvert.SerializeObject(index.Records, Formatting.Indented) );
	

                } catch ( Exception ex ) {
                    Console.Error.Write( ex.Message );

                }
            } else {
                Usage();
            }
        }
    }
}
