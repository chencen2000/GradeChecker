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
    #region Grading Criteria xml
    public class logical_item
    {
        public string name;
        public string type;
        public double length_min;
        public double length_max;
        public double width_min;
        public double width_max;
        public double area_max;        
        public double count;
    }
    public class grade_item
    {
        public string surface;
        public int max_defect_total;
        public int max_defect_major;
        public int max_defect_scratch_s_minor;
        public int max_defect_nick_gcd_minor;
        public int max_defect_discoloration_minor;
        public string rule;
        [XmlArrayAttribute("logical_and")]
        [XmlArrayItemAttribute("defect")]
        public logical_item[] logical_and_items;
        [XmlArrayAttribute("logical_or")]
        [XmlArrayItemAttribute("defect")]
        public logical_item[] logical_or_items;

    }
    public class grade
    {
        [XmlAttribute]
        public string name;
        public int max_defect_total;
        [XmlArrayAttribute("surface_grade")]
        [XmlArrayItemAttribute("item")]
        public grade_item[] grade_items;
    }
    [XmlRoot("grading_criteria")]
    public class grade_criteria
    {
        [XmlAttribute]
        public int version;
        [XmlAttribute]
        public int subversion;
        //[XmlArrayAttribute("category")]
        //[XmlArrayItemAttribute("item")]
        [XmlElement("grade")]
        public grade[] grades;
    }

    #endregion

/*
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
        public string defect_item;
        public double length;
        public double width;
        public double area_mm;
        public double area_pixel;
        public double contrast;
        public string region;
        public double value;
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
        [XmlAttribute]
        public string version;
        public int index;
        public string model;
        public string time;
        [XmlElement("station")]
        public station[] stations;
    }

    #endregion
*/
    class test1
    {
        static void Main(string[] args)
        {
            test_1();
            //test_2();
        }

        static void test_2()
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
            string s = @"data\Grading Criteria.xml";
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(grade_criteria));
                StreamReader reader = new StreamReader(s);
                grade_criteria c = (grade_criteria)serializer.Deserialize(reader);
                reader.Close();
            }
            catch (Exception) { }
        }
    }
}
