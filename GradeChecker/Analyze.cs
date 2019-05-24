using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace GradeChecker
{
    class Analyze
    {
        static string[] grade_level = new string[] { "A+", "A", "B", "C", "D+", "D" };
        static void Main(string[] args)
        {
            /*
            standard spec = standard.LoadSpec(@"C:\tools\avia\classify_0523.xml");
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(classify));
                Stream fs = new FileStream("classify_text.xml", FileMode.Create);
                using (XmlWriter writer = new XmlTextWriter(fs, Encoding.Unicode))
                {
                    serializer.Serialize(writer, standard.TheClassify);
                }
            }
            catch (Exception) { }
            */
            ana_A_B();
        }

        static void ana_A_B()
        {
            string sample_folder = @"C:\tools\avia\records";
            standard spec = standard.LoadSpec(@"C:\tools\avia\classify_0523_mod.xml");
            System.Collections.Specialized.StringDictionary[] vdata = Program.read_verizon_data();
            Dictionary<string, object>[] all_data = prep(sample_folder, vdata);
            var selected_data = from i in all_data
                           where string.Compare(i["VZW"].ToString(), "A") == 0  // || string.Compare(i["VZW"].ToString(), "A+") == 0
                             select i;
            /*
            int match = 0;
            int over = 0;
            int under = 0;
            foreach(Dictionary<string,object> d in selected_data)
            {
                string fd = grade_one_sample(d, standard.TheClassify);
                int v = Array.IndexOf(grade_level, d["VZW"] as string);
                int f = Array.IndexOf(grade_level, fd);
                if (v == f) match++;
                else if (v > f) over++;
                else under++;
            }
            Program.logIt($"Tested {selected_data.Count()} devices, matching rate={1.0 * match / selected_data.Count():P2}");
            Program.logIt($"OverGrading: {1.0 * over / selected_data.Count():P2}");
            Program.logIt($"UnderGrading: {1.0 * under / selected_data.Count():P2}");
            */
            //query.Cast<MyEntityType>().ToArray();
            grade_samples(selected_data.Cast<Dictionary<string,object>>().ToArray(), standard.TheClassify);
        }
        static string grade_one_sample(Dictionary<string, object> sample, classify spec)
        {
            string ret = "";
            Program.logIt($"======================> Start grading for device {sample["imei"]} {sample["model"]} {sample["color"]}");
            Program.logIt(sample["dump"].ToString());
            foreach(string g in grade_level)
            {
                Program.logIt($"checking grading for {g}");
                grade_item gi = standard.get_grade_item_by_grade(spec, g);
                if (gi.max_flaws > 0)
                {
                    if (sample.ContainsKey("all-all-all"))
                    {
                        if((int)sample["all-all-all"]>gi.max_flaws)
                        {
                            Program.logIt($"Fail to grading for {g}, due to all-all-all={sample["all-all-all"]} > {gi.max_flaws}");
                            continue;
                        }
                    }
                }
                if (gi.max_major_flaws > 0)
                {
                    if (sample.ContainsKey("all-major-all"))
                    {
                        if ((int)sample["all-major-all"] > gi.max_flaws)
                        {
                            Program.logIt($"Fail to grading for {g}, due to all-major-all={sample["all-major-all"]} > {gi.max_major_flaws}");
                            continue;
                        }
                    }
                }
                if (gi.max_region_flaws> 0)
                {
                    if (sample.ContainsKey("AA-region-all"))
                    {
                        if ((int)sample["AA-region-all"] > gi.max_flaws)
                        {
                            Program.logIt($"Fail to grading for {g}, due to AA-region-all={sample["AA-region-all"]} > {gi.max_region_flaws}");
                            continue;
                        }
                    }
                }
                foreach(surface_item si in gi.surface)
                {
                    Program.logIt($"check surface {si.surface} for {g}");
                    if (si.max_flaws > 0)
                    {
                        if (sample.ContainsKey($"{si.surface}-all-all"))
                        {
                            if ((int)sample[$"{si.surface}-all-all"] > si.max_flaws)
                            {
                                Program.logIt($"Fail to grading for {g}, due to {si.surface}-all-all={sample[$"{si.surface}-all-all"]} > {si.max_flaws}");
                                break;
                            }
                        }
                    }
                    if (si.max_major_flaws > 0)
                    {
                        if (sample.ContainsKey($"{si.surface}-major-all"))
                        {
                            if ((int)sample[$"{si.surface}-major-all"] > si.max_major_flaws)
                            {
                                Program.logIt($"Fail to grading for {g}, due to {si.surface}-major-all={sample[$"{si.surface}-major-all"]} > {si.max_major_flaws}");
                                break;
                            }
                        }
                    }
                    if (si.max_region_flaws> 0)
                    {
                        if (sample.ContainsKey($"{si.surface}-region-all"))
                        {
                            if ((int)sample[$"{si.surface}-region-all"] > si.max_region_flaws)
                            {
                                Program.logIt($"Fail to grading for {g}, due to {si.surface}-region-all={sample[$"{si.surface}-region-all"]} > {si.max_region_flaws}");
                                break;
                            }
                        }
                    }
                    bool all_meet = true;
                    foreach(flaw_allow_item ff in si.flaw_allow)
                    {
                        if (sample.ContainsKey(ff.flaw))
                        {
                            if((int)sample[ff.flaw]> ff.allow)
                            {
                                //
                                Program.logIt($"Fail to grading for {g}, due to {si.surface} {ff.flaw}={sample[ff.flaw]} > {ff.allow}");
                                all_meet = false;
                            }
                        }
                    }
                    if (all_meet)
                    {
                        ret = g;
                    }
                    break;
                }
                if (!string.IsNullOrEmpty(ret))
                    break;
            }
            if (string.IsNullOrEmpty(ret))
                ret = "D";
            Program.logIt($"======================> Complete grading for device {sample["imei"]}, VZW={sample["VZW"]}, FD={ret}");
            return ret;
        }
        static void grade_samples(Dictionary<string, object>[] samples, classify spec)
        {
            List<Dictionary<string, object>> report = new List<Dictionary<string, object>>();
            foreach (Dictionary<string, object> s in samples)
            {
                string g = grade_one_sample(s, spec);
                //Console.WriteLine($"imei={s["imei"]}, VZW={s["VZW"]}, FD={g}");
                if (string.Compare(s["VZW"].ToString(), g) != 0)
                {
                    s["FD"] = g;
                    report.Add(s);
                }
            }
            // summary
            Program.logIt($"match rate: {1.0 - 1.0 * report.Count / samples.Length:P2} (total: {samples.Length})");
            foreach (Dictionary<string, object> s in report)
            {
                Program.logIt($"imei={s["imei"]}, VZW={s["VZW"]}, FD={s["FD"]}");
            }
        }
        static Dictionary<string, object>[] prep(string folder, System.Collections.Specialized.StringDictionary[] vdata)
        {
            List<Dictionary<string, object>> ret = new List<Dictionary<string, object>>();
            System.Text.RegularExpressions.Regex r = new Regex(@"classify-(\d{4}).txt");
            string root = folder;
            foreach (string fn in System.IO.Directory.GetFiles(root))
            {
                Match m = r.Match(fn);
                if (m.Success && m.Groups.Count > 1)
                {
                    Dictionary<string, object> report = new Dictionary<string, object>();
                    StringDictionary vd = find_device(vdata, m.Groups[1].Value);
                    System.Console.WriteLine("=======================================");
                    Program.logIt($"Prep device data: imei={vd?["imei"]}, model={vd?["Model"]}, color={vd?["Color"]}");
                    Program.logIt($"Load device flaws from: {fn}");
                    flaw f = new flaw(fn);
                    string sdump = f.dump();
                    // save data in report
                    report.Add("imei", vd?["imei"]);
                    report.Add("model", vd?["Model"]);
                    report.Add("color", vd?["Color"]);
                    report.Add("XPO", vd?["XPO"]);
                    report.Add("VZW", vd?["VZW"]);
                    report.Add("OE", f.Grade);
                    report.Add("dump", sdump);
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
                    foreach (KeyValuePair<string, int> kvp in f.Counts)
                    {
                        report.Add(kvp.Key, kvp.Value);
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
        static System.Collections.Specialized.StringDictionary find_device(System.Collections.Specialized.StringDictionary[] vdata, string last_4_imei)
        {
            System.Collections.Specialized.StringDictionary ret = null;
            foreach (System.Collections.Specialized.StringDictionary sd in vdata)
            {
                if (sd.ContainsKey("imei"))
                {
                    string s = sd["imei"];
                    if (s.Length > 4 && string.Compare(last_4_imei, s.Substring(s.Length - 4)) == 0)
                    {
                        ret = sd;
                        break;
                    }
                }
            }
            return ret;
        }

    }
}
