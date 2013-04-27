using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace XR.Mono.Cover
{
    public class FilesystemMap
    {

        public Dictionary<string,string> SourceMap { get; private set; }

        public List<string> SearchPaths { get; private set; }

        public FilesystemMap ()
        {
            SourceMap = new Dictionary<string, string>();
            SearchPaths = new List<string>();
        }

        /// <summary>
        /// Finds the highest level folder of the sources of each assembly
        /// </summary>
        /// <param name="records">Records.</param>
        public string FindMainFolder( string forAssembly, IEnumerable<CodeRecord> records )
        {
            if ( forAssembly == null ) throw new ArgumentNullException( "forAssembly" );
            if ( records == null ) throw new ArgumentNullException( "records" );

            string rv = null;

            var recs = (from r in records where r.Assembly == forAssembly select r).ToArray();
            if ( recs.Length > 0 ) {
                var longest_substring = Path.GetDirectoryName( recs[0].SourceFile );
  
                foreach ( var r in recs ) {
                    if ( !string.IsNullOrEmpty( r.SourceFile ) )
                        longest_substring = CommonStartPath( longest_substring, r.SourceFile );
                }

                if ( !string.IsNullOrEmpty(longest_substring) )
                    rv = longest_substring;
            }

            return rv;
        }

        public void AddMapping( string origpath, string localpath )
        {
            if ( !SourceMap.ContainsKey(origpath) ){ 
                SourceMap.Add( origpath, localpath );
            }
        }

        static string CommonStartPath( string a, string b )
        {
            var pa = a.Split( new char[]{ '/' } );
            var pb = b.Split( new char[]{ '/' } );
            string common = null;

            for ( int i = 0; i < pa.Length && i < pb.Length; i++ ) {
                if ( pa[i] == pb[i] ) {
                    common = string.Join("/", pa.Take(1+i).ToArray() );
                } else {
                    break;
                }
            }

            return common;
        }

        public static string CommonPath( string origpath, string localpath )
        {
            // yes, there are more efficiant ways of doing this
            string common = null;
            for ( int i = localpath.Length-1; i > 0; i-- ) {
                var tmp = localpath.Substring(i);
                if ( origpath.EndsWith( tmp ) ){
                    common = tmp;
                } else {
                    break;
                }
            }
            return common;
        }

    }
}

