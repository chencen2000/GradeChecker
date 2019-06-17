using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace GradeChecker
{
    #region detail log xml
    public class item_location
    {
        [XmlIgnore]
        public PointF point;
        [XmlText]
        public string pointStr
        {
            get
            {
                if (point != null && !point.IsEmpty)
                    return $"{point.X},{point.Y}";
                else
                    return "";
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    var xy = value.Split(',');
                    if (xy.Length == 2)
                    {
                        float x;
                        float y;
                        if (float.TryParse(xy[0], out x) && float.TryParse(xy[1], out y))
                        {
                            point = new PointF(x, y);
                        }
                    }
                }
            }
        }

    }

    public class item
    {
        [XmlAttribute("class")]
        public string @class;
        public string type;
        public int defect_item;
        public double length;
        public double width;
        public double area_mm;
        public double area_pixel;
        public double contrast;
        [XmlArrayAttribute("location")]
        [XmlArrayItemAttribute("point")]
        public item_location[] points;
    }
    public class defect_finder
    {
        //public int defect_unit;
        //public defect_sort sort;
        [XmlAnyElement]
        public XmlElement[] Nodes;

    }

    public class defact_item
    {
        public bool enabled;
        public int color;
        public int threshold;
        public defect_finder defect_finder;
    }

    public class sensor
    {
        [XmlAttribute]
        public string name;
        [XmlAttribute]
        public string type;
        [XmlArrayAttribute("parameter")]
        [XmlArrayItemAttribute("defect_item")]
        public defact_item[] defect_items;
        [XmlArrayAttribute("defect")]
        [XmlArrayItemAttribute("item")]
        public item[] items;

    }

    public class surface
    {
        [XmlAttribute]
        public string name;
        [XmlElement("sensor")]
        public sensor[] sensors;
    }
    public class station
    {
        [XmlAttribute]
        public string name;
        [XmlElement("surface")]
        public surface[] surfaces;
    }

    public class defect_record
    {
        public int index;
        public string model;
        public string time;
        [XmlElement("station")]
        public station[] stations;
    }

    #endregion
    class test
    {

        static string[] gradeing_label = new string[] { "A+", "A", "B", "C", "D+", "D" };
        static void Main(string[] args)
        {
            //test_2();
            //test_3();
            //test_1();
            test_4();
        }

        static void test_4()
        {
            string s = @"data\defect_123_B.xml";
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(defect_record));
                StreamReader reader = new StreamReader(s);
                defect_record c = (defect_record)serializer.Deserialize(reader);
                reader.Close();
            }
            catch (Exception) { }
        }
        static void test_1()
        {
            System.Collections.Specialized.StringDictionary[] vdata = Program.read_verizon_data();
            standard spec = standard.LoadSpec(@"C:\Tools\avia\classify.xml");
            Dictionary<string, object> specs = spec.ToDictionary();
            //Dictionary<string, object>[] ddata = Program.prep(@"C:\Tools\avia\test", vdata);
            Dictionary<string, object>[] ddata = Program.prep(@"C:\Tools\avia\ClassifyLog", vdata);
            // save json
            {
                var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                string s = jss.Serialize(ddata);
                System.IO.File.WriteAllText("temp.json", s);
            }

            List<string> keys = new List<string>();
            foreach (Dictionary<string, object> d in ddata)
            {
                foreach (KeyValuePair<string, object> kvp in d)
                {
                    if (kvp.Key.StartsWith("Score_"))
                    {
                        if (!keys.Contains(kvp.Key))
                            keys.Add(kvp.Key);
                    }
                }
            }
            List<Dictionary<string, object>> report = new List<Dictionary<string, object>>();
            foreach (Dictionary<string, object> d in ddata)
            {
                Dictionary<string, object> r = new Dictionary<string, object>();
                //r.Add("imei", d["imei"]);
                //r.Add("VZW", d["VZW"]);
                r.Add("VZW", Array.IndexOf(gradeing_label, d["VZW"]));
                foreach (string k in keys)
                {
                    if (d.ContainsKey(k))
                    {
                        r.Add(k, d[k]);
                    }
                    else
                    {
                        r.Add(k, 0);
                    }
                }
                report.Add(r);
            }
            // save json
            {
                var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                string s = jss.Serialize(report);
                System.IO.File.WriteAllText("temp_1.json", s);
            }

            // dump
            foreach (Dictionary<string,object> d in ddata)
            {                
                StringBuilder sb = new StringBuilder();
                sb.Append($"imei={d["imei"]},");
                sb.Append($"VZW={d["VZW"]},");
                foreach(KeyValuePair<string,object> kvp in d)
                {
                    if (kvp.Key.StartsWith("Score_"))
                    {
                        sb.Append($"{kvp.Key}={kvp.Value},");
                    }
                }
                System.Console.WriteLine(sb.ToString());
            }
        }
        static void test_2()
        {
            Dictionary<string, object> all_flaws = new Dictionary<string, object>();
            string root = @"C:\Tools\avia\ClassifyLog";  // @"C:\Tools\avia\ClassifyLog";
            standard spec = standard.LoadSpec(@"C:\Tools\avia\classify.xml");
            Dictionary<string, object> specs = spec.ToDictionary();
            foreach (string fn in System.IO.Directory.GetFiles(root, "*.txt"))
            {
                flaw df = new flaw(fn);
                foreach(Dictionary<string,string> f in df.Flaws)
                {
                    string surface = f["surface"];
                    string sort= f["sort"];
                    string k = $"{sort}_{surface}";
                    if (!all_flaws.ContainsKey(k))
                    {
                        all_flaws.Add(k, new List<Dictionary<string, object>>());
                    }
                    List<Dictionary<string, object>> kv = (List<Dictionary<string, object>>)all_flaws[k];
                    kv.Add(f.ToDictionary(pair => pair.Key, pair => (object)pair.Value));
                }
            }
            // save json
            {
                var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                string s = jss.Serialize(all_flaws);
                System.IO.File.WriteAllText("all_flaws.json", s);
            }

        }
        static Tuple<double,double> get_max_min_area(ArrayList data)
        {
            double min = 100.0;
            double max = 0.0;
            foreach(Dictionary<string,object> d in data)
            {
                double v;
                if(d.ContainsKey("area"))
                {
                    string []s = (d["area"] as string).Split(' ');
                    if (s.Length > 0 && double.TryParse(s[0], out v))
                    {
                        if (v > max) max = v;
                        if (v != 0 && v < min) min = v;
                    }
                }
            }
            return new Tuple<double, double>(max, min);
        }
        static void test_3()
        {
            string str = System.IO.File.ReadAllText("all_flaws.json");
            var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
            Dictionary<string, object> all_flaws = jss.Deserialize<Dictionary<string,object>>(str);
            foreach(KeyValuePair<string,object> kvp in all_flaws)
            {
                string key = kvp.Key;
                ArrayList flaws = (ArrayList)kvp.Value;
                Tuple<double, double> mm = get_max_min_area(flaws);
                Program.logIt($"{key}: max={mm.Item1}, min={mm.Item2}");
            }
        }
    }
}
