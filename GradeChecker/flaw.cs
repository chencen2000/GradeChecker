using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GradeChecker
{
    class flaw
    {
        List<System.Collections.Generic.Dictionary<string, string>> _flaws = new List<Dictionary<string, string>>();
        System.Collections.Generic.Dictionary<string, int> _counts = new Dictionary<string, int>();
        string _grade = "";
        public flaw(string filename)
        {
            parse(filename);
        }

        public Dictionary<string, int> Counts { get => _counts; }
        public List<Dictionary<string, string>> Flaws { get => _flaws; /*set => _flaws = value;*/ }
        public string Grade { get => _grade; /*set => _grade = value;*/ }

        static void Main(string[] args)
        {
            test();
        }
        static void test()
        {
            flaw f = new flaw(@"data\classify_643.txt");
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
                            int pos = line.IndexOf('=');
                            if(pos>0 && pos + 1 < line.Length)
                            {
                                string k = line.Substring(0, pos).Trim();
                                string v = line.Substring(pos+1).Trim();
                                int i;
                                if (Int32.TryParse(v, out i))
                                    _counts.Add(k, i);
                            }
                        }
                    }
                }
            }
            catch (Exception) { }
        }
        public void dump()
        {
            Program.logIt($"Device Grade: ({_grade})");
            Program.logIt($"Dump device flaws: ({_flaws.Count})");
            foreach(Dictionary<string,string> d in _flaws)
            {
                Program.logIt(string.Join(",", d.Select(kv => kv.Key + "=" + kv.Value).ToArray()));
            }
            Program.logIt($"Dump device counts: ({_counts.Count})");
            Program.logIt(string.Join(System.Environment.NewLine, _counts.Select(kv => kv.Key + "=" + kv.Value).ToArray()));
        }
    }
}
