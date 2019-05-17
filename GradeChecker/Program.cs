using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
            System.Configuration.Install.InstallContext _args = new System.Configuration.Install.InstallContext(null, args);
            if (_args.IsParameterTrue("debug"))
            {
                System.Console.WriteLine("wait for debug, press any key to continue...");
                System.Console.ReadKey();
            }
            System.Collections.Specialized.StringDictionary[] vdata = read_verizon_data();
            if (_args.Parameters.ContainsKey("folder"))
            {
                run_batch_grade(_args.Parameters, vdata);
            }
            else if (_args.Parameters.ContainsKey("file"))
            {
                string spec = _args.Parameters.ContainsKey("spec") ? _args.Parameters["spec"] : @"data\classify.xml";
                string fn = _args.Parameters.ContainsKey("file") ? _args.Parameters["file"] : "";
                if(System.IO.File.Exists(fn))
                    run_grade(fn, spec, vdata);
            }
            else if (_args.IsParameterTrue("test"))
                test();
        }
        static System.Collections.Specialized.StringDictionary[] read_verizon_data()
        {
            List<System.Collections.Specialized.StringDictionary> vdata = new List<System.Collections.Specialized.StringDictionary>();
            try
            {
                System.IO.StreamReader file = new System.IO.StreamReader(@"data\verizon_data.txt");
                string line = file.ReadLine();
                while (line != null)
                {
                    line = file.ReadLine();
                    if (!string.IsNullOrEmpty(line))
                    {
                        string[] values = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        if (values.Length > 5)
                        {
                            System.Collections.Specialized.StringDictionary sd = new System.Collections.Specialized.StringDictionary();
                            sd.Add("imei", values[0]);
                            sd.Add("Model", values[1]);
                            sd.Add("Color", values[2]);
                            sd.Add("XPO", values[3]);
                            sd.Add("VZW", values[4]);
                            vdata.Add(sd);
                        }
                    }
                }
            }
            catch (Exception) { }
            return vdata.ToArray();
        }
        static System.Collections.Specialized.StringDictionary find_device(System.Collections.Specialized.StringDictionary[] vdata, string last_4_imei)
        {
            System.Collections.Specialized.StringDictionary ret = null;
            foreach(System.Collections.Specialized.StringDictionary sd in vdata)
            {
                if (sd.ContainsKey("imei"))
                {
                    string s = sd["imei"];
                    if(s.Length>4 && string.Compare(last_4_imei, s.Substring(s.Length - 4)) == 0)
                    {
                        ret = sd;
                        break;
                    }
                }
            }
            return ret;
        }
        static standard load_spec(string fn)
        {
            standard ret = null;
            //string fn = @"data\classify.xml";
            if(!System.IO.File.Exists(fn))
            {
                fn = @"data\classify.xml";
            }
            ret = standard.LoadSpec(fn);
            return ret;
        }
        static void run_grade(string fn, string specfn, System.Collections.Specialized.StringDictionary[] vdata)
        {
            Regex r = new Regex(@"classify-(\d{4}).txt");
            //standard spec = standard.LoadSpec(@"data\classify.xml");
            standard spec = load_spec(specfn);
            //string fn = args["file"];
            if (System.IO.File.Exists(fn))
            {
                Match m = r.Match(fn);
                if (m.Success && m.Groups.Count > 1)
                {
                    StringDictionary vd = find_device(vdata, m.Groups[1].Value);
                    System.Console.WriteLine("=======================================");
                    logIt($"Start Grade device: imei={vd?["imei"]}, model={vd?["Model"]}, color={vd?["Color"]}");
                    logIt($"Load device flaws from: {fn}");
                    // load device flaws
                    //flaw f = new flaw(@"data\classify_643.txt");
                    flaw f = new flaw(fn);
                    f.dump();
                    // grade
                    string s = spec.grade(f);
                    System.Console.WriteLine($"Complete Grade: XPO={vd?["XPO"]}, VZW={vd?["VZW"]}, OE={f.Grade}, FD={s}");
                    System.Console.WriteLine("=======================================");
                }
            }
        }
        static void run_batch_grade(System.Collections.Specialized.StringDictionary args, System.Collections.Specialized.StringDictionary[] vdata)
        {
            Regex r = new Regex(@"classify-(\d{4}).txt");
            // load spec
            //standard spec = standard.LoadSpec(@"data\classify.xml");
            string root = args["folder"];
            string spec = args.ContainsKey("spec") ? args["spec"] : @"data\classify.xml";
            foreach (string fn in System.IO.Directory.GetFiles(root))
            {
#if true
                run_grade(fn, spec, vdata);
#else
                Match m = r.Match(fn);
                if (m.Success && m.Groups.Count>1)
                {
                    StringDictionary vd = find_device(vdata, m.Groups[1].Value);
                    System.Console.WriteLine("=======================================");
                    logIt($"Start Grade device: imei={vd?["imei"]}, model={vd?["Model"]}, color={vd?["Color"]}");
                    logIt($"Load device flaws from: {fn}");
                    // load device flaws
                    //flaw f = new flaw(@"data\classify_643.txt");
                    flaw f = new flaw(fn);
                    f.dump();
                    // grade
                    string s = spec.grade(f);
                    System.Console.WriteLine($"Complete Grade: XPO={vd?["XPO"]}, VZW={vd?["VZW"]}, FD={s}");
                    System.Console.WriteLine("=======================================");
                }
#endif
            }
        }
        static void test()
        {

        }
    }
}
