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
            if (_args.IsParameterTrue("test"))
            {
                test();
            }
            else if (_args.Parameters.ContainsKey("folder"))
            {
                run_batch_grade(_args.Parameters, vdata);
            }
            else if (_args.Parameters.ContainsKey("file"))
            {
                string spec = _args.Parameters.ContainsKey("spec") ? _args.Parameters["spec"] : @"data\classify.xml";
                string fn = _args.Parameters.ContainsKey("file") ? _args.Parameters["file"] : "";
                if (System.IO.File.Exists(fn))
                    run_grade(fn, spec, vdata);
            }
            else { }
        }
        static System.Collections.Specialized.StringDictionary[] read_verizon_data()
        {
            List<System.Collections.Specialized.StringDictionary> vdata = new List<System.Collections.Specialized.StringDictionary>();
#if true
            string[] lines = GradeChecker.Properties.Resources.verizon_data.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach(string line in lines)
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
#else
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
#endif
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
        static Dictionary<string, string> run_grade(string fn, string specfn, System.Collections.Specialized.StringDictionary[] vdata)
        {
            Dictionary<string, string> report = new Dictionary<string, string>();
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
                    // save data in report
                    report.Add("imei", vd?["imei"]);
                    report.Add("model", vd?["Model"]);
                    report.Add("color", vd?["Color"]);
                    report.Add("XPO", vd?["XPO"]);
                    report.Add("VZW", vd?["VZW"]);
                    report.Add("OE", f.Grade);
                    report.Add("FD", s);
                    string[] keys = spec.get_all_flaw_keys();
                    foreach(string k in keys)
                    {
                        int c = 0;
                        if (f.Counts.ContainsKey(k))
                        {
                            c = f.Counts[k];
                        }
                        report.Add(k, c.ToString());
                    }
                    try
                    {
                        var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                        string js = jss.Serialize(report);
                        logIt(js);
                    }
                    catch (Exception) { }
                }
            }
            return report;
        }
        static void run_batch_grade(System.Collections.Specialized.StringDictionary args, System.Collections.Specialized.StringDictionary[] vdata)
        {
            Regex r = new Regex(@"classify-(\d{4}).txt");
            List<Dictionary<string, string>> report = new List<Dictionary<string, string>>();
            // load spec
            //standard spec = standard.LoadSpec(@"data\classify.xml");
            string root = args["folder"];
            string spec = args.ContainsKey("spec") ? args["spec"] : @"data\classify.xml";
            foreach (string fn in System.IO.Directory.GetFiles(root))
            {
#if true
                Dictionary<string, string> res = run_grade(fn, spec, vdata);
                report.Add(res);
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
            try
            {
                var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                string js = jss.Serialize(report);
                logIt(js);
                if (args.ContainsKey("output"))
                {
                    System.IO.File.WriteAllText(args["output"], js);
                }
            }
            catch (Exception) { }
        }
        static void test()
        {
            string s = GradeChecker.Properties.Resources.verizon_data;
            string[] lines = s.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            /*
            string fn = @"C:\Tools\avia\report.json";
            try
            {
                var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                var obj = jss.Deserialize<List<Dictionary<string,object>>>(System.IO.File.ReadAllText(fn));
                List<Dictionary<string, object>> all = new List<Dictionary<string, object>>();
                foreach (Dictionary<string,object> d in obj)
                {
                    if (d.ContainsKey("imei") && d["imei"] != null)
                        all.Add(d);
                }
                Program.logIt($"There are total: {all.Count}");
                // check xpo vs vzw
                int xpo_eq_vzw = 0;
                int fd_eq_vzw = 0;
                int fd_eq_xpo = 0;
                int oe_eq_vzw = 0;
                int oe_eq_xpo = 0;
                foreach (Dictionary<string,object> d in all)
                {
                    string xpo = d.ContainsKey("XPO") ? d["XPO"].ToString() : "";
                    string vzw = d.ContainsKey("VZW") ? d["VZW"].ToString() : "";
                    string oe = d.ContainsKey("OE") ? d["OE"].ToString() : "";
                    string fd = d.ContainsKey("FD") ? d["FD"].ToString() : "";
                    if (string.Compare(xpo, vzw) == 0)
                        xpo_eq_vzw++;
                    if (string.Compare(fd, vzw) == 0)
                        fd_eq_vzw++;
                    if (string.Compare(fd, xpo) == 0)
                        fd_eq_xpo++;
                    if (string.Compare(oe, vzw) == 0)
                        oe_eq_vzw++;
                    if (string.Compare(oe, xpo) == 0)
                        oe_eq_xpo++;
                }
                Program.logIt($"XPO vs VZW: {1.0 * xpo_eq_vzw / all.Count:P2}");
                Program.logIt($"FD vs VZW: {1.0 * fd_eq_vzw / all.Count:P2}");
                Program.logIt($"FD vs XPO: {1.0 * fd_eq_xpo / all.Count:P2}");
                Program.logIt($"OE vs VZW: {1.0 * oe_eq_vzw / all.Count:P2}");
                Program.logIt($"OE vs XPO: {1.0 * oe_eq_xpo / all.Count:P2}");
            }
            catch (Exception) { }
            */
        }
    }
}
