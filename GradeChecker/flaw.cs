using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace GradeChecker
{
    class flaw
    {
        /// <summary>
        /// the flaws in ClassifyLog
        /// </summary>
        List<System.Collections.Generic.Dictionary<string, string>> _flaws = new List<Dictionary<string, string>>();
        /// <summary>
        /// the counts in ClassifyLog
        /// </summary>
        System.Collections.Generic.Dictionary<string, Tuple<int,int>> _counts = new Dictionary<string, Tuple<int, int>>();
        System.Collections.Generic.Dictionary<string, int> _zones = new Dictionary<string, int>();
        System.Collections.Generic.Dictionary<string, int> _scores = new Dictionary<string, int>();
        XmlDocument _score_doc = null;
        string _grade = "";
        double m_Area, m_Width, m_Length;
        public flaw(string filename, double f_Area = 0, double f_Width = 0, double f_Length = 0)
        {
            m_Area = f_Area;
            m_Width = f_Width;
            m_Length = f_Length;
            _score_doc = new XmlDocument();
            _score_doc.Load(System.IO.Path.Combine(Program.Root, "score.xml"));
            parse(filename);
        }

        public Dictionary<string, Tuple<int, int>> Counts { get => _counts; }
        public List<Dictionary<string, string>> Flaws { get => _flaws; /*set => _flaws = value;*/ }
        public string Grade { get => _grade; /*set => _grade = value;*/ }
        public Dictionary<string, int> Zones { get => _zones; /*set => _zones = value;*/ }
        public Dictionary<string, int> Scores { get => _scores; /*set => _scores = value;*/ }

        static void Main(string[] args)
        {
            test_1();
        }
        static void test_1()
        {
            standard spec = standard.LoadSpec(@"C:\Tools\avia\classify.xml");
            Dictionary<string, object> specs = spec.ToDictionary();
            //flaw f = new flaw(@"C:\Tools\avia\test\classify-0028.txt");
            //f.dump();
            List<Dictionary<string, int>> db = new List<Dictionary<string, int>>();
            string folder = @"C:\Tools\avia\ClassifyLog";
            foreach(string fn in System.IO.Directory.GetFiles(folder))
            {
                flaw f = new flaw(fn);
                f.dump();
                db.Add(f.Scores);
            }
            List<string> keys = new List<string>();
            Console.WriteLine($"dump score:");
            foreach (Dictionary<string,int> r in db)
            {
                foreach (var item in r)
                {
                    Console.WriteLine($"{item.Key}={item.Value}");
                    if (!keys.Contains(item.Key))
                        keys.Add(item.Key);
                }
            }

        }
        static void test()
        {

        }
        void parse(string filename)
        {
            Regex r = new Regex(@"^.*flaw = .*surface =.*sort =.*$");
            string section_name = "";
            try
            {
                string[] lines = System.IO.File.ReadAllLines(filename);
                foreach(string line in lines)
                {
                    if(string.Compare(line, "Flaws:") == 0)
                    {
                        section_name = "Flaws";
                    }
                    else if (string.Compare(line, "Count:") == 0)
                    {
                        section_name = "Count";
                    }
                    else if (string.Compare(line, "AA Surface:") == 0)
                    {
                        section_name = "AA Surface";
                    }
                    else if (string.Compare(line, "A Surface:") == 0)
                    {
                        section_name = "A Surface";
                    }
                    else if (string.Compare(line, "B Surface:") == 0)
                    {
                        section_name = "B Surface";
                    }
                    else if (string.Compare(line, "C Surface:") == 0)
                    {
                        section_name = "C Surface";
                    }
                    else if (line.StartsWith("Grade ="))
                    {
                        //section_name = "C Surface";
                        section_name = "";
                        _grade = line.Substring("Grade =".Length + 1).Trim();
                    }
                    else
                    {
                        if (string.Compare(section_name, "Flaws") == 0)
                        {
                            Program.m_Result.m_Flaws.Add(line);
                            Dictionary<string, string> f = new Dictionary<string, string>();
                            // parse flaws
                            string[] kvs = line.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string kv in kvs)
                            {
                                int pos = kv.IndexOf('=');
                                if (pos > 0 && pos < kv.Length - 1)
                                {
                                    string k = kv.Substring(0, pos).Trim();
                                    string v = kv.Substring(pos + 1).Trim();
                                    f.Add(k, v);
                                }
                            }
                            if(f.Count>0)
                                _flaws.Add(f);
                        }
                        else if (string.Compare(section_name, "Count") == 0)
                        {
                            // parse count Nick-A-Minor = 1
                            Program.m_Result.m_Counts.Add(line);
                            int pos = line.IndexOf('=');
                            if(pos>0 && pos + 1 < line.Length)
                            {
                                string k = line.Substring(0, pos).Trim();
                                string v = line.Substring(pos+1).Trim();
                                int i;
                                ///?Why is Tuple<int, int> rather than Tuple<int>
                                if (Int32.TryParse(v, out i))
                                    _counts.Add(k, new Tuple<int, int>(i, 0));
                            }
                        }
                        else if (string.Compare(section_name, "AA Surface") == 0)
                        {
                            // parse Zone2 = 1
                            Program.m_Result.m_AASurface.Add(line);
                            int pos = line.IndexOf('=');
                            if (pos > 0 && pos + 1 < line.Length)
                            {
                                string k = line.Substring(0, pos).Trim();
                                if (k.StartsWith("Zone"))
                                {
                                    string v = line.Substring(pos + 1).Trim();
                                    int i;
                                    if (Int32.TryParse(v, out i))
                                        _zones.Add(k, i);
                                }
                                else if (k.StartsWith("Totoal number on AA"))
                                {
                                    string v = line.Substring(pos + 1).Trim();
                                    int i;
                                    if (Int32.TryParse(v, out i))
                                        _counts.Add("AA-all-all", new Tuple<int, int>(i, 0));
                                }
                                else if (k.StartsWith("Totoal number of major on AA"))
                                {
                                    string v = line.Substring(pos + 1).Trim();
                                    int i;
                                    if (Int32.TryParse(v, out i))
                                        _counts.Add("AA-major-all", new Tuple<int, int>(i, 0));
                                }
                            }
                        }
                        else if (string.Compare(section_name, "A Surface") == 0)
                        {
                            // 
                            Program.m_Result.m_ASurface.Add(line);
                            int pos = line.IndexOf('=');
                            if (pos > 0 && pos + 1 < line.Length)
                            {
                                string k = line.Substring(0, pos).Trim();
                                if (k.StartsWith("Totoal number on A"))
                                {
                                    string v = line.Substring(pos + 1).Trim();
                                    int i;
                                    if (Int32.TryParse(v, out i))
                                        _counts.Add("A-all-all", new Tuple<int, int>(i, 0));
                                }
                                else if (k.StartsWith("Totoal number of major on A"))
                                {
                                    string v = line.Substring(pos + 1).Trim();
                                    int i;
                                    if (Int32.TryParse(v, out i))
                                        _counts.Add("A-major-all", new Tuple<int, int>(i, 0));
                                }
                            }
                        }
                        else if (string.Compare(section_name, "B Surface") == 0)
                        {
                            Program.m_Result.m_BSurface.Add(line);
                            // 
                            int pos = line.IndexOf('=');
                            if (pos > 0 && pos + 1 < line.Length)
                            {
                                string k = line.Substring(0, pos).Trim();
                                if (k.StartsWith("Totoal number on B"))
                                {
                                    string v = line.Substring(pos + 1).Trim();
                                    int i;
                                    if (Int32.TryParse(v, out i))
                                        _counts.Add("B-all-all", new Tuple<int, int>(i, 0));
                                }
                                else if (k.StartsWith("Totoal number of major on B"))
                                {
                                    string v = line.Substring(pos + 1).Trim();
                                    int i;
                                    if (Int32.TryParse(v, out i))
                                        _counts.Add("B-major-all", new Tuple<int, int>(i, 0));
                                }
                            }
                        }
                        else if (string.Compare(section_name, "C Surface") == 0)
                        {
                            Program.m_Result.m_CSurface.Add(line);
                            // 
                            int pos = line.IndexOf('=');
                            if (pos > 0 && pos + 1 < line.Length)
                            {
                                string k = line.Substring(0, pos).Trim();
                                if (k.StartsWith("Totoal number on C"))
                                {
                                    string v = line.Substring(pos + 1).Trim();
                                    int i;
                                    if (Int32.TryParse(v, out i))
                                        _counts.Add("C-all-all", new Tuple<int, int>(i, 0));
                                }
                                else if (k.StartsWith("Totoal number of major on C"))
                                {
                                    string v = line.Substring(pos + 1).Trim();
                                    int i;
                                    if (Int32.TryParse(v, out i))
                                        _counts.Add("C-major-all", new Tuple<int, int>(i, 0));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception) { }
            _counts.Add("AA-region-all", new Tuple<int, int>(_zones.Count, 0));
            System.Collections.Generic.Dictionary<string, Tuple<int,int>> Ttmep = new Dictionary<string, Tuple<int, int>>(_counts);
            // recount
            if (m_Area == 0 && m_Length == 0 && m_Width == 0)
            {
             
            }
            else
            {
                recount();
            }
            _counts.Add("all-all-all", count_total_flaws());
            // score the flaws
            score();
        }
        public int count_total_flaws_by_grade(XmlNode gNode)
        {
            int ret = 0;
            //ret = count_total_flaws();
            string t_GradeLevel = gNode["name"]?.InnerText;
            classify spec = standard.TheClassify;
            //get defect name on specific surface
            grade_item gi = standard.get_grade_item_by_grade(spec, t_GradeLevel);
            foreach(surface_item s in gi.surface)
            {
                foreach(flaw_allow_item f in s.flaw_allow)
                {
                    if (_counts.ContainsKey(f.flaw))
                    {
                        ret += _counts[f.flaw].Item1;
                    }
                }
            }
            return ret;
        }
        public Tuple<int,int> count_total_flaws()
        {
            int ret = 0;
            double score = 0.0;
            foreach (Dictionary<string, string> d in _flaws)
            {
                // ignore sort=Discoloration
                if (d.ContainsKey("sort") && string.Compare(d["sort"], "Discoloration") == 0)
                {
                    // ignore
                    continue;
                }
#if true
                // ignore area < 0.1mm
                if (d.ContainsKey("area"))
                {
                    double a;
                    if(double.TryParse(d["area"].Split(new char[] { ' '})[0], out a))
                    {
                        //if (a < 0.1)
                        //{
                        //    // ignore
                        //    continue;
                        //}
                        score += a;
                    }
                }
#endif
                ret++;
            }
            score *= 1000;
            return new Tuple<int, int>(ret, (int)score);
        }
        public int count_total_flaws_by_surface(string g, string surface)
        {
            int ret = 0;
            classify spec = standard.TheClassify;
            surface_item si = standard.get_surface_item_by_grade_surface(spec, g, surface);
            foreach (flaw_allow_item f in si.flaw_allow)
            {
                if (_counts.ContainsKey(f.flaw))
                {
                    ret += _counts[f.flaw].Item1;
                }
            }
            return ret;
        }
        public int count_total_flaws_by_surface(XmlNode surfaceNode)
        {
            int ret = 0;
            foreach(XmlNode n in surfaceNode["flaw_allow"]?.ChildNodes)
            {
                string k = n["flaw"]?.InnerText;
                if (_counts.ContainsKey(k))
                {
                    ret += _counts[k].Item1;
                }
            }
            return ret;
        }
        public int count_total_flaws_by_surface(string surface)
        {
            int ret = 0;
            foreach (var f in _flaws)
            {
                if (f.ContainsKey("surface") && string.Compare(f["surface"], surface) == 0)
                {
                    // ignore sort=Discoloration
                    if (f.ContainsKey("sort") && string.Compare(f["sort"], "Discoloration") == 0)
                    {
                        // ignore
                        continue;
                    }

#if true
                    // ignore area < 0.1mm
                    if (f.ContainsKey("area"))
                    {
                        double a;
                        if (double.TryParse(f["area"].Split(new char[] { ' ' })[0], out a))
                        {
                            if (a < 0.1)
                            {
                                // ignore
                                continue;
                            }
                        }
                    }
#endif
                    ret++;
                }
            }
            return ret;
        }
        public string dump()
        {
            StringBuilder sb = new StringBuilder();
            //Program.logIt($"Device Grade: ({_grade})");
            sb.AppendLine($"Device Grade: ({_grade})");
            //Program.logIt($"Dump device flaws: ({_flaws.Count})");
            sb.AppendLine($"Dump device flaws: ({_flaws.Count})");
            foreach (Dictionary<string,string> d in _flaws)
            {
                //Program.logIt(string.Join(",", d.Select(kv => kv.Key + "=" + kv.Value).ToArray()));
                sb.AppendLine(string.Join(",", d.Select(kv => kv.Key + "=" + kv.Value).ToArray()));
            }
            //Program.logIt($"Dump device counts: ({_counts.Count})");
            sb.AppendLine($"Dump device counts: ({_counts.Count})");
            //Program.logIt(string.Join(System.Environment.NewLine, _counts.Select(kv => kv.Key + "=" + kv.Value).ToArray()));
            foreach (KeyValuePair<string,Tuple<int,int>> kvp in _counts)
            {
                if (kvp.Value.Item1 > 0)
                {
                    //Program.logIt($"{kvp.Key}=={kvp.Value}");
                    sb.AppendLine($"{kvp.Key}=={kvp.Value.Item1}({kvp.Value.Item2})");
                }
            }
            // score
            sb.AppendLine($"Dump device score: ({_scores.Count})");
            sb.AppendLine(string.Join(Environment.NewLine, _scores.Select(kv => kv.Key + "=" + kv.Value).ToArray()));

            //Program.logIt($"Dump device AA Zone: ({_zones.Count})");
            //Program.logIt(string.Join(System.Environment.NewLine, _zones.Select(kv => kv.Key + "=" + kv.Value).ToArray()));
            sb.AppendLine($"Dump device AA Zone: ({_zones.Count})");
            sb.AppendLine(string.Join(System.Environment.NewLine, _zones.Select(kv => kv.Key + "=" + kv.Value).ToArray()));
            // score

            Program.logIt(sb.ToString());
            return sb.ToString();
        }
        System.Collections.Generic.Dictionary<string, string>[] get_flaws_by_surface_sort(string surface, string sort)
        {
            List<System.Collections.Generic.Dictionary<string, string>> ret = new List<Dictionary<string, string>>();
            foreach(Dictionary<string,string> d in _flaws)
            {
                if (string.Compare(d["surface"], surface) == 0 && string.Compare(d["sort"], sort) == 0)
                {
                    if (d["surface"].CompareTo("AA") == 0 || d["surface"].CompareTo("A") == 0)
                    {
                        if (d["sort"].CompareTo("Scratch") == 0)
                        {
                            double Length = double.Parse(d["length"].Replace("m", " ").TrimEnd('_'));
                            double Width = double.Parse(d["width"].Replace("m", " ").TrimEnd('_'));
                            double Area = double.Parse(d["area"].Replace("m", " ").TrimEnd('_'));
                            if (Area > m_Area && Width > m_Width && Length > m_Length)
                            {
                                ret.Add(d);
                            }
                        }
                        else
                        {
                            ret.Add(d);
                        }
                    }
                    else
                    {
                        ret.Add(d);
                    }
                }
            }
            return ret.ToArray();
        }
        Tuple<double,double,double> get_data_from_flaw(Dictionary<string, string> flaw)
        {
            double l = 0;
            double w = 0;
            double a = 0;
            if(flaw.ContainsKey("length"))
            {
                double.TryParse(flaw["length"].Split(new char[] { ' ' })[0], out l);
            }
            if (flaw.ContainsKey("width"))
            {
                double.TryParse(flaw["width"].Split(new char[] { ' ' })[0], out w);
            }
            if (flaw.ContainsKey("area"))
            {
                double.TryParse(flaw["area"].Split(new char[] { ' ' })[0], out a);
            }
            return new Tuple<double, double, double>(l, w, a);
        }
        Tuple<string, int,int> count_by_name(XmlNode node, Dictionary<string, string>[] flaws)
        {
            int ret = 0;
            string name = node["name"]?.InnerText;
            string flaw = node["flaw"]?.InnerText;
            double max_l = 0.0;
            double max_w = 0.0;
            double min_l = 0.0;
            double min_w = 0.0;
            string logic = node["logic"]?.InnerText;
            double score = 0.0;
            double unit_score = 0.0;
            if (!double.TryParse(node["length_max"]?.InnerText, out max_l))
            {
                max_l = 0.0;
            }
            if (!double.TryParse(node["width_max"]?.InnerText, out max_w))
            {
                max_w = 0.0;
            }
            if (!double.TryParse(node["length_min"]?.InnerText, out min_l))
            {
                min_l = 0.0;
            }
            if (!double.TryParse(node["width_min"]?.InnerText, out min_w))
            {
                min_w = 0.0;
            }
            if(_score_doc!=null && _score_doc.DocumentElement != null)
            {
                if(!double.TryParse(_score_doc.DocumentElement[name]?.InnerText, out unit_score))
                {
                    unit_score = 0.0;
                }
            }
            foreach(Dictionary<string,string> f in flaws)
            {
                Tuple<double, double, double> fd = get_data_from_flaw(f);
                if (string.Compare(name, "Scratch-AA-S") == 0 ||
                    string.Compare(name, "Scratch-AA-Minor") == 0 ||
                    string.Compare(name, "Scratch-A-S1") == 0 ||
                    string.Compare(name, "Scratch-A-S2") == 0 ||
                    string.Compare(name, "Scratch-A-Minor") == 0 ||
                    string.Compare(name, "Scratch-B-S1") == 0 ||
                    string.Compare(name, "Scratch-B-S2") == 0 ||
                    string.Compare(name, "Scratch-B-Minor") == 0 ||
                    string.Compare(name, "Scratch-C-S1") == 0 ||
                    string.Compare(name, "Scratch-C-S2") == 0 ||
                    string.Compare(name, "Scratch-C-Minor") == 0 ||
                    string.Compare(name, "Nick-AA-S") == 0 ||
                    string.Compare(name, "Nick-AA-Minor") == 0 ||
                    string.Compare(name, "Nick-A-S") == 0 ||
                    string.Compare(name, "Nick-A-Minor") == 0 ||
                    string.Compare(name, "Nick-B-S") == 0 ||
                    string.Compare(name, "Nick-B-Minor") == 0 ||
                    string.Compare(name, "Nick-C-S") == 0 ||
                    string.Compare(name, "Nick-C-Minor") == 0 ||
                    string.Compare(name, "Crack-B") == 0 ||
                    string.Compare(name, "Crack-C") == 0 ||
                    string.Compare(name, "Discoloration-A-S") == 0 ||
                    string.Compare(name, "Discoloration-A-Minor") == 0 ||
                    string.Compare(name, "Discoloration-C-S") == 0 ||
                    string.Compare(name, "Discoloration-C-Minor") == 0 )
                {
                    //if (fd.Item1 < max_l && fd.Item2 < max_w && fd.Item3 > 0.14)
                    if (fd.Item1 > min_l && fd.Item1 < max_l && fd.Item2 < max_w && fd.Item2 > min_w && fd.Item3 > 0.1)
                    {
                        ret++;
                        score += unit_score;
                    }
                        
                }
                else if (string.Compare(name, "Scratch-B-Major") == 0 ||
                    string.Compare(name, "Scratch-C-Major") == 0 ||
                    string.Compare(name, "Nick-B-Major") == 0 ||
                    string.Compare(name, "Nick-C-Major") == 0 ||
                    string.Compare(name, "Discoloration-A-Major") == 0 ||
                    string.Compare(name, "Discoloration-C-Major") == 0)
                {
                    if (fd.Item1 > min_l && fd.Item1 < max_l && fd.Item2 < max_w && fd.Item2 > min_w)
                    {
                        ret++;
                        score += unit_score;
                    }
                }
                else if (string.Compare(name, "Scratch-AA-Major") == 0 ||
                    string.Compare(name, "Scratch-A-Major") == 0 ||
                    string.Compare(name, "Nick-AA-Major") == 0 ||
                    string.Compare(name, "Nick-A-Major") == 0)
                {
                    if (fd.Item1 > min_l && fd.Item1 < max_l && fd.Item2 < max_w && fd.Item2 > min_w)
                    {
                        ret++;
                        score += unit_score;
                    }
                }
                else if (string.Compare(name, "Scratch-AA-Other1") == 0 ||
                    string.Compare(name, "Scratch-AA-Other2") == 0 ||
                    string.Compare(name, "Scratch-A-Other3") == 0 ||
                    string.Compare(name, "Scratch-A-Other2") == 0 ||
                    string.Compare(name, "Scratch-A-Other1") == 0 ||
                    string.Compare(name, "Scratch-B-Other3") == 0 ||
                    string.Compare(name, "Scratch-B-Other2") == 0 ||
                    string.Compare(name, "Scratch-B-Other1") == 0 ||
                    string.Compare(name, "Scratch-C-Other3") == 0 ||
                    string.Compare(name, "Scratch-C-Other2") == 0 ||
                    string.Compare(name, "Scratch-C-Other1") == 0 ||
                    string.Compare(name, "Nick-AA-Other2") == 0 ||
                    string.Compare(name, "Nick-AA-Other1") == 0 ||
                    string.Compare(name, "Nick-A-Other2") == 0 ||
                    string.Compare(name, "Nick-A-Other1") == 0 ||
                    string.Compare(name, "Nick-B-Other2") == 0 ||
                    string.Compare(name, "Nick-B-Other1") == 0 ||
                    string.Compare(name, "Nick-C-Other2") == 0 ||
                    string.Compare(name, "Nick-C-Other1") == 0 ||
                    string.Compare(name, "Crack-B-Other") == 0 ||
                    string.Compare(name, "Crack-C-Other") == 0 ||
                    string.Compare(name, "Discoloration-AA") == 0 ||
                    string.Compare(name, "Discoloration-A-Other1") == 0 ||
                    string.Compare(name, "Discoloration-A-Other2") == 0 ||
                    string.Compare(name, "Discoloration-C-Other1") == 0 ||
                    string.Compare(name, "Discoloration-C-Other2") == 0)
                {
                    if (fd.Item1 > min_l || fd.Item2 > min_w)
                    {
                        ret++;
                        score += unit_score;
                    }
                }
                else if (string.Compare(name, "Discoloration-B-Logo") == 0 ||
                    string.Compare(name, "Discoloration-B-Rear_Cam") == 0)
                {
                    if (string.Compare(f["flaw"], name) == 0)
                    {
                        ret++;
                        score += unit_score;
                    }
                }
                else if (string.Compare(name, "Scratch-A-WearedHomeButton") == 0 ||
                    string.Compare(name, "Crack-AA-A-Glass") == 0 ||
                    string.Compare(name, "Crack-AA-A-Glass") == 0)
                {
                    ret++;
                    // how to?
                }
                else if (string.Compare(name, "Discoloration-B-Area1") == 0 ||
                    string.Compare(name, "Discoloration-B-Area2") == 0 ||
                    string.Compare(name, "Discoloration-B-Area3") == 0)
                {
                    if (string.Compare(f["flaw"], name) == 0)
                    {
                        ret++;
                    }
                    // how to?
                }
                else if (string.Compare(name, "PinDotGroup-B-10x10")== 0 ||
                    string.Compare(name, "PinDotGroup-B-10x40") == 0||
                    string.Compare(name, "PinDotGroup-B-Other") ==0)
                {
                    if (string.Compare(f["flaw"], name) == 0)
                    {
                        ret++;
                    }
                }

            }
            //score *= 1000;
            return new Tuple<string, int, int>(name, ret, (int)score);
        }
        void recount()
        {
            Dictionary<string, Tuple<int,int>> flaw_cat = new Dictionary<string, Tuple<int, int>>();
            //standard spec = standard.LoadSpec(@"C:\tools\avia\classify_0520_mod.xml");
            standard spec = standard.TheSpec;
            foreach (XmlNode node in spec.get_all_category())
            {
                string surface = node["surface"]?.InnerText;
                string sort = node["sort"]?.InnerText;
                Dictionary<string, string>[] fs = get_flaws_by_surface_sort(surface, sort);
                foreach(XmlNode n in node["flaw"]?.ChildNodes)
                {
                    Tuple<string,int, int> cnt = count_by_name(n, fs);
                    //if (cnt.Item2 > 0)
                    flaw_cat[cnt.Item1] = new Tuple<int, int>(cnt.Item2, cnt.Item3);
                }
            }
#if false
            Program.logIt("dump: after recount:");
            foreach(KeyValuePair<string,int> kvp in flaw_cat)
            {
                Program.logIt($"{kvp.Key}={kvp.Value}");
            }
#endif
            // merge into _counts;
            foreach (KeyValuePair<string, Tuple<int, int>> kvp in flaw_cat)
            {
                _counts[kvp.Key] = kvp.Value;
            }
        }
        void score()
        {
            // load score xml
            XmlDocument doc = _score_doc;
            // score the device by flaw;
            Dictionary<string, object> all_flaws = new Dictionary<string, object>();
            foreach (Dictionary<string, string> f in _flaws)
            {
                string surface = f["surface"];
                string sort = f["sort"];
                string s = f["flaw"];
                if (string.Compare(s, "Fail", true) == 0)
                    continue;
                s = $"{sort}_{surface}";
                if (!all_flaws.ContainsKey(s))
                {
                    all_flaws.Add(s, new List<Dictionary<string, string>>());
                }
                List<Dictionary<string, string>> kv = (List<Dictionary<string, string>>)all_flaws[s];
                kv.Add(f);
            }
            foreach(KeyValuePair<string,object> kvp in all_flaws)
            {
                double sum = 0.0;
                List<Dictionary<string, string>> fd = (List<Dictionary<string, string>>)kvp.Value;
                foreach (Dictionary<string, string> f in fd)
                {
                    if (string.Compare(f["sort"], "Discoloration") == 0)
                    {
                        sum += 1;
                    }
                    else
                    {
                        if (f.ContainsKey("area"))
                        {
                            string[] s = f["area"].Split(' ');
                            double v;
                            if (s.Length > 0 && double.TryParse(s[0], out v))
                            {
                                sum += v;
                            }
                        }
                    }
                }
                int w = 1000;
                if (!Int32.TryParse(doc.DocumentElement[kvp.Key]?.InnerText, out w))
                    w = 1000;
                sum *= w;
                _scores.Add(kvp.Key, (int)sum);
            }
        }
    }
}
