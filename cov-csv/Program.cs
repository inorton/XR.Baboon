using System;
using System.Linq;
using System.IO;
using XR.Mono.Cover;

namespace covcsv
{
    class MainClass
    {
        static void Usage()
        {
            Console.Error.WriteLine("Usage: cov-csv COVERAGEDB PROJECTNAME");
            Console.Error.WriteLine("cov-csv creates and populates a 'csv' folder.");
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


                    if (!Directory.Exists("csv"))
                        Directory.CreateDirectory("csv");

                   // File.WriteAllText( Path.Combine("csv","index.csv"), index.TransformText() );
		File.WriteAllText( Path.Combine("csv","cov.csv"), 
			string.Join(",","ClassName","Name","CallCount","Saved","Coverage")+Environment.NewLine+
			string.Join(Environment.NewLine,index.Records.Select ( r => string.Join(",",r.ClassName,r.Name,r.CallCount,r.Saved,r.Coverage))) +
		Environment.NewLine );
	

                } catch ( Exception ex ) {
                    Console.Error.Write( ex.Message );

                }
            } else {
                Usage();
            }
        }

    }
}
