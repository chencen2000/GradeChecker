using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
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
            else if (_args.IsParameterTrue("predict"))
            {
                if (System.IO.File.Exists(_args.Parameters["input"]))
                {
                    msml.predict_main(_args.Parameters);
                }
            }
            else if (_args.IsParameterTrue("prep"))
            {
                string specfn = _args.Parameters.ContainsKey("spec") ? _args.Parameters["spec"] : @"data\classify.xml";
                standard spec = load_spec(specfn);
                Dictionary<string, object> specs = spec.ToDictionary();
                Dictionary<string, object>[] d = prep(_args.Parameters, vdata);
                Dictionary<string, object>[] dd = prepare_for_training(d, specs);
                // save
                try
                {
                    var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                    string s = jss.Serialize(dd);
                    System.IO.File.WriteAllText("samples.json", s);
                }
                catch (Exception) { }
            }
            else if (_args.IsParameterTrue("grade"))
            {
                string specfn = _args.Parameters.ContainsKey("spec") ? _args.Parameters["spec"] : @"data\classify.xml";
                standard spec = load_spec(specfn);
                Dictionary<string, object> specs = spec.ToDictionary();
                Dictionary<string, object>[] samples = prep(_args.Parameters, vdata);
                grade_samples(samples, specs);
            }
            else if (_args.IsParameterTrue("gen"))
            {
                Dictionary<string, object>[] samples = prep(_args.Parameters, vdata);
                string specfn = _args.Parameters.ContainsKey("spec") ? _args.Parameters["spec"] : @"data\classify.xml";
                standard spec = load_spec(specfn);
                Dictionary<string, object> specs = spec.ToDictionary();
                Dictionary<string, object> gened_spec = gen_spec(samples,specs);
                grade_samples(samples, gened_spec);
            }
            else if (_args.IsParameterTrue("testspec"))
            {
                Dictionary<string, object>[] samples = prep(_args.Parameters, vdata);
                string specfn = _args.Parameters.ContainsKey("spec") ? _args.Parameters["spec"] : "";
                if (System.IO.File.Exists(specfn))
                {
                    try
                    {
                        var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                        Dictionary<string, object> specs = jss.Deserialize<Dictionary<string, object>>(System.IO.File.ReadAllText(specfn));
                        grade_samples(samples, specs);
                    }
                    catch (Exception) { }
                }
                //standard spec = load_spec(specfn);
                //Dictionary<string, object> specs = spec.ToDictionary();
                //Dictionary<string, object> gened_spec = gen_spec(samples, specs);
                //grade_samples(samples, gened_spec);

            }
            else if (_args.Parameters.ContainsKey("folder"))
            {
                run_batch_grade(_args.Parameters, vdata);
            }
            else if (_args.Parameters.ContainsKey("file"))
            {
                string spec = _args.Parameters.ContainsKey("spec") ? _args.Parameters["spec"] : @"data\classify.xml";
                string fn = _args.Parameters.ContainsKey("file") ? _args.Parameters["file"] : "";
                bool detail = false;
                if (_args.IsParameterTrue("detail"))
                    detail = true;
                if (System.IO.File.Exists(fn))
                    run_grade(fn, spec,detail, vdata, _args.Parameters);
            }
            else { }
        }
        public static System.Collections.Specialized.StringDictionary[] read_verizon_data()
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
        static Dictionary<string, string> run_grade(string fn, string specfn, bool detail, System.Collections.Specialized.StringDictionary[] vdata, System.Collections.Specialized.StringDictionary _args)
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
                    double t_Area = 0, t_Width = 0, t_Length = 0;
                    if(_args.ContainsKey("Area"))
                    {
                        t_Area = double.Parse(_args["Area"]);
                    }
                    if (_args.ContainsKey("Width"))
                    {
                        t_Width = double.Parse(_args["Width"]);
                    }
                    if (_args.ContainsKey("Length"))
                    {
                        t_Length = double.Parse(_args["Length"]);
                    }
                    flaw f = new flaw(fn,t_Area, t_Width, t_Length);
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
                    if (detail)
                    {
                        string[] keys = spec.get_all_flaw_keys();
                        foreach (string k in keys)
                        {
                            int c = 0;
                            if (f.Counts.ContainsKey(k))
                            {
                                c = f.Counts[k].Item1;
                            }
                            report.Add(k, c.ToString());
                        }
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
                bool detail = false;
                if (args.ContainsKey("detail"))
                    detail = true;
                Dictionary<string, string> res = run_grade(fn, spec, detail, vdata, args);
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
                    string fn = args["output"];
                    if (string.Compare(System.IO.Path.GetExtension(fn), ".json", true) == 0)
                    {
                        System.IO.File.WriteAllText(args["output"], js);
                    }
                    if (string.Compare(System.IO.Path.GetExtension(fn), ".csv", true) == 0)
                    {
                        System.IO.File.WriteAllText("report.json", js);
                        string param = $@"-Command "" & {{ (Get-Content report.json | ConvertFrom-Json)| ConvertTo-Csv | out-file -Encoding default test.csv}}""";
                        int i;
                        runExe("powershell.exe", param, out i, systemCommand: true);

                    }
                }
            }
            catch (Exception) { }

            // summary the report
