using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace GradeChecker1
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

    public class item_1
    {
        [XmlAttribute("class")]
        public string @class;
        [XmlAnyElement]
        public XmlElement[] Nodes
        {
            get { return _nodes == null ? null : _nodes.ToArray(); }
            set
            {
                _nodes = new List<XmlElement>();
                // save to _nodes;
                foreach (var v in value)
                    _nodes.Add(v);
            }
        }
        public string type;
        public string defect_item;
        public double length;
        public double width;
        public double area_mm;
        public double area_pixel;
        public double contrast;
        public string region;
        public double value;
        //[XmlArrayAttribute("location")]
        //[XmlArrayItemAttribute("point")]
        //public item_location[] points;
        [XmlIgnore]
        public string surface;
        [XmlIgnore]
        public double threshold;
        [XmlIgnore]
        public List<XmlElement> _nodes;
    }

    public class item
    {
        [XmlAttribute("class")]
        public string @class;
        public string type;
        public string defect_item;
        public double length;
        public double width;
        public double area_mm;
        public double area_pixel;
        public double contrast;
        public string region;
        public double value;
        [JsonIgnore]
        [XmlArrayAttribute("location")]
        [XmlArrayItemAttribute("point")]
        public item_location[] points;
        [XmlIgnore]
        public string surface;
        [XmlIgnore]
        public double threshold;
    }
    public class defect_finder
    {
        //public int defect_unit;
        //public defect_sort sort;
        [XmlAnyElement]
        public XmlElement[] Nodes;

    }

    public class defect_item
    {
        [XmlAttribute]
        public int index;
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
        public defect_item[] defect_items;
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
        [XmlAttribute]
        public string version;
        public int index;
        public string model;
        public string time;
        [XmlElement("station")]
        public station[] stations;
    }

    #endregion

    class DeviceDefect
    {
        public DeviceDefect(string filename)
        {
            parse(filename);
        }
        static void Main(string[] args)
        {
            test();
        }

        static void test()
        {
            DeviceDefect dd = new DeviceDefect(@"data\defect_123_B.xml");
            if (dd.Ready)
            {
                item[] defects = dd.get_all_defects();
                JsonSerializer serializer = new JsonSerializer();
                using (StreamWriter sw = new StreamWriter(@"test.json"))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, defects);
                }
            }

        }

        defect_record _defect_record = null;

        public bool Ready { get => _defect_record != null; /*set => ready = value; */}
        defect_item[] find_defect_item_by_index(sensor ss, string index)
        {
            List<defect_item> ret = new List<defect_item>();
            if (ss != null && !string.IsNullOrEmpty(index))
            {
                string[] idx = index.Split(',');
                foreach (string s in idx)
                {
                    int i;
                    if (Int32.TryParse(s, out i))
                    {
                        foreach (defect_item di in ss.defect_items)
                        {
                            if (di.index == i)
                            {
                                ret.Add(di);
                                break;
                            }
                        }
                    }
                }
            }
            return ret.ToArray();
        }
        void parse(string filename)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(defect_record));
                StreamReader reader = new StreamReader(filename);
                defect_record c = (defect_record)serializer.Deserialize(reader);
                reader.Close();
                foreach(station st in c.stations)
                {
                    foreach(surface sf in st.surfaces)
                    {
                        //string surface_name = sf.name;
                        foreach(sensor ss in sf.sensors)
                        {
                            foreach(item i in ss.items)
                            {
                                i.surface = sf.name;
                                defect_item[] di = find_defect_item_by_index(ss, i.defect_item);
                                if (di.Length > 0)
                                {
                                    i.threshold = di[0].threshold;
                                }
                            }
                        }
                    }
                }
                _defect_record = c;
            }
            catch (Exception) { }            
        }
        public item[] get_all_defects()
        {
            List<item> ret = new List<item>();
            if (_defect_record != null)
            {
                defect_record c = _defect_record;
                foreach (station st in c.stations)
                {
                    foreach (surface sf in st.surfaces)
                    {
                        foreach (sensor ss in sf.sensors)
                        {
                            foreach (item i in ss.items)
                            {
                                ret.Add(i);
                            }
                        }
                    }
                }
            }
            return ret.ToArray();
        }
    }
}
