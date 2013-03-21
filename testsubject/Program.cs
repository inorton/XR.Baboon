using System;

namespace testsubject
{
    class MainClass
    {
        public static void Main (string[] args) 
        {          
            for (int i = 0; i < 12; i++)
                GetCount ();

#if FALSE
            for (int i = 0; i < 22; i++)
                GetCount ();
#endif

        }

        static int count = 0;
        public static int GetCount ()
        {
            Console.WriteLine(count);
            return ++count;
        }
    }
}
