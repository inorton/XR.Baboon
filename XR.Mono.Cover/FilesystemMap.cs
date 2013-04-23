using System;
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


        public void AddMapping( string origpath, string localpath )
        {
            if ( !SourceMap.ContainsKey(origpath) ){ 
                SourceMap.Add( origpath, localpath );
            }
        }

        public string CommonPath( string origpath, string localpath )
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

