using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace GradeChecker
{
    class Program
    {
        public static void logIt(string msg)
        {
            System.Diagnostics.Trace.WriteLine($"[Grading]: {msg}");
            System.Console.WriteLine(msg);
        }
        static void Main(string[] args)
        {
            test();
        }

        static void test()
        {
            // load spec
            standard spec = standard.LoadSpec(@"data\classify.xml");
            // load device flaws
            flaw f = new flaw(@"data\classify_643.txt");
            f.dump();
            // grade
            spec.grade(f);
        }
    }
}
