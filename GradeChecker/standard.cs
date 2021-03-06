﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace GradeChecker
{
    #region spec xml define
    public class flaw_allow_item
    {
        public string flaw;
        public int allow;
    }

    public class surface_item
    {
        public string surface;
        public int max_flaws;
        public int max_major_flaws;
        public int max_region_flaws;
        [XmlArrayAttribute("flaw_allow")]
        [XmlArrayItemAttribute("item")]
        public flaw_allow_item[] flaw_allow;
    }

    public class grade_item
    {
        [XmlElement("name")]
        public string grade;
        public int max_flaws;
        public int max_major_flaws;
        public int max_region_flaws;
        [XmlArrayAttribute("surface_grade")]
        [XmlArrayItemAttribute("item")]
        public surface_item[] surface;
    }

    public class flaw_item
    {
        [XmlElement("name")]
        public string name;
        [XmlElement("length_max")]
        public double max_length;
        [XmlElement("length_min")]
        public double min_length;
        [XmlElement("width_max")]
        public double max_width;
        [XmlElement("width_min")]
        public double min_width;
        [XmlElement("logic")]
        public string logic;
        public double area_max;
        public double area_min;
        public string area_name;
    }

    public class category_item
    {
        public string sort;
        public string surface;
        [XmlArrayAttribute()]
        [XmlArrayItemAttribute("item")]
        public flaw_item[] flaw;
    }

    public class classify
    {
        [XmlArrayAttribute("category")]
        [XmlArrayItemAttribute("item")]
        public category_item[] categories;
        [XmlArrayAttribute("grade")]
        [XmlArrayItemAttribute("item")]
        public grade_item[] grades;
    }
    #endregion
    class standard
    {
        /// <summary>
        /// Specification Array
        /// </summary>
        static classify _theClassify = null;
        /// <summary>
        /// Specification XML
        /// </summary>
        static standard _theSpec = null;
        XmlDocument _spec = null;

        internal static standard TheSpec { get => _theSpec; /*set => _theSpec = value; */}
        public static classify TheClassify { get => _theClassify; /*set => _theClassify = value; */}

        public standard(XmlDocument doc)
        {
            _spec = doc;
        }
        static void Main(string[] args)
        {
            test();
        }
        static void test()
        {
            // load standard from xml
            string s = @"data\classify.xml";
#if false
            try
            {
                flaw_item[] fi = new flaw_item[2];
                flaw_item f = new flaw_item();
                f.name = "test";
                f.max_width = 1.0;
                f.max_length = 0.5;
                fi[0] = f;
                fi[1] = f;
                category_item ci = new category_item();
                ci.flaw = fi;
                ci.sort = "sort";
                ci.surface = "AA";
                classify c = new classify();
                c.categories = new category_item[] { ci };
                XmlSerializer serializer = new XmlSerializer(typeof(classify));
                var output = new StringBuilder();
                using (XmlWriter xw = XmlWriter.Create(output))
                {
                    serializer.Serialize(xw, c);
                }
            }
            catch (Exception) { }
#else
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(classify));
                StreamReader reader = new StreamReader(s);
                classify c = (classify)serializer.Deserialize(reader);
                reader.Close();
            }
            catch (Exception) { }

            // load all keys from spec
            try
            {
                string str = System.IO.File.ReadAllText(@"data\spec_keys.txt");
                var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                Dictionary<string, object> data1 = jss.Deserialize<Dictionary<string, object>>(str);
                //var v = jss.DeserializeObject(s);
                List<string> all_keys = new List<string>();
                foreach(KeyValuePair<string,object> kvp in data1)
                {
                    Dictionary<string, object> d = (Dictionary<string, object> )kvp.Value;
                    foreach(KeyValuePair<string,object> kv in d)
                    {
                        if (!all_keys.Contains(kv.Key))
                            all_keys.Add(kv.Key);

                    }
                }
                str = jss.Serialize(all_keys);
            }
            catch (Exception) { }
#endif
        }
        public static standard LoadSpec(string spec)
        {
            standard ret = null;
            //string s = @"data\classify.xml";
            if (!string.IsNullOrEmpty(spec))
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(classify));
                    StreamReader reader = new StreamReader(spec);
                    _theClassify = (classify)serializer.Deserialize(reader);
                    reader.Close();
                }
                catch (Exception) { }

                XmlDocument doc = new XmlDocument();
                try
                {
                    if (System.IO.File.Exists(spec))
                        doc.Load(spec);
                    else
                        doc.LoadXml(spec);
                    ret = new standard(doc);
                }
                catch (Exception) { }
            }
            _theSpec = ret;
            return ret;
        }

        void dump()
        {
            if(_spec!=null && _spec.DocumentElement != null)
            {

            }
        }
        public XmlNodeList get_all_grade()
        {
            return _spec.DocumentElement["grade"].ChildNodes;
        }
        public XmlNode get_grade_item(string grade)
        {
            XmlNode ret = null;
            if (_spec != null && _spec.DocumentElement != null)
            {
                foreach (XmlNode n in _spec.DocumentElement["grade"].ChildNodes)
                {
                    if (string.Compare(grade, n["grade"]?.InnerText) == 0 )
                    {
                        ret = n;
                        break;
                    }
                }
            }
            return ret;
        }
        public XmlNodeList get_all_category()
        {
            return _spec.DocumentElement["category"].ChildNodes;
        }
        public XmlNode get_category_item(string sort, string surface)
        {
            XmlNode ret = null;
            if (_spec != null && _spec.DocumentElement != null)
            {
                foreach(XmlNode n in _spec.DocumentElement["category"].ChildNodes)
                {
                    if(string.Compare(sort, n["sort"]?.InnerText)==0 && string.Compare(surface, n["surface"]?.InnerText) == 0)
                    {
                        ret = n;
                        break;
                    }
                }
            }
            return ret;
        }
        public string grade(flaw device_flaw)
        {
            string ret = "";
            //Get the grade specification in Classify.xml
            XmlNodeList grades = get_all_grade();
            foreach(XmlNode t_CurrentGrade in grades)
            {
                if(meet_grade(t_CurrentGrade, device_flaw))
                {
                    // pass 
                    ret = t_CurrentGrade["name"]?.InnerText;
                    Program.logIt($"Grade {ret}");
                    break;
                }
            }
            return ret;
        }
        
        bool meet_grade(XmlNode f_GradeLevel, flaw device_flas)
        {
            bool ret = false;
            string g = f_GradeLevel["name"]?.InnerText;
            Program.logIt($"meet_grade: Grade {g}");
            //the max flaws of grade spacification per surface in classify.xml
            bool t_IsStopCount = false;
            if (f_GradeLevel["max_flaws"] != null)
            {
                int max_flaws = 0;
                if (Int32.TryParse(f_GradeLevel["max_flaws"]?.InnerText, out max_flaws))
                {
                    //int j = device_flas.count_total_flaws();
                    //base on grade level to assign defect name
                    int j = device_flas.count_total_flaws_by_grade(f_GradeLevel);
                    //int j = device_flas.Flaws.Count;                    
                    if (max_flaws < j)
                    {
                        t_IsStopCount = true;
                        if (g.CompareTo("A+") == 0)
                        {
                            Program.m_Result.m_GradeAPlusDecision.Add($"the total defects limitation is {j} bigger than {max_flaws}");
                        }
                        else if (g.CompareTo("A") == 0)
                        {
                            Program.m_Result.m_GradeADecision.Add($"the total defects limitation is {j} bigger than {max_flaws}");
                        }
                        else if (g.CompareTo("B") == 0)
                        {
                            Program.m_Result.m_GradeBDecision.Add($"the total defects limitation is {j} bigger than {max_flaws}");
                        }
                        else if (g.CompareTo("C") == 0)
                        {
                            Program.m_Result.m_GradeCDecision.Add($"the total defects limitation is {j} bigger than {max_flaws}");
                        }
                        else if (g.CompareTo("D+") == 0)
                        {
                            Program.m_Result.m_GradeDPlusDecision.Add($"the total defects limitation is {j} bigger than {max_flaws}");
                        }
                        else if(g.CompareTo("D") == 0)
                        {
                            Program.m_Result.m_GradeDDecision.Add($"the total defects of no matching with D is {j} bigger than {max_flaws}");
                        }
                        Program.logIt($"Fail to meet max_flaws condition. ({j}>{f_GradeLevel["max_flaws"]?.InnerText})");
                        goto exit;
                    }
                }
            }

            if (f_GradeLevel["surface_grade"] != null)
            {
                foreach(XmlNode n in f_GradeLevel["surface_grade"].ChildNodes)
                {
                    if (!meet_surface_grade(g, n, device_flas))
                    {
                        goto exit;
                    }
                }
            }
            ret = true;
            exit:
            Program.logIt($"meet_grade: -- ret={ret}");
            return ret;
        }
        bool meet_surface_grade(string grade, XmlNode node, flaw device_flas)
        {
            bool ret = false;
            string surface = node["surface"].InnerText;
            Program.logIt($"meet_surface_grade: ++ surface={surface}");
            if (node["max_flaws"] != null)
            {
                int j = 0;
                int i;
                if(Int32.TryParse(node["max_flaws"].InnerText, out i))
                {
                    //foreach(var f in device_flas.Flaws)
                    //{
                    //    if(f.ContainsKey("surface") && string.Compare(f["surface"], surface) == 0)
                    //    {
                    //        j++;
                    //    }
                    //}
                    //j = device_flas.count_total_flaws_by_surface(surface);
                    j = device_flas.count_total_flaws_by_surface(node);
                    if (j > i)
                    {
                        if (grade.CompareTo("A+") == 0)
                        {
                            Program.m_Result.m_GradeAPlusDecision.Add($"too many defects on Surface {surface}, it is {j} bigger than {i}");
                        }
                        else if (grade.CompareTo("A") == 0)
                        {
                            Program.m_Result.m_GradeADecision.Add($"too many defects on Surface {surface}, it is {j} bigger than {i}");
                        }
                        else if (grade.CompareTo("B") == 0)
                        {
                            Program.m_Result.m_GradeBDecision.Add($"too many defects on Surface {surface}, it is {j} bigger than {i}");
                        }
                        else if (grade.CompareTo("C") == 0)
                        {
                            Program.m_Result.m_GradeCDecision.Add($"too many defects on Surface {surface}, it is {j} bigger than {i}");
                        }
                        else if (grade.CompareTo("D+") == 0)
                        {
                            Program.m_Result.m_GradeDPlusDecision.Add($"too many defects on Surface {surface}, it is {j} bigger than {i}");
                        }
                        else if (grade.CompareTo("D") == 0)
                        {
                            Program.m_Result.m_GradeDDecision.Add($"too many defects on Surface {surface}, it is {j} bigger than {i}");
                        }
                        Program.logIt($"meet_surface_grade: Failed, due to max_flaws={j} (max: {i})");
                        goto exit;
                    }
                }
            }
            if (node["max_major_flaws"] != null)
            {
                int j = 0;
                int i;
                if (Int32.TryParse(node["max_major_flaws"].InnerText, out i))
                {
                    foreach (KeyValuePair<string, Tuple<int,int>> kvp in device_flas.Counts)
                    {
                        if (Regex.Match(kvp.Key, $@"^.*-{surface}-Major$").Success)
                        {
                            j += kvp.Value.Item1;
                        }
                    }
                    if (j > i)
                    {
                        if (grade.CompareTo("A+") == 0)
                        {
                            Program.m_Result.m_GradeAPlusDecision.Add($"max major flaws on Surface {surface}, it is {j} bigger than {i}");
                        }
                        else if (grade.CompareTo("A") == 0)
                        {
                            Program.m_Result.m_GradeADecision.Add($"max major flaws on Surface {surface}, it is {j} bigger than {i}");
                        }
                        else if (grade.CompareTo("B") == 0)
                        {
                            Program.m_Result.m_GradeBDecision.Add($"max major flaws on Surface {surface}, it is {j} bigger than {i}");
                        }
                        else if (grade.CompareTo("C") == 0)
                        {
                            Program.m_Result.m_GradeCDecision.Add($"max major flaws on Surface {surface}, it is {j} bigger than {i}");
                        }
                        else if (grade.CompareTo("D+") == 0)
                        {
                            Program.m_Result.m_GradeDPlusDecision.Add($"max major flaws on Surface {surface}, it is {j} bigger than {i}");
                        }
                        else if (grade.CompareTo("D") == 0)
                        {
                            Program.m_Result.m_GradeDDecision.Add($"max major flaws on Surface {surface}, it is {j} bigger than {i}");
                        }
                        Program.logIt($"meet_surface_grade: Failed, due to max_major_flaws={j} (max: {i})");
                        goto exit;
                    }
                }
            }
            bool t_IsStopCountFlag = false;
            if (node["max_region_flaws"] != null)
            {
                int j = 0;
                int i;
                if (Int32.TryParse(node["max_region_flaws"].InnerText, out i))
                {
                    foreach (KeyValuePair<string, int> kvp in device_flas.Zones)
                    {
                        if (j > i)
                        {
                            t_IsStopCountFlag = true;
                            if (grade.CompareTo("A+") == 0)
                            {
                                Program.m_Result.m_GradeAPlusDecision.Add($"max region flaws on Surface {surface}, it is {j} bigger than {i}");
                            }
                            else if (grade.CompareTo("A") == 0)
                            {
                                Program.m_Result.m_GradeADecision.Add($"max region flaws on Surface {surface}, it is {j} bigger than {i}");
                            }
                            else if (grade.CompareTo("B") == 0)
                            {
                                Program.m_Result.m_GradeBDecision.Add($"max region flaws on Surface {surface}, it is {j} bigger than {i}");
                            }
                            else if (grade.CompareTo("C") == 0)
                            {
                                Program.m_Result.m_GradeCDecision.Add($"max region flaws on Surface {surface}, it is {j} bigger than {i}");
                            }
                            else if (grade.CompareTo("D+") == 0)
                            {
                                Program.m_Result.m_GradeDPlusDecision.Add($"max region flaws on Surface {surface}, it is {j} bigger than {i}");
                            }
                            else if (grade.CompareTo("D") == 0)
                            {
                                Program.m_Result.m_GradeDDecision.Add($"max region flaws on Surface {surface}, it is {j} bigger than {i}");
                            }
                            Program.logIt($"meet_surface_grade: Failed, due to max_region_flaws={j} (max: {i})");
                            //goto exit;
                        }
                    }
                    if(t_IsStopCountFlag == true)
                    {
                        goto exit;
                    }
                }
            }
            // check counts;
            t_IsStopCountFlag = false;
            foreach (XmlNode n in node["flaw_allow"]?.ChildNodes)
            {
                string name = n["flaw"]?.InnerText;
                string value = n["allow"]?.InnerText;
                if (device_flas.Counts.ContainsKey(name))
                {
                    int i;
                    if (Int32.TryParse(value, out i))
                    {
                        if (device_flas.Counts[name].Item1 <= i)
                        {
                            // pass
                        }
                        else
                        {
                            t_IsStopCountFlag = true;
                            if (grade.CompareTo("A+") == 0)
                            {
                                Program.m_Result.m_GradeAPlusDecision.Add($"The {name} on Surface {surface}, it is {device_flas.Counts[name].Item1} bigger than {i}");
                            }
                            else if (grade.CompareTo("A") == 0)
                            {
                                Program.m_Result.m_GradeADecision.Add($"The {name}  on Surface {surface}, it is {device_flas.Counts[name].Item1} bigger than {i}");
                            }
                            else if (grade.CompareTo("B") == 0)
                            {
                                Program.m_Result.m_GradeBDecision.Add($"The {name}  on Surface {surface}, it is {device_flas.Counts[name].Item1} bigger than {i}");
                            }
                            else if (grade.CompareTo("C") == 0)
                            {
                                Program.m_Result.m_GradeCDecision.Add($"The {name}  on Surface {surface}, it is {device_flas.Counts[name].Item1} bigger than {i}");
                            }
                            else if (grade.CompareTo("D+") == 0)
                            {
                                Program.m_Result.m_GradeDPlusDecision.Add($"The {name}  on Surface {surface}, it is {device_flas.Counts[name].Item1} bigger than {i}");
                            }
                            else if (grade.CompareTo("D") == 0)
                            {
                                Program.m_Result.m_GradeDDecision.Add($"The {name}  on Surface {surface}, it is {device_flas.Counts[name].Item1} bigger than {i}");
                            }
                            Program.logIt($"meet_surface_grade: Failed, due to {name}={device_flas.Counts[name]} (max: {value})");
                            //goto exit;
                        }
                    }
                }
            }
            if (t_IsStopCountFlag == true)
            {
                goto exit;
            }
            // finally
            ret = true;
            exit:
            Program.logIt($"meet_surface_grade: -- ret={ret}");
            return ret;
        }
        public string[] get_all_flaw_keys()
        {
            List<string> ret = new List<string>();
            try
            {
                if(_spec!=null && _spec.DocumentElement != null)
                {
                    foreach (XmlNode n1 in _spec.DocumentElement["category"]?.ChildNodes)
                    {
                        foreach (XmlNode n in n1?["flaw"]?.ChildNodes)
                        {
                            string k = n["name"]?.InnerText;
                            if(!ret.Contains(k))
                                ret.Add(k);
                        }
                    }
                }
            }
            catch (Exception) { }
            return ret.ToArray();
        }
        public Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> ret = new Dictionary<string, object>();
            try
            {
                if (_spec!=null && _spec.DocumentElement != null)
                {
                    foreach (XmlNode gNode in _spec.DocumentElement["grade"]?.ChildNodes)
                    {
                        Dictionary<string, object> d = new Dictionary<string, object>();
                        string name = gNode["name"]?.InnerText;
                        //d.Add("grade", name);
                        ret.Add(name, d);
                        string ks = gNode["max_flaws"]?.InnerText;
                        string vs = "";
                        int v = 0;
                        // max_flaws
                        if (!string.IsNullOrEmpty(ks))
                        {
                            if (Int32.TryParse(ks, out v))
                            {
                                d.Add("all-all-all", v);
                            }
                        }
                        // max_major_flaws
                        ks = gNode["max_major_flaws"]?.InnerText;
                        if (!string.IsNullOrEmpty(ks))
                        {
                            if (Int32.TryParse(ks, out v))
                            {
                                d.Add($"all-major-all", v);
                            }
                        }

                        // loop each surface
                        foreach (XmlNode sNode in gNode["surface_grade"]?.ChildNodes)
                        {
                            name = sNode["surface"].InnerText;
                            if (!string.IsNullOrEmpty(name))
                            {
                                //  max_flaws
                                ks = sNode["max_flaws"]?.InnerText;
                                if (!string.IsNullOrEmpty(ks))
                                {
                                    if (Int32.TryParse(ks, out v))
                                    {
                                        d.Add($"{name}-all-all", v);
                                    }
                                }
                                // max_major_flaws
                                ks = sNode["max_major_flaws"]?.InnerText;
                                if (!string.IsNullOrEmpty(ks))
                                {
                                    if (Int32.TryParse(ks, out v))
                                    {
                                        d.Add($"{name}-major-all", v);
                                    }
                                }
                                // max_region_flaws
                                ks = sNode["max_region_flaws"]?.InnerText;
                                if (!string.IsNullOrEmpty(ks))
                                {
                                    if (Int32.TryParse(ks, out v))
                                    {
                                        d.Add($"{name}-region-all", v);
                                    }
                                }


                                // loop each flaw
                                foreach (XmlNode sf in sNode["flaw_allow"]?.ChildNodes)
                                {
                                    ks = sf["flaw"]?.InnerText;
                                    vs = sf["allow"].InnerText;
                                    if (Int32.TryParse(vs, out v))
                                    {
                                        d[ks] = v;
                                    }
                                }
                            }
                        }
                        //ret.Add(d);
                    }
                }
            }
            catch (Exception) { }
            return ret;
        }
        #region classify class access function
        static public grade_item get_grade_item_by_grade(classify c, string grade)
        {
            grade_item ret = null;
            foreach (grade_item f in c.grades)
            {
                if(string.Compare(f.grade,grade)==0)
                {
                    ret = f;
                    break;
                }
            }
            return ret;
        }
        static public surface_item get_surface_item_by_grade_surface(classify c, string grade, string surface)
        {
            surface_item ret = null;
            foreach (grade_item f in c.grades)
            {
                if (string.Compare(f.grade, grade) == 0)
                {
                    foreach(surface_item s in f.surface)
                    {
                        if(string.Compare(s.surface, surface)==0)
                        {
                            ret = s;
                            break;
                        }
                    }
                }
                if (ret != null)
                    break;
            }
            return ret;
        }
        #endregion
    }
}
