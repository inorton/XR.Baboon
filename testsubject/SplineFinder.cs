using System;

namespace testsubject
{
    public class SplineFinder
    {
        public static int CtorCount { get; set; }

        public SplineFinder ()
        {
            CtorCount++;
        }

        public int GetInstanceCount() 
        {
            return CtorCount;
        }

        public void Wait() 
        {
            System.Threading.Thread.Sleep(1000);
        }

        public void Reticulate() 
        {
            for ( double i = 0; i < GetInstanceCount() + 10; i++ ) {
                i = i - 0.1;
                Console.Error.WriteLine( i );
            }
        }
    }
}

