﻿using System;
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
    public class CriteriaStatistics
    {
        public string criteria = string.Empty;

        int count_criteria;
        double count_defect;

        public CriteriaStatistics(string name)
        {
            criteria = name;

            count_criteria = 0;
            count_defect = 0.0;
        }

        public void AddDefect(double defect)
        {
            count_criteria++;
            count_defect += defect;
        }

        public double GetCriteriaAverage()
        {
            return Math.Round(count_defect / count_criteria);
        }
        public void Print(System.IO.StreamWriter writer)
        {
            writer.WriteLine($"    Criteria {criteria}: Defect average is {GetCriteriaAverage()}");
        }
    }
    public class SurfaceStatistics
    {
        public string surface = string.Empty;

        int count_award;
        double score_award;

        int court_pentaly;
        double score_pentaly;

        List<CriteriaStatistics> criteria = new List<CriteriaStatistics>();

        public SurfaceStatistics(string name)
        {
            surface = name;

            count_award = 0;
            score_award = 0.0;

            court_pentaly = 0;
            score_pentaly = 0.0;
        }
        public void AddScore(double score_a, double score_p)
        {
            count_award++;
            score_award += score_a;
            court_pentaly++;
            score_pentaly += score_p;
        }
        public void AddDefect(string name, int defect)
        {
            foreach (CriteriaStatistics cr in criteria)
            {
                if (cr.criteria.Equals(name) == true)
                {
                    cr.AddDefect(defect);
                    return;
                }
            }

            CriteriaStatistics cr1 = new CriteriaStatistics(name);
            criteria.Add(cr1);
            cr1.AddDefect(defect);
        }
        public double GetScoreAwardAverage()
        {
            return score_award / count_award;
        }

        public double GetScorePentalyAverage()
        {
            return score_pentaly / court_pentaly;
        }
        public double ChangeCritera(double allow, double defect)
        {
            double allow_new = -1.0;

            /*
             * Critera <Nick-AA-Other2> no change: defect 1 out of critera 0
             * Critera <Nick-AA-Other2> fail: Defect is 1, critera is 0
             */
            if (allow == 0 && defect == 1)
            {
                allow_new = allow + 1;
            }
            /*
             * Critera <Scratch-AA-Other1> fail: Defect is 2, critera is 1
             */
            else if (allow == 1 && defect == 2)
            {
                allow_new = allow + 1;
            }
            else
            {
                allow_new = allow;
            }

            return allow_new;
        }
        public bool TrainingGradeCritera(int grade, System.IO.StreamWriter writer)
        {
            foreach (CriteriaStatistics cr in criteria)
            {
                double value_critera = Program.m_AvgCSV[grade].m_DefectValue[cr.criteria];
                double value_defect = cr.GetCriteriaAverage();

                if (value_defect > value_critera)
                {
                    double value_new_critera = ChangeCritera(value_critera, value_defect);
                    if (value_new_critera >= 0)
                    {
                        Program.m_AvgCSV[grade].m_DefectValue[cr.criteria] = value_new_critera;
                        writer.WriteLine($"Critera <{cr.criteria}> changed: from {value_critera} to {value_new_critera}");
                    }
                    else
                    {
                        writer.WriteLine($"Critera <{cr.criteria}> no change: defect {value_defect} out of critera {value_critera}");
                    }
                }
            }

            return CheckGradeCritera(grade, writer);
        }
        public bool CheckGradeCritera(int grade, System.IO.StreamWriter writer)
        {
            bool result = true;

            foreach (CriteriaStatistics cr in criteria)
            {
                double value_critera = Program.m_AvgCSV[grade].m_DefectValue[cr.criteria];
                double value_defect = cr.GetCriteriaAverage();

                if (value_critera >= 0.0 && value_defect > value_critera)
                {
                    writer.WriteLine($"Critera <{cr.criteria}> fail: Defect is {value_defect}, critera is {value_critera}");
                    result = false;
                }
            }

            return result;
        }
        public void Reset()
        {
            count_award = 0;
            score_award = 0.0;

            court_pentaly = 0;
            score_pentaly = 0.0;

            criteria.Clear();
        }
        public void Print(System.IO.StreamWriter writer)
        {
            //writer.WriteLine($"  Surface {surface}: Score average are award={GetScoreAwardAverage()}, pentaly={GetScorePentalyAverage()}");
            writer.WriteLine($"  Surface {surface}:");

            foreach (CriteriaStatistics cr in criteria)
            {
                cr.Print(writer);
            }
        }
    }
    public class GradeStatistics
    {
        public string grade = string.Empty;

        int count_award;
        double score_award;

        int court_pentaly;
        double score_pentaly;

        List<SurfaceStatistics> surfaces = new List<SurfaceStatistics>();

        public GradeStatistics(string name)
        {
            grade = name;

            count_award = 0;
            score_award = 0.0;

            court_pentaly = 0;
            score_pentaly = 0.0;

            surfaces.Add(new SurfaceStatistics("AA"));
            surfaces.Add(new SurfaceStatistics("A"));
            surfaces.Add(new SurfaceStatistics("B"));
        }
        public int GetGradeId()
        {
            int ret = -1;

            if (grade.Equals("A+"))
                ret = 0;
            else if (grade.Equals("A"))
                ret = 1;
            else if (grade.Equals("B"))
                ret = 2;
            else if (grade.Equals("C"))
                ret = 3;
            else if (grade.Equals("D+"))
                ret = 4;
            else if (grade.Equals("D"))
                ret = 5;

            return ret;
        }
        public SurfaceStatistics GetSurface(string name)
        {
            foreach (SurfaceStatistics sf in surfaces)
            {
                if (sf.surface.Equals(name))
                    return sf;
            }

            return null;
        }
        public double GetScoreAwardAverage()
        {
            return score_award / count_award;
        }

        public double GetScorePentalyAverage()
        {
            return score_pentaly / court_pentaly;
        }
        public void AddScoreByGrade(double score_a, double score_p)
        {
            count_award ++;
            score_award += score_a;
            court_pentaly ++;
            score_pentaly += score_p;
        }
        public void AddScoreBySurface(string name, double score_a, double score_p)
        {
            GetSurface(name).AddScore(score_a, score_p);
         }
        public void AddDefectBySurface(string surf, string crit, int defect)
        {
            GetSurface(surf).AddDefect(crit, defect);
        }
        
        public double GetScoreAwardAverageBySurface(string name)
        {
            return GetSurface(name).GetScoreAwardAverage();
        }
        public double GetScorePentalyAverageBySurface(string name)
        {
            return GetSurface(name).GetScoreAwardAverage();
        }
        public bool TrainingGradeCritera(System.IO.StreamWriter writer)
        {
            bool result = true;

            foreach (SurfaceStatistics sf in surfaces)
            {
                if (!sf.TrainingGradeCritera(GetGradeId(), writer))
                    result = false;
            }

            return result;
        }
        public void Reset()
        {
            count_award = 0;
            score_award = 0.0;

            court_pentaly = 0;
            score_pentaly = 0.0;

            foreach (SurfaceStatistics sf in surfaces)
            {
                sf.Reset();
            }
        }
        public void Print(System.IO.StreamWriter writer)
        {
            //writer.WriteLine($"Grade {grade}: Score average are award={GetScoreAwardAverage()}, pentaly={GetScorePentalyAverage()}");
            writer.WriteLine($"Grade {grade}:");

            foreach (SurfaceStatistics sf in surfaces)
            {
                sf.Print(writer);
            }
        }
    }
   
    class Program
    {
        public class CSVDefectData
        {
            public string m_Grade;
            public Dictionary<string, double> m_DefectValue;
            public CSVDefectData(string f_String)
            {
                m_Grade = f_String;
                m_DefectValue = new Dictionary<string, double>();
            }

        }
        static public List<CSVDefectData> m_AvgCSV = new List<CSVDefectData>();
        static private List<CSVDefectData> ReadCSV(string f_Path)
        {
            List<CSVDefectData> t_CSV = new List<CSVDefectData>();
            t_CSV.Add(new CSVDefectData("A+"));
            t_CSV.Add(new CSVDefectData("A"));
            t_CSV.Add(new CSVDefectData("B"));
            t_CSV.Add(new CSVDefectData("C"));
            t_CSV.Add(new CSVDefectData("D+"));
            string[] CSVAll = System.IO.File.ReadAllLines(f_Path);
            for (int i = 0; i < CSVAll.Length; i++)
            {
                if (i == 0)
                {
                    continue;
                }

                string[] t_Column = (CSVAll[i].Replace("\"", "")).Split(',');
                for (int j = 0; j < t_CSV.Count; j++)
                {
                    t_CSV[j].m_DefectValue.Add(t_Column[0].Trim(), double.Parse(t_Column[j + 1].Trim()));
                }
            }
            return t_CSV;
        }

        public static List<GradeStatistics> grade_statistics = new List<GradeStatistics>();

        public static GradeStatistics GetGrade(string grade)
        {
            foreach (GradeStatistics gr in grade_statistics)
            {
                if (gr.grade.Equals(grade))
                    return gr;
            }
            return null;
        }
        public static void AddScore(string grade, string surface, double score_a, double score_p)
        {
            GradeStatistics gr = GetGrade(grade);
            gr.AddScoreBySurface(surface, score_a, score_p);
            gr.AddScoreByGrade(score_a, score_p);
        }
        public static void AddDefect(string grade, string surface, string criteria, int defect)
        {
            GradeStatistics gr = GetGrade(grade);

            if (Program.m_AvgCSV[gr.GetGradeId()].m_DefectValue.ContainsKey(criteria) == true)
                gr.AddDefectBySurface(surface, criteria, defect);
        }

        public static double GetScoreAwardAverage(string grade, string surface)
        {
            return GetGrade(grade).GetScoreAwardAverageBySurface(surface);
        }

        public static double GetScorePentalyAverage(string grade, string surface)
        {
            return GetGrade(grade).GetScorePentalyAverageBySurface(surface);
        }
        public static bool CheckGradeCriteraBySurface(string grade, string surface, System.IO.StreamWriter writer)
        {
            GradeStatistics gr = GetGrade(grade);
            return gr.GetSurface(surface).CheckGradeCritera(gr.GetGradeId(), writer);
        }
        public static bool TrainingGradeCritera(string grade, System.IO.StreamWriter writer)
        {
            GradeStatistics gr = GetGrade(grade);
            return gr.TrainingGradeCritera(writer);
        }
        public static void ResetGradeStatistics()
        {
            foreach (GradeStatistics gr in grade_statistics)
            {
                gr.Reset();
            }
        }
        public static void PrintGradeStatistics(System.IO.StreamWriter writer)
        {
            foreach (GradeStatistics gr in grade_statistics)
            {
                gr.Print(writer);
            }
        }

        public static void ChangeSpect(Dictionary<string, object> spec)
        {
            System.IO.StreamWriter t_StreamWriter1 = new System.IO.StreamWriter("GradingProcess.log", true);
            t_StreamWriter1.WriteLine("--------------------");
            t_StreamWriter1.WriteLine($"Reading standard critera parameter:");
            t_StreamWriter1.WriteLine("--------------------");

            List<Dictionary<string, object>> t_TempSpec = new List<Dictionary<string, object>>();
            foreach (KeyValuePair<string, object> kvp in spec)
            {
                Dictionary<string, object> d = (Dictionary<string, object>)kvp.Value;
                t_TempSpec.Add(new Dictionary<string, object>(d));
            }

            Dictionary<string, double> t_TempDefectValue = new Dictionary<string, double>();
            for (int i = 0; i < 5; i ++)    // from A+ to D+
            {
                t_TempDefectValue.Clear();

                foreach (KeyValuePair<string, double> item in m_AvgCSV[i].m_DefectValue)
                {
                    if (t_TempSpec[i].ContainsKey(item.Key))
                    {
                        double t_Value = double.Parse(t_TempSpec[i][item.Key].ToString());
                        t_TempDefectValue.Add(item.Key, t_Value);

                        //t_StreamWriter1.WriteLine($"Critera <{item.Key} value is {t_Value}");
                    }
                    else
                    {
                        t_TempDefectValue.Add(item.Key, -1.0);

                        //t_StreamWriter1.WriteLine($"Critera <{item.Key} does not be found, ignore it");
                    }
                }

                m_AvgCSV[i].m_DefectValue = new Dictionary<string, double>(t_TempDefectValue);
            }

            t_StreamWriter1.Close();
        }
        public static bool b_GradingMode = false;

        public static string Root = "";
        public static void logIt(string msg)
        {
            System.Diagnostics.Trace.WriteLine($"[Grading]: {msg}");
            System.Console.WriteLine(msg);
        }
        public static System.Collections.Specialized.StringDictionary[] m_VersionData;
        static void Main(string[] args)
        {
            grade_statistics.Add(new GradeStatistics("A+"));
            grade_statistics.Add(new GradeStatistics("A"));
            grade_statistics.Add(new GradeStatistics("B"));
            grade_statistics.Add(new GradeStatistics("C"));
            grade_statistics.Add(new GradeStatistics("D+"));
            grade_statistics.Add(new GradeStatistics("D"));

            System.Configuration.Install.InstallContext _args = new System.Configuration.Install.InstallContext(null, args);
            if (_args.IsParameterTrue("debug"))
            {
                System.Console.WriteLine("wait for debug, press any key to continue...");
                System.Console.ReadKey();
            }
            Root = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            System.Collections.Specialized.StringDictionary[] vdata = read_verizon_data();
            m_VersionData = vdata;
            if (_args.IsParameterTrue("test"))
            {                
                test();
            }
            else if (_args.IsParameterTrue("score"))
            {
                score_main(_args.Parameters, vdata);
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
                if (_args.Parameters.ContainsKey("CSVFile") == false)
                {
                    System.Windows.Forms.DialogResult t_Result = System.Windows.Forms.MessageBox.Show("Not Found CSV File, Do you start forced", "Inform", System.Windows.Forms.MessageBoxButtons.YesNo);
                    if (t_Result == System.Windows.Forms.DialogResult.No)
                    {
                        return;
                    }
                }
                else
                {
                    string t_CSVPath = _args.Parameters["CSVFile"];
                    if (System.IO.File.Exists(t_CSVPath) == false)
                    {
                        System.Windows.Forms.DialogResult t_Result = System.Windows.Forms.MessageBox.Show("Not Found CSV File, Do you start forced");
                        return;
                    }
                    else
                    {
                        m_AvgCSV = ReadCSV(t_CSVPath);
                    }
                }
                string specfn = _args.Parameters.ContainsKey("spec") ? _args.Parameters["spec"] : @"data\classify.xml";
                standard spec = load_spec(specfn);
                Dictionary<string, object> specs = spec.ToDictionary();
                Dictionary<string, object>[] samples = prep(_args.Parameters, vdata);


                // Looking for the best result
                Dictionary<string, int> t_Map = new Dictionary<string, int>();
                t_Map.Add("A+", 1); t_Map.Add("A", 2); t_Map.Add("B", 3); t_Map.Add("C", 4); t_Map.Add("D+", 5); t_Map.Add("D", 6); t_Map.Add("Fail", 7);

                ChangeSpect(specs);

                for (int loop = 0; loop < 1; loop++)
                {
                    //b_GradingMode = (bool)(loop % 1);

                    int total_samples = 0;
                    int total_match_vzw = 0;
                    int total_match_oe = 0;
                    int total_higher = 0;
                    int total_lower = 0;

                    System.IO.StreamWriter t_StreamWriter = new System.IO.StreamWriter("LookingMaxMatching.log", true);
                    t_StreamWriter.WriteLine("----------------");
                    t_StreamWriter.WriteLine($"The loop {loop}:");
                    t_StreamWriter.WriteLine("----------------");

                    foreach (Dictionary<string, object> s in samples)
                    {
                        System.IO.StreamWriter t_StreamWriter1 = new System.IO.StreamWriter("GradingProcess.log", true);
                        t_StreamWriter1.WriteLine("--------------------");
                        t_StreamWriter1.WriteLine($"IEMI:{s["imei"]}, Target Grade: {s["VZW"]}, FD Grade: {s["OE"]}");
                        t_StreamWriter1.WriteLine("--------------------");
                        t_StreamWriter1.Close();

                        string g = score_one_sample(s, specs);
                        s["FD"] = g;

                        //t_StreamWriter = new System.IO.StreamWriter("SurfaceScore.txt", true);

                        if (s["VZW"].Equals(g) == true)
                        {
                            total_match_vzw++;
                            //t_StreamWriter.WriteLine($"----- Adjusted grade: {g} match with VZW grade -----");
                        }
                        else if (s["OE"].Equals(g) == true)
                        {
                            total_match_oe++;
                            //t_StreamWriter.WriteLine($"***** Adjusted grade: {g} does not match VZW but OE *****");
                        }
                        else
                        {
                            //t_StreamWriter.WriteLine($"***** Adjusted grade: {g} matches no one *****");
                        }

                        if (t_Map[g] < t_Map[s["VZW"] as string])
                        {
                            total_higher++;
                        }
                        else if (t_Map[g] > t_Map[s["VZW"] as string])
                        {
                            total_lower++;
                        }

                        total_samples++;
#if false
                        t_StreamWriter.Write(t_StreamWriter.NewLine);
                        t_StreamWriter.Close();
#endif
                    }

                    float matching_rate_vzw = (float)total_match_vzw / (float)total_samples;
                    float matching_rate_oe = (float)total_match_oe / (float)total_samples;
                    float higher_rate = (float)total_higher / (float)total_samples;
                    float lower_rate = (float)total_lower / (float)total_samples;
#if false
                    System.IO.StreamWriter t_StreamWriter2 = new System.IO.StreamWriter("SurfaceScore.txt", true);
                    t_StreamWriter2.WriteLine("********************");

                    t_StreamWriter2.WriteLine($"Total samples is {total_samples}, VZW matching rate is {matching_rate_vzw}, OE matching rate is {matching_rate_oe}");
                    t_StreamWriter2.WriteLine($"Higher grade rate is {higher_rate}, lower grade rate is {lower_rate}");
                    t_StreamWriter2.WriteLine("####################");
                    t_StreamWriter2.Close();

                    summary_report(samples);
#endif
                    t_StreamWriter.WriteLine($"Total samples is {total_samples.ToString("f3")}, VZW matching rate is {matching_rate_vzw.ToString("f3")}, OE matching rate is {matching_rate_oe.ToString("f3")}");
                    t_StreamWriter.WriteLine($"Higher grade rate is {higher_rate.ToString("f3")}, lower grade rate is {lower_rate.ToString("f3")}");

                    t_StreamWriter.Close();
                }
            }
            else if (_args.IsParameterTrue("grade_no_use"))
            {
                string specfn = _args.Parameters.ContainsKey("spec") ? _args.Parameters["spec"] : @"data\classify.xml";
                standard spec = load_spec(specfn);
                Dictionary<string, object> specs = spec.ToDictionary();
                Dictionary<string, object>[] samples = prep(_args.Parameters, vdata);
                grade_samples(samples, specs);
            }
            else if (_args.IsParameterTrue("tojson"))
            {
                string fi = _args.Parameters["input"];
                string fo = _args.Parameters["output"];
                if (System.IO.Directory.Exists(fi))
                {
                    System.IO.Directory.CreateDirectory(fo);
                    string specfn = _args.Parameters.ContainsKey("spec") ? _args.Parameters["spec"] : @"data\classify.xml";
                    standard spec = load_spec(specfn);
                    Dictionary<string, object> specs = spec.ToDictionary();
                    foreach (string fn in System.IO.Directory.GetFiles(fi))
                    {
                        flaw f = new flaw(fn);
                        //f.dump();
                        Dictionary<string, object> r = f.toDictionary();
                        // save
                        try
                        {
                            var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                            string s = jss.Serialize(r);
                            System.IO.File.WriteAllText(System.IO.Path.Combine(fo, System.IO.Path.ChangeExtension(System.IO.Path.GetFileNameWithoutExtension(fn), ".json")), s);
                        }
                        catch (Exception) { }
                    }
                }
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

        public static Report m_Result = new Report();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fn">the classifyLog file path</param>
        /// <param name="specfn">the specification file path</param>
        /// <param name="detail"></param>
        /// <param name="vdata"></param>
        /// <param name="_args"></param>
        /// <returns></returns>
        static Dictionary<string, string> run_grade(string fn, string specfn, bool detail, System.Collections.Specialized.StringDictionary[] vdata, System.Collections.Specialized.StringDictionary _args)
        {
            string t_OutputFolder=string.Empty;
            if(_args.ContainsKey("ReportFolder"))
            {
                t_OutputFolder = _args["ReportFolder"];
            }
            if(t_OutputFolder == string.Empty)
            {
                t_OutputFolder = System.IO.Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Report" );
            }
            System.IO.Directory.CreateDirectory(t_OutputFolder);
            m_Result = new Report();
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
                    //vd: current device classifyLog
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
                    //read classifyLog
                    flaw f = new flaw(fn,t_Area, t_Width, t_Length);
                    f.dump();
                    // grade
                    string s = spec.grade(f);



                    System.Console.WriteLine($"Complete Grade: XPO={vd?["XPO"]}, VZW={vd?["VZW"]}, OE={f.Grade}, FD={s}");
                    System.Console.WriteLine("=======================================");
                    // save data in report
                    if(vd == null)
                    {
                        System.Windows.Forms.MessageBox.Show($"Cant Find the IMEI in Verison File{fn}, Please Ignore it.","Not Found", System.Windows.Forms.MessageBoxButtons.OK);
                        report.Add("imei", "");
                        report.Add("model", "");
                        report.Add("color", "");
                        report.Add("XPO", "");
                        report.Add("VZW", "");
                        report.Add("OE", f.Grade);
                        report.Add("FD", s);

                        return report;
                    }
                    report.Add("imei", vd?["imei"]);
                    report.Add("model", vd?["Model"]);
                    report.Add("color", vd?["Color"]);
                    report.Add("XPO", vd?["XPO"]);
                    report.Add("VZW", vd?["VZW"]);
                    report.Add("OE", f.Grade);
                    report.Add("FD", s);

                    m_Result.m_IMEI = vd["imei"];
                    m_Result.m_Model= vd["model"];
                    m_Result.m_FinalGrade = s;
                    m_Result.m_TargetGrade = vd["VZW"];
                        
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
                    string t_ReportFilePath = System.IO.Path.Combine(t_OutputFolder, vd["imei"] + ".txt");
                    m_Result.OutputToFile(t_ReportFilePath);
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
            //read a ClassifyLog and grade it
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
        static void summary_report(Dictionary<string, object>[] reports)
        {
            List<Dictionary<string, string>> ld = new List<Dictionary<string, string>>();
            foreach(Dictionary<string,object> r in reports)
            {
                ld.Add(r.ToDictionary(k => k.Key, k => k.Value?.ToString()));
            }
            summary_report(ld.ToArray());
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
                        report.Add($"Score_Count_{kvp.Key}", kvp.Value.Item2);
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
        static void score_main(System.Collections.Specialized.StringDictionary args, StringDictionary[] vdata)
        {
            string specfn = args.ContainsKey("spec") ? args["spec"] : @"data\classify.xml";
            standard spec = load_spec(specfn);
            Dictionary<string, object> specs = spec.ToDictionary();
            Dictionary<string, object>[] samples = prep(args, vdata);
            foreach (Dictionary<string, object> r in samples)
            {
                Dictionary<string, object> _score = new Dictionary<string, object>();
                int score = 0;
                foreach (KeyValuePair<string, object> kvp in r)
                {
                    if (kvp.Key.StartsWith("Score_"))
                    {
                        if (!kvp.Key.StartsWith("Score_Count"))
                        {
                            score += (int)kvp.Value;
                            _score.Add(kvp.Key, kvp.Value);
                        }
                    }
                }
                logIt($"imei={r["imei"]}, VZW={r["VZW"]}, score={score}");
                logIt(string.Join(",", _score.Select(kv => kv.Key + "=" + kv.Value).ToArray()));
            }
        }
        static void test()
        {
            string specfn = @"C:\Tools\avia\classify.xml";
            standard spec = load_spec(specfn);
            Dictionary<string, object> specs = spec.ToDictionary();
            System.Collections.Specialized.StringDictionary[] vdata = read_verizon_data();
            Dictionary<string, object>[] samples = prep(@"C:\Tools\avia\test", vdata);
            foreach(Dictionary<string,object> s in samples)
                score_one_sample(s, specs);
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
        static string score_one_sample(Dictionary<string, object> samples, Dictionary<string, object> specs)
        {
            string ret = "D";
            //string[] grade_level = new string[] { "A+", "A", "B", "C", "D+", "D" };

            string standard_result = (string)samples["VZW"];
            string reference_result = (string)samples["OE"];
            string[] grade_level = new string[] { "A+", "A", "B", "C", "D+" };
            string[] t_Surface = new string[] { "AA", "A", "B" };
            Dictionary<string, object> grades = new Dictionary<string, object>();
            string adjusted_result = "";
            string target_result = "";
            for (int l = 0; l < grade_level.Length; l ++)
            {
                if (grade_level[l].Equals(reference_result) && l > 0)
                {
                    target_result = grade_level[l - 1];
                }
            }
            //Dictionary<string, double> _score = score.Scores;
            //string[] grade_keys = GradeChecker.Properties.Resources.grade_keys.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string gl in grade_level)
            {
                Program.logIt($"Start grading: {gl}");
                Dictionary<string, double> report = new Dictionary<string, double>();
                grades.Add(gl, report);
                Dictionary<string, object> spec = (Dictionary<string, object>)specs[gl]; //look_for_spec_by_grade(gl, specs);

                double[] t_AwardScore = new double[4] { 0.0, 0.0, 0.0, 0.0 };
                double[] t_PenaltyScore = new double[4] { 0.0, 0.0, 0.0, 0.0 };
                int[] t_AwardCount = new int[4] { 0, 0, 0, 0 };
                int[] t_PenaltyCount = new int[4] { 0, 0, 0, 0 };
                int[] t_MeetCount = new int[4] { 0, 0, 0, 0 };

                Dictionary<string, Tuple< double[], double[], int[],int[], int[]>> t_SurfaceScore = new Dictionary<string,Tuple< double[], double[], int[],int[], int[]>>();
                
                if (spec != null)
                {
                    // get the spec score first;
                    Dictionary<string,Tuple<int,double>> spec_score = score.get_score_by_spec(gl, spec);
                    double grade_score = spec_score["total"].Item2;
                    Program.logIt($"The full score of grade {gl} is {spec_score["total"].Item2}");
                    List<string> grade_keys = new List<string>(spec.Keys);
                    foreach (string k in grade_keys)
                    {
                        //Program.logIt($"Defect criteria: {k}, Socre: {spec_score[k].Item2}");
#if false
                        if (samples.ContainsKey(k) && spec_score.ContainsKey(k))
                        {
                            int cnt;
                            if(Int32.TryParse(samples[k]?.ToString(), out cnt))
                            {
                                reminder = spec_score[k].Item1 - cnt;
                                //report.Add(k, reminder);

                                double key_score = -1.0 * score.get_score_by_key(k) * cnt;
                                grade_score += key_score;
                                Tuple<int, double> key_spec = spec_score[k];
                                key_score = key_spec.Item2 + key_score;
                                report.Add(k, key_score);
                                //Program.logIt($"Sample {k}={cnt}, score: {key_score}x{cnt}={key_score * cnt}");

                            }
                            else
                            {
                                reminder = spec_score[k].Item1;
                                Program.logIt($"Sample has no valid count value of {k}");
                            }
                        }
                        else
                        {
                            reminder = spec_score[k].Item1;
                            // sample not include this type of flaw
                            //Program.logIt($"Sample not include {k}");
                        }
#endif

                        int cnt = 0;
                        if (samples.ContainsKey(k) && Int32.TryParse(samples[k]?.ToString(), out cnt))
                        {
                        }
                        {
                            double v = 0;                                
                            if (spec_score[k].Item1 == 0)
                            {
                                //if (cnt == 0) v = 0; //spec_score[k].Item2;
                                //else v = (1.0 * (spec_score[k].Item1 - cnt) / Math.Max(1, spec_score[k].Item1)) * spec_score[k].Item2;
                                v = - (1.0 * cnt * spec_score[k].Item2);
                            }
                            else
                            {
                                //v = (1.0 * (spec_score[k].Item1 - cnt) / Math.Max(1, spec_score[k].Item1)) * spec_score[k].Item2;
                                v = (1.0 * spec_score[k].Item1 - cnt) / spec_score[k].Item1 * spec_score[k].Item2;
                            }
                            grade_score += v;
                            report.Add(k, v);

                            int t_Index = -1;
                            if (k.Contains("-AA-") == true)
                            {
                                t_Index = 0;
                            }
                            else if (k.Contains("-A-") == true)
                            {
                                t_Index = 1;
                            }
                            else if (k.Contains("-B-") == true)
                            {
                                t_Index = 2;
                            }
                            else if (k.Contains("-C-") == true)
                            {
                                t_Index = 3;
                            }
                            else if (k.Contains("AA-all-all"))
                            {
                                t_Index = 0;
                            }
                            else if (k.Contains("A-all-all"))
                            {
                                t_Index = 1;
                            }
                            else if (k.Contains("B-all-all"))
                            {
                                t_Index = 2;
                            }
                            else if (k.Contains("C-all-all"))
                            {
                                t_Index = 3;
                            }
                            else
                            {
                                //System.Windows.Forms.MessageBox.Show("Error grade_score");
                                continue;
                            }

                            if (v == 0)
                            {
                                t_MeetCount[t_Index]++;
                            }
                            else if(v > 0)
                            {
                                t_AwardScore[t_Index] += v * 0.65;
                                t_AwardCount[t_Index]++;
                            }
                            else 
                            {
                                t_PenaltyScore[t_Index] += v;
                                t_PenaltyCount[t_Index]++;
                            }

                            //if (gl.Equals(standard_result) == true && t_Index < t_Surface.Length)
                            if (t_Index < t_Surface.Length)
                            {
                                AddDefect(gl, t_Surface[t_Index], k, cnt);
                            }
                        }
                    }
                    t_SurfaceScore.Add(gl, new Tuple<double[], double[], int[], int[], int[]>(t_AwardScore, t_PenaltyScore, t_AwardCount, t_PenaltyCount, t_MeetCount));
#if false
                    System.IO.StreamWriter t_StreamWriter = new System.IO.StreamWriter("SurfaceScore.txt", true);
                    t_StreamWriter.WriteLine($"Analysis Grade: {gl}");

                    for (int i = 0; i < t_Surface.Length; i ++)
                    {
                        t_StreamWriter.WriteLine($"Surface {t_Surface[i]}: AwardScore: {Math.Round(t_SurfaceScore[gl].Item1[i])}, PenaltyScore: {Math.Round(t_SurfaceScore[gl].Item2[i])}, AwardCount:{t_SurfaceScore[gl].Item3[i]}, " +
                            $"PenaltyCount:{t_SurfaceScore[gl].Item4[i]}, MeetCount: {t_SurfaceScore[gl].Item5[i]}");

                        if (gl.Equals(standard_result) == true)
                        {
                            AddScore(gl, t_Surface[i], t_SurfaceScore[gl].Item1[i], t_SurfaceScore[gl].Item2[i]);
                        }
                    }
                    double apPercentage = t_SurfaceScore[gl].Item1.Sum();
                    if (t_SurfaceScore[gl].Item2.Sum() != 0)
                        apPercentage = apPercentage / t_SurfaceScore[gl].Item2.Sum();

                    double apMixTotal = t_SurfaceScore[gl].Item1.Sum() + t_SurfaceScore[gl].Item2.Sum();

                    t_StreamWriter.WriteLine($"Total: AwardTotalScore: {Math.Round(t_SurfaceScore[gl].Item1.Sum())}, PenaltyTotalScore: {Math.Round(t_SurfaceScore[gl].Item2.Sum())}, " +
                        $"Award/Penalty: {apPercentage}, Mix: {Math.Round(apMixTotal)}");
                    t_StreamWriter.Close();
#endif
#if false
                    if (string.Compare(gl, "D") == 0 || string.Compare(gl, "D+") == 0)
                        grade_score = 6400;
                    Program.logIt($"Complete grade {gl} result: score={grade_score}");
                    report.Add("score", grade_score);
#endif
                }
                //report.Add("score", score);
            }

            // Check grade level here
            adjusted_result = string.Empty;

            //System.IO.StreamWriter t_StreamWriter = new System.IO.StreamWriter("SurfaceScore.txt", true);
            System.IO.StreamWriter t_StreamWriter = new System.IO.StreamWriter("GradingProcess.log", true);
            //PrintGradeStatistics(t_StreamWriter);

            foreach (string gl in grade_level)
            {
                if (!Program.b_GradingMode)
                {
                    if (gl.Equals(standard_result) == true)
                    {
                        if (!TrainingGradeCritera(gl, t_StreamWriter))
                        {
                            t_StreamWriter.WriteLine($" --> Grade {gl}, training failed");
                        }
                        else
                        {
                            adjusted_result = gl;
                        }
                    }
                }
                else
                {
                    //if (Math.Round(t_SurfaceScore[gl].Item1.Sum()) + Math.Round(t_SurfaceScore[gl].Item2.Sum()) >= 0)
                    if (!CheckGradeCriteraBySurface(gl, "AA", t_StreamWriter))
                    {
                        t_StreamWriter.WriteLine($" --> Grade {gl}, surface AA failed");
                    }
                    else if (!CheckGradeCriteraBySurface(gl, "A", t_StreamWriter))
                    {
                        t_StreamWriter.WriteLine($" --> Grade {gl}, surface A failed");
                    }
                    else if (!CheckGradeCriteraBySurface(gl, "B", t_StreamWriter))
                    {
                        t_StreamWriter.WriteLine($" --> Grade {gl}, surface B failed");
                    }
                    else
                    {
                        adjusted_result = gl;
                        break;
                    }
                }
            }

            ResetGradeStatistics();

            if (adjusted_result == string.Empty)
            {
                t_StreamWriter.WriteLine($"Acutally failed all grades!");
                adjusted_result = reference_result;
            }

            t_StreamWriter.Close();

            return adjusted_result;
#if false
            // dump
            {
                foreach (KeyValuePair<string,object> grade in grades)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append($"Grade: {grade.Key}, ");
                    Dictionary<string, double> d = (Dictionary<string, double>) grade.Value;
                    foreach(KeyValuePair<string, double> kvp in d)
                    {
                        if(kvp.Value!=0)
                            sb.Append($"{kvp.Key}={kvp.Value}, ");
                    }
                    Program.logIt(sb.ToString());
                }
            }
            // final grade
            {
                Dictionary<string, double> result_info = null;
                string result = "D";
                foreach (KeyValuePair<string, object> grade in grades)
                {
                    Dictionary<string, double> d = (Dictionary<string, double>)grade.Value;
                    if(d.Count(x => x.Value < 0) == 0)
                    {
                        // found grade
                        result = grade.Key;
                        result_info = d;
                        break;
                    }
                }
                Program.logIt($"Pre-grade: {result}");
#if false
                int idx = Array.IndexOf(grade_level, result);
                if (idx - 1 >= 0)
                {
                    Dictionary<string, double> d = (Dictionary<string, double>)grades[grade_level[idx - 1]];
                    if (d["score"] + result_info["score"] >= 0)
                    {
                        result = grade_level[idx - 1];
                    }
                }
                Program.logIt($"Final grade: {result}");
                ret = result;
#endif
                string[] keys = grades.Keys.ToArray();
                for (int i = Math.Min(keys.Length - 1, Array.IndexOf(grade_level, result)); i > 0; i--)
                {
                    Tuple<double, double> factor = score.get_apfactor_by_grade(keys[i]);
                    Dictionary<string, double> d = (Dictionary<string, double>)grades[keys[i]];
                    Dictionary<string, double> d1 = (Dictionary<string, double>)grades[keys[i - 1]];
                    double s = d["score"];
                    if (s > 0) s *= factor.Item1;
                    if (s <= 0) s *= factor.Item2;
                    s += d1["score"];
                    d1["score"] = s;
                }
                // final
                result = "";
                foreach (KeyValuePair<string, object> grade in grades)
                {
                    Dictionary<string, double> d = (Dictionary<string, double>)grade.Value;
                    double score1 = d["score"];
                    Program.logIt($"New score for grade {grade.Key}: score={score1}");
                    if (score1 > 0 && string.IsNullOrEmpty(result))
                        result = grade.Key;
                }
                Program.logIt($"Final grade: {result}");
                ret = result;
            }
#endif
            return ret;
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
                //score_one_sample(s, spec);

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
