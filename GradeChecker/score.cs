using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace GradeChecker
{
    class score
    {
        static Dictionary<string, double> _scores = new Dictionary<string, double>();
        static score()
        {
            try
            {
                XmlDocument _doc = new XmlDocument();
                _doc.Load(System.IO.Path.Combine(Program.Root, "score.xml"));
                if (_doc.DocumentElement != null)
                {
                    foreach(XmlNode n in _doc.DocumentElement.ChildNodes)
                    {
                        double i;
                        if (Double.TryParse(n.InnerText, out i))
                        {
                            _scores[n.Name] = i;
                        }
                    }
                }
            }
            catch (Exception) { }
        }

        public static Dictionary<string, double> Scores { get => _scores; /*set => _scores = value;*/ }

        public static Dictionary<string, Tuple<int, double>> get_score_by_spec(string grade, Dictionary<string,object> spec)
        {
            Dictionary<string, Tuple<int,double>> s = new Dictionary<string, Tuple<int, double>>();
            foreach(KeyValuePair<string,object> kvp in spec)
            {
                int count = 0;
                double key_score = 0.0;
                string key = kvp.Key;
                count = (int)kvp.Value;
                if (_scores.ContainsKey(key))
                {
                    key_score = _scores[kvp.Key] * (int)kvp.Value;
                    //total_score += key_score;
                }
                s.Add(key, new Tuple<int, double>(count, key_score));
            }
            {
                string g = $"grade-{grade.Replace("+", "P")}";
                if(_scores.ContainsKey(g))
                    s.Add("total", new Tuple<int, double>(0, _scores[g]));
            }
            return s;
        }
        public static double get_score_by_key(string key)
        {
            double ret = 0.0;
            if (_scores.ContainsKey(key))
            {
                ret = _scores[key];
            }
            return ret;
                
        }
    }
}
