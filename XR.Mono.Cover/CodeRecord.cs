using System;
using Mono.Debugger.Soft;
using System.Collections.Generic;
using System.Linq;

namespace XR.Mono.Cover
{

    public class CodeRecord
    {
        public List<int> Lines { get; set; }

        public string SourceFile { get; set; }
        public string Assembly { get; set; }
        public string ClassName { get; set; }
        public string Name { get; set; }
        public string FullMethodName { get; set; }
        public int CallCount { get; set; }

        public CodeRecord ()
        {
            Lines = new List<int> ();
        }

        Dictionary<int,int> hitCounts = new Dictionary<int, int>();

        public void Hit (int line)
        { 
            if ( !hitCounts.ContainsKey( line ) )
                hitCounts[line] = 0;
            hitCounts[line]++;
        }

        public void SetHits( int line, int hitcount )
        {
            if ( hitcount > 0 ){
                hitCounts[line] = hitcount;
            } else {
                if ( hitCounts.ContainsKey(line) ) hitCounts.Remove(line);
            }
        }

        public int GetHits (int line)
        {
            if ( hitCounts.ContainsKey( line ) )
                return hitCounts[line];
            return 0;
        }

        public int GetHits() 
        {
            return hitCounts.Count;
        }

        public virtual double Coverage {
            get {
                return (hitCounts.Count * 1.0) / Lines.Distinct ().Count ();
            }
            set {}
        }

        public override string ToString ()
        {
 return String.Format ("{0}:{1},Calls={2},Coverage={3:00.0}%,{4}", ClassName, Name, CallCount, 100 * Coverage, FullMethodName);
        }

    }

}