#if true
            summary_report(report.ToArray());
#endif
        }
        static void summary_report(Dictionary<string,string>[] reports)
        {
            string[] grade_order = new string[] { "A+", "A", "B", "C", "D+", "D" };
            int total = 0;
            int fd_vs_vzw = 0;
            int big_diff = 0;
            int over_graded = 0;
            int under_graded = 0;
            List<Dictionary<string, string>> big_diff_list = new List<Dictionary<string, string>>();
            List<Dictionary<string, string>> over_graded_list = new List<Dictionary<string, string>>();
            List<Dictionary<string, string>> under_graded_list = new List<Dictionary<string, string>>();
            foreach (Dictionary<string, string> d in reports)
            {
                if (d.ContainsKey("VZW"))
                {
                    total++;
                    //if (string.Compare(d["VZW"], d["FD"]) == 0)
                    //    fd_vs_vzw++;
                    int vzw_idx = Array.IndexOf(grade_order, d["VZW"]);
                    int fd_idx = Array.IndexOf(grade_order, d["FD"]);
                    if(vzw_idx==fd_idx) fd_vs_vzw++;
                    if (Math.Abs(vzw_idx - fd_idx) > 1)
                    {
                        big_diff++;
                        big_diff_list.Add(d);
                    }
                    if (vzw_idx > fd_idx)
                    {
                        over_graded++;
                        over_graded_list.Add(d);
                    }
                    if (vzw_idx < fd_idx)
                    {
                        under_graded++;
                        under_graded_list.Add(d);

                    }
                }
            }
            System.Console.WriteLine($"There are {total} devices graded.");
            System.Console.WriteLine($"FD VS. VZW matching rate: {1.0 * fd_vs_vzw / total:P2}");
            System.Console.WriteLine($"FD VS. VZW big diff rate: {1.0 * big_diff / total:P2}");
            System.Console.WriteLine($"FD VS. VZW over graded rate: {1.0 * over_graded / total:P2}");
            System.Console.WriteLine($"FD VS. VZW under graded rate: {1.0 * under_graded / total:P2}");
            // dump
            System.Console.WriteLine($"FD VS. VZW big diff device list: ");
            foreach(Dictionary<string,string>d in big_diff_list)
            {
                StringBuilder sb = new StringBuilder();
                foreach(KeyValuePair<string,string>kvp in d)
                {
                    sb.Append($"{kvp.Key}={kvp.Value},");
                }
                Console.WriteLine(sb.ToString());
            }
            System.Console.WriteLine($"FD VS. VZW over graded device list: ");
            foreach (Dictionary<string, string> d in over_graded_list)
            {
                StringBuilder sb = new StringBuilder();
                foreach (KeyValuePair<string, string> kvp in d)
                {
                    sb.Append($"{kvp.Key}={kvp.Value},");
                }
                Console.WriteLine(sb.ToString());
            }
            System.Console.WriteLine($"FD VS. VZW under graded device list: ");
            foreach (Dictionary<string, string> d in under_graded_list)
            {
                StringBuilder sb = new StringBuilder();
                foreach (KeyValuePair<string, string> kvp in d)
                {
                    sb.Append($"{kvp.Key}={kvp.Value},");
                }
                Console.WriteLine(sb.ToString());
            }
        }
        public static Dictionary<string, object>[] prep(System.Collections.Specialized.StringDictionary args, System.Collections.Specialized.StringDictionary[] vdata)
        {
            string root = args["folder"];
            return prep(root, vdata);
        }
        public static Dictionary<string, object>[] prep(string folder, System.Collections.Specialized.StringDictionary[] vdata)
        {
            List<Dictionary<string, object>> ret = new List<Dictionary<string, object>>();
            Regex r = new Regex(@"classify-(\d{4}).txt");
            //string root = args["folder"];
            string root = folder;
            foreach (string fn in System.IO.Directory.GetFiles(root))
            {
                Match m = r.Match(fn);
                if (m.Success && m.Groups.Count > 1)
                {
                    Dictionary<string, object> report = new Dictionary<string, object>();
                    StringDictionary vd = find_device(vdata, m.Groups[1].Value);
                    System.Console.WriteLine("=======================================");
                    logIt($"Start Grade device: imei={vd?["imei"]}, model={vd?["Model"]}, color={vd?["Color"]}");
                    logIt($"Load device flaws from: {fn}");
                    flaw f = new flaw(fn);
                    f.dump();
                    // save data in report
                    report.Add("imei", vd?["imei"]);
                    report.Add("model", vd?["Model"]);
                    report.Add("color", vd?["Color"]);
                    report.Add("XPO", vd?["XPO"]);
                    report.Add("VZW", vd?["VZW"]);
                    report.Add("OE", f.Grade);
#if false
                    {
                        string[] keys = GradeChecker.Properties.Resources.grade_keys.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string k in keys)
                        {
                            int c = 0;
                            if (f.Counts.ContainsKey(k))
                            {
                                c = f.Counts[k];
                            }
                            report.Add(k, c.ToString());
                        }
                    }
#else
                    foreach(KeyValuePair<string,int> kvp in f.Scores)
                    {
                        report.Add($"Score_{kvp.Key}", kvp.Value);
                    }
                    foreach(KeyValuePair<string,Tuple<int,int>> kvp in f.Counts)
                    {
                        // chris: modify
                        //report.Add(kvp.Key, kvp.Value);
                        report.Add(kvp.Key, kvp.Value.Item1);
                        report.Add($"Score_{kvp.Key}", kvp.Value.Item2);
                    }                    
#endif
                    ret.Add(report);
                }
            }
            // save
            //try
            //{
            //    var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
            //    string s = jss.Serialize(ret);
            //    System.IO.File.WriteAllText("samples.json", s);
            //}
            //catch (Exception) { }
            return ret.ToArray();
        }
        static void test()
        {
            string[] grade_level = new string[] { "A+", "A", "B", "C", "D+", "D" };
            string[] grade_keys = GradeChecker.Properties.Resources.grade_keys.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            string fn = @"C:\projects\local\avia\train_data\report_130.json";
            List<Dictionary<string, object>> roi = new List<Dictionary<string, object>>();
            string sroi = "A+";
            try
            {
                var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                List<Dictionary<string, object>> records = jss.Deserialize<List<Dictionary<string, object>>>(System.IO.File.ReadAllText(fn));
                foreach (Dictionary<string, object> r in records)
                {
                    Dictionary<string, object> d = new Dictionary<string, object>();
                    string s = r["VZW"].ToString();
                    int v = Array.IndexOf(grade_level, s);
                    //d.Add("vzw", v);
                    //if (string.Compare(s, "C") == 0)
                    //    d.Add("vzw", 1);
                    //else
                    //    d.Add("vzw", -1);
                    if (string.Compare(s, sroi) == 0)
                    {
                        d.Add("vzw", s);
                        foreach (string k in grade_keys)
                        {
                            s = r[k].ToString();
                            if (Int32.TryParse(s, out v))
                                d[k] = v;
                        }
                        roi.Add(d);
                    }
                }
            }
            catch (Exception) { }
            try
            {                
                Dictionary<string, object> ns = new Dictionary<string, object>();
                ns.Add("grade", sroi);
                foreach(string k in grade_keys)
                {
                    ns.Add(k, 0);
                }
                foreach(Dictionary<string,object> r in roi)
                {
                    foreach(KeyValuePair<string,object> kvp in r)
                    {
                        if(ns.ContainsKey(kvp.Key) && kvp.Value.GetType()==typeof(int) && ns[kvp.Key].GetType() == typeof(int))
                            ns[kvp.Key] = Math.Max((int)ns[kvp.Key], (int)kvp.Value);
                    }
                }
                var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                string s = jss.Serialize(ns);
                // test
                {

                }
            }
            catch (Exception) { }
        }
        static void test_prep()
        {
            string[] grade_level = new string[] { "A+", "A", "B", "C", "D+", "D" };
            string[] grade_keys = GradeChecker.Properties.Resources.grade_keys.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            string fn = @"C:\projects\local\avia\train_data\report_130.json";
            List<Dictionary<string, object>> ret = new List<Dictionary<string, object>>();
            try
            {
                var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                List<Dictionary<string, object>> records = jss.Deserialize<List<Dictionary<string, object>>>(System.IO.File.ReadAllText(fn));
                foreach(Dictionary<string,object> r in records)
                {
                    Dictionary<string, object> d = new Dictionary<string, object>();
                    string s = r["VZW"].ToString();
                    int v = Array.IndexOf(grade_level, s);
                    //d.Add("vzw", v);
                    if (string.Compare(s, "C") == 0)
                        d.Add("vzw", 1);
                    else
                        d.Add("vzw", -1);
                    foreach (string k in grade_keys)
                    {
                        s = r[k].ToString();
                        if (Int32.TryParse(s, out v))
                            d[k] = v;
                    }
                    ret.Add(d);
                }
            }
            catch (Exception) { }
            try
            {
                var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                string s = jss.Serialize(ret);
                fn = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(fn), "output.json");
                System.IO.File.WriteAllText(fn, s);
            }
            catch (Exception) { }
        }
        public static string[] runExe(string exeFilename, string param, out int exitCode, System.Collections.Specialized.StringDictionary env = null, bool systemCommand = false, int timeout = 180 * 1000)
        {
            List<string> ret = new List<string>();
            exitCode = 1;
            logIt(string.Format("[runExe]: ++ exe={0}, param={1}", exeFilename, param));
            try
            {
                if (System.IO.File.Exists(exeFilename)||systemCommand)
                {
                    System.Threading.AutoResetEvent ev = new System.Threading.AutoResetEvent(false);
                    Process p = new Process();
                    p.StartInfo.FileName = exeFilename;
                    p.StartInfo.Arguments = param;
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.CreateNoWindow = true;
                    if (env != null && env.Count > 0)
                    {
                        foreach (DictionaryEntry de in env)
                        {
                            p.StartInfo.EnvironmentVariables.Add(de.Key as string, de.Value as string);
                        }
                    }
                    p.OutputDataReceived += (obj, args) =>
                    {
                        if (!string.IsNullOrEmpty(args.Data))
                        {
                            logIt(string.Format("[runExe]: {0}", args.Data));
                            ret.Add(args.Data);
                        }
                        if (args.Data == null)
                            ev.Set();
                    };
                    p.Start();
                    p.BeginOutputReadLine();
                    if (p.WaitForExit(timeout))
                    {
                        ev.WaitOne(timeout);
                        if (!p.HasExited)
                        {
                            exitCode = 1460;
                            p.Kill();
                        }
                        else
                            exitCode = p.ExitCode;
                    }
                    else
                    {
                        if (!p.HasExited)
                        {
                            p.Kill();
                        }
                        exitCode = 1460;
                    }
                }
            }
            catch (Exception ex)
            {
                logIt(string.Format("[runExe]: {0}", ex.Message));
                logIt(string.Format("[runExe]: {0}", ex.StackTrace));
            }
            logIt(string.Format("[runExe]: -- ret={0}", exitCode));
            return ret.ToArray();
        }
        static Dictionary<string,object> look_for_spec_by_grade(string grade, Dictionary<string, object>[] spec)
        {
            Dictionary<string, object> ret = null;
            foreach(Dictionary<string,object> d in spec)
            {
                if(d.ContainsKey("grade") && string.Compare(d["grade"].ToString(), grade) == 0)
                {
                    ret = d;
                    break;
                }
            }
            return ret;
        }
        static void score_one_sample(Dictionary<string, object> samples, Dictionary<string, object> specs)
        {
            string[] grade_level = new string[] { "A+", "A", "B", "C", "D+", "D" };
            Dictionary<string, object> grades = new Dictionary<string, object>();
            //string[] grade_keys = GradeChecker.Properties.Resources.grade_keys.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string gl in grade_level)
            {
                int score = 0;
                Dictionary<string, object> report = new Dictionary<string, object>();
                grades.Add(gl, report);
                report.Add("pass", true);
                Dictionary<string, object> spec = (Dictionary<string, object>)specs[gl]; //look_for_spec_by_grade(gl, specs);
                if (spec != null)
                {
                    List<string> grade_keys = new List<string>(spec.Keys);
                    foreach (string k in grade_keys)
                    {
                        int i;
                        if (samples.ContainsKey(k) && Int32.TryParse(samples[k]?.ToString(), out i))
                        {
                            if (spec.ContainsKey(k) && i > (int)spec[k])
                            {
                                report["pass"] = false;
                                report[k] = i;
                                report[$"Spec_{k}"] = (int)spec[k];
                                if (samples.ContainsKey($"Score_{k}"))
                                    report[$"Score_{k}"] = samples[$"Score_{k}"];
                                score -= 3;
                            }
                            else
                            {
                                if(i==0)
                                    score += 3;
                                else
                                    score += 1;
                            }
                        }
                        else
                            score += 3;
                    }
                }
                report.Add("score", score);
            }
            // dump
            {
                foreach(KeyValuePair<string,object> grade in grades)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append($"Grade: {grade.Key}, ");
                    Dictionary<string, object> d = (Dictionary<string, object>) grade.Value;
                    foreach(KeyValuePair<string,object> kvp in d)
                    {
                        sb.Append($"{kvp.Key}={kvp.Value}, ");
                    }
                    Program.logIt(sb.ToString());
                }
            }
        }
        static string grade_one_sample(Dictionary<string, object> samples, Dictionary<string, object> specs)
        {
            string ret = "";
            string[] grade_level = new string[] { "A+", "A", "B", "C", "D+", "D" };
            //string[] grade_keys = GradeChecker.Properties.Resources.grade_keys.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach(string gl in grade_level)
            {
                Dictionary<string, object> spec = (Dictionary<string, object>)specs[gl]; //look_for_spec_by_grade(gl, specs);
                if (spec != null)
                {
                    bool all_pass = true;
                    List<string> grade_keys = new List<string>(spec.Keys);
                    foreach (string k in grade_keys)
                    {
                        int i;
                        if(samples.ContainsKey(k) && Int32.TryParse(samples[k]?.ToString(), out i))
                        {
                            if (spec.ContainsKey(k) && i > (int)spec[k])
                            {
                                all_pass = false;
                                break;
                            }
                        }
                    }
                    if(all_pass)
                    {
                        ret = gl;
                        break;
                    }
                }
            }
            return ret;
        }
        static void grade_samples(Dictionary<string,object>[] samples, Dictionary<string,object> spec)
        {
            List<Dictionary<string, object>> report = new List<Dictionary<string, object>>();
            foreach(Dictionary<string,object> s in samples)
            {
                // test
                score_one_sample(s, spec);

                string g = grade_one_sample(s, spec);
                //Console.WriteLine($"imei={s["imei"]}, VZW={s["VZW"]}, FD={g}");
                if (string.Compare(s["VZW"].ToString(), g) != 0)
                {
                    s["FD"] = g;
                    report.Add(s);
                }

            }
            // summary
            Console.WriteLine($"match rate: {1.0-1.0*report.Count/samples.Length:P2}");
            foreach (Dictionary<string,object> s in report)
            {
                Console.WriteLine($"imei={s["imei"]}, VZW={s["VZW"]}, FD={s["FD"]}");
            }
        }
        static Dictionary<string, object> load_spec_keys()
        {
            Dictionary<string, object> ret = null;
            try
            {
                var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                //string s = System.Text.Encoding.UTF8.GetString(GradeChecker.Properties.Resources.spec_keys);
                ret = jss.Deserialize<Dictionary<string, object>>(GradeChecker.Properties.Resources.spec_keys);
            }
            catch (Exception) { }
            return ret;
        }
        static Dictionary<string, object> gen_spec(Dictionary<string, object>[] samples, Dictionary<string, object> specs)
        {
            //Dictionary<string, object> specs = load_spec_keys();
            // clear specs
            foreach(KeyValuePair<string,object> kvp in specs)
            {
                if (kvp.Value.GetType() == typeof(Dictionary<string, int>))
                {
                    Dictionary<string, object> s = (Dictionary<string, object>)kvp.Value;
                    List<string> keys = new List<string>(s.Keys);
                    foreach(string k in keys)
                    {
                        s[k] = 0;
                    }
                }
            }
            //string[] grade_level = new string[] { "A+", "A", "B", "C", "D+", "D" };
            //string[] grade_keys = GradeChecker.Properties.Resources.grade_keys.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach(Dictionary<string,object> r in samples)
            {
                string s = r["VZW"].ToString();
                if (specs.ContainsKey(s))
                {
                    Dictionary<string, object> spec = (Dictionary<string, object>)specs[s];
                    List<string> keys = new List<string>(spec.Keys);
                    foreach (string k in keys)
                    {
                        if (r.ContainsKey(k))
                        {
                            s = r[k].ToString();
                            int v;
                            if (Int32.TryParse(s, out v))
                            {
                                spec[k] = Math.Max((int)spec[k], v);
                            }
                        }
                    }
                }
            }
            try
            {
                var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                string s = jss.Serialize(specs);
                System.IO.File.WriteAllText("gen_spec.json", s);
            }
            catch (Exception) { }
            return specs;
        }
        static Dictionary<string, object>[] prepare_for_training(Dictionary<string,object>[] samples, Dictionary<string,object> specs)
        {
            string[] grade_order = new string[] { "A+", "A", "B", "C", "D+", "D" };
            List<string> fields = new List<string>();
            fields.Add("VZW");
            foreach(KeyValuePair<string,object> kvp in specs)
            {
                Dictionary<string, object> d = (Dictionary<string, object>)kvp.Value;
                foreach(KeyValuePair<string,object> kvp1 in d)
                {
                    if (!fields.Contains(kvp1.Key))
                    {
                        fields.Add(kvp1.Key);
                    }
                }
            }
            List<Dictionary<string, object>> ret = new List<Dictionary<string, object>>();
            foreach(Dictionary<string,object> r in samples)
            {
                Dictionary<string, object> d = new Dictionary<string, object>();
                foreach (string k in fields)
                {
                    if(string.Compare(k, "VZW", true)==0)
                    {
                        d.Add(k, Array.IndexOf(grade_order, r[k] as string));
                    }
                    else
                        d.Add(k, r.ContainsKey(k) ? r[k] : 0);
                }
                ret.Add(d);
            }
            return ret.ToArray();
        }
    }
}
