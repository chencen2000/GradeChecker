using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace GradeChecker
{
    class standard
    {
        XmlDocument _spec = null;
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
            try
            {

            }
            catch (Exception) { }
        }
        public static standard LoadSpec(string spec)
        {
            standard ret = null;
            //string s = @"data\classify.xml";
            if (!string.IsNullOrEmpty(spec))
            {
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
            XmlNodeList grades = get_all_grade();
            foreach(XmlNode n in grades)
            {
                if(meet_grade(n, device_flaw))
                {
                    // pass 
                    ret = n["name"]?.InnerText;
                    Program.logIt($"Grade {ret}");
                    break;
                }
            }
            return ret;
        }
        bool meet_grade(XmlNode grade, flaw device_flas)
        {
            bool ret = false;
            Program.logIt($"meet_grade: Grade {grade["name"]?.InnerText}");
            if (grade["max_flaws"] != null)
            {
                int max_flaws = 0;
                if (Int32.TryParse(grade["max_flaws"]?.InnerText, out max_flaws))
                {
                    int j = device_flas.count_total_flaws();
                    //int j = device_flas.Flaws.Count;                    
                    if (max_flaws < j)
                    {
                        Program.logIt($"Fail to meet max_flaws condition. ({j}>{grade["max_flaws"]?.InnerText})");
                        goto exit;
                    }
                }
            }

            if (grade["surface_grade"] != null)
            {
                foreach(XmlNode n in grade["surface_grade"].ChildNodes)
                {
                    if (!meet_surface_grade(n, device_flas))
                        goto exit;
                }
            }
            ret = true;
            exit:
            Program.logIt($"meet_grade: -- ret={ret}");
            return ret;
        }
        bool meet_surface_grade(XmlNode node, flaw device_flas)
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
                    j = device_flas.count_total_flaws_by_surface(surface);
                    if (j > i)
                    {
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
                    foreach (KeyValuePair<string, int> kvp in device_flas.Counts)
                    {
                        if (Regex.Match(kvp.Key, $@"^.*-{surface}-Major$").Success)
                        {
                            j += kvp.Value;
                        }
                    }
                    if (j > i)
                    {
                        Program.logIt($"meet_surface_grade: Failed, due to max_major_flaws={j} (max: {i})");
                        goto exit;
                    }
                }
            }
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
                            Program.logIt($"meet_surface_grade: Failed, due to max_region_flaws={j} (max: {i})");
                            goto exit;
                        }
                    }
                }
            }
            // check counts;
            foreach (XmlNode n in node["flaw_allow"]?.ChildNodes)
            {
                string name = n["flaw"]?.InnerText;
                string value = n["allow"]?.InnerText;
                if (device_flas.Counts.ContainsKey(name))
                {
                    int i;
                    if(Int32.TryParse(value, out i))
                    {
                        if (device_flas.Counts[name] <= i)
                        {
                            // pass
                        }
                        else
                        {
                            Program.logIt($"meet_surface_grade: Failed, due to {name}={device_flas.Counts[name]} (max: {value})");
                            goto exit;
                        }
                    }
                }
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
        
    }
}
