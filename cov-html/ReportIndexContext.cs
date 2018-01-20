using System;
using System.Linq;
using System.Collections.Generic;
using XR.Mono.Cover;

namespace covhtml 
{

    public partial class ReportIndex 
    {
        public string ProjectName { get; set; }

        public List<CodeRecord> Records { get; set; }

        public void LoadCoverage( string db ) {
            var data = new CodeRecordData();
            data.Open( db );
            Records = data.Load();
            metadata = data.LoadMeta();
        }

        Dictionary<string,string> metadata;

        public IEnumerable<string> GetTypes()
        {
            string[] classes = (from x in Records select x.ClassName).Distinct().ToArray();
            Array.Sort( classes );
            return classes;
        }

        public IEnumerable<CodeRecord> GetMembers( string typename )
        {
            var recs = (from x in Records where x.ClassName == typename select x).ToArray();
            Array.Sort( recs, (a,b) => {
                return a.Coverage.CompareTo(b.Coverage);
            } );
            return recs;
        }

        public int GetTypeCoverage( string type )
        {
            int lines = 0;
            int hits = 0;
            foreach ( var x in GetMembers( type ) ){
                lines += x.GetLines().Length;
                hits += x.GetHits();
            }
            return lines == 0 ? 0 : (int)Math.Round((100.0 * hits/lines));
        }

        public int GetCoverage( CodeRecord rec )
        {
            if ( rec.GetLines().Length == 0 ) return -1;
            return (int)Math.Round( 100*rec.Coverage );
        }

        public void LineMatchCount( string regex, out int lines, out int hits ) 
        {
            lines = 0;
            hits = 0;
            var rx = new System.Text.RegularExpressions.Regex( regex );
            foreach ( var c in Records ) {
                if ( rx.IsMatch( c.ClassName ) ){
                    lines += c.GetLines().Length;
                    hits += c.GetHits();
                }
            }
        }

    }
}
