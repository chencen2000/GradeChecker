using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GradeChecker
{
    #region Model Data struct
    public class ModelInput
    {
        [ColumnName("VZW"), LoadColumn(0)]
        public float VZW { get; set; }


        [ColumnName("all_all_all"), LoadColumn(1)]
        public float All_all_all { get; set; }


        [ColumnName("Scratch_AA_S"), LoadColumn(2)]
        public float Scratch_AA_S { get; set; }


        [ColumnName("Scratch_AA_Other1"), LoadColumn(3)]
        public float Scratch_AA_Other1 { get; set; }


        [ColumnName("Nick_AA_S"), LoadColumn(4)]
        public float Nick_AA_S { get; set; }


        [ColumnName("Nick_AA_Other1"), LoadColumn(5)]
        public float Nick_AA_Other1 { get; set; }


        [ColumnName("Crack_AA_A_Glass"), LoadColumn(6)]
        public float Crack_AA_A_Glass { get; set; }


        [ColumnName("Discoloration_AA"), LoadColumn(7)]
        public float Discoloration_AA { get; set; }


        [ColumnName("Scratch_A_S1"), LoadColumn(8)]
        public float Scratch_A_S1 { get; set; }


        [ColumnName("Scratch_A_Other1"), LoadColumn(9)]
        public float Scratch_A_Other1 { get; set; }


        [ColumnName("Scratch_A_WearedHomeButton"), LoadColumn(10)]
        public float Scratch_A_WearedHomeButton { get; set; }


        [ColumnName("Nick_A_S"), LoadColumn(11)]
        public float Nick_A_S { get; set; }


        [ColumnName("Nick_A_Other1"), LoadColumn(12)]
        public float Nick_A_Other1 { get; set; }


        [ColumnName("Discoloration_A_S"), LoadColumn(13)]
        public float Discoloration_A_S { get; set; }


        [ColumnName("Discoloration_A_Other1"), LoadColumn(14)]
        public float Discoloration_A_Other1 { get; set; }


        [ColumnName("Scratch_B_S1"), LoadColumn(15)]
        public float Scratch_B_S1 { get; set; }


        [ColumnName("Scratch_B_Other1"), LoadColumn(16)]
        public float Scratch_B_Other1 { get; set; }


        [ColumnName("Nick_B_S"), LoadColumn(17)]
        public float Nick_B_S { get; set; }


        [ColumnName("Nick_B_Other1"), LoadColumn(18)]
        public float Nick_B_Other1 { get; set; }


        [ColumnName("Crack_B"), LoadColumn(19)]
        public float Crack_B { get; set; }


        [ColumnName("Crack_B_Other"), LoadColumn(20)]
        public float Crack_B_Other { get; set; }


        [ColumnName("Discoloration_B_Area1"), LoadColumn(21)]
        public float Discoloration_B_Area1 { get; set; }


        [ColumnName("Discoloration_B_Area2"), LoadColumn(22)]
        public float Discoloration_B_Area2 { get; set; }


        [ColumnName("Discoloration_B_Area3"), LoadColumn(23)]
        public float Discoloration_B_Area3 { get; set; }


        [ColumnName("Discoloration_B_Logo"), LoadColumn(24)]
        public float Discoloration_B_Logo { get; set; }


        [ColumnName("Discoloration_B_Rear_Cam"), LoadColumn(25)]
        public float Discoloration_B_Rear_Cam { get; set; }


        [ColumnName("Discoloration_B_Switch"), LoadColumn(26)]
        public float Discoloration_B_Switch { get; set; }


        [ColumnName("Discoloration_B_Mic"), LoadColumn(27)]
        public float Discoloration_B_Mic { get; set; }


        [ColumnName("PinDotGroup_B_10x10"), LoadColumn(28)]
        public float PinDotGroup_B_10x10 { get; set; }


        [ColumnName("PinDotGroup_B_10x40"), LoadColumn(29)]
        public float PinDotGroup_B_10x40 { get; set; }


        [ColumnName("PinDotGroup_B_Other"), LoadColumn(30)]
        public float PinDotGroup_B_Other { get; set; }


        [ColumnName("Scratch_C_S1"), LoadColumn(31)]
        public float Scratch_C_S1 { get; set; }


        [ColumnName("Scratch_C_Other1"), LoadColumn(32)]
        public float Scratch_C_Other1 { get; set; }


        [ColumnName("Nick_C_S"), LoadColumn(33)]
        public float Nick_C_S { get; set; }


        [ColumnName("Nick_C_Other1"), LoadColumn(34)]
        public float Nick_C_Other1 { get; set; }


        [ColumnName("Crack_C"), LoadColumn(35)]
        public float Crack_C { get; set; }


        [ColumnName("Crack_C_Other"), LoadColumn(36)]
        public float Crack_C_Other { get; set; }


        [ColumnName("Discoloration_C_S"), LoadColumn(37)]
        public float Discoloration_C_S { get; set; }


        [ColumnName("Discoloration_C_Other1"), LoadColumn(38)]
        public float Discoloration_C_Other1 { get; set; }


        [ColumnName("A_all_all"), LoadColumn(39)]
        public float A_all_all { get; set; }


        [ColumnName("Scratch_A_S2"), LoadColumn(40)]
        public float Scratch_A_S2 { get; set; }


        [ColumnName("Scratch_A_Other2"), LoadColumn(41)]
        public float Scratch_A_Other2 { get; set; }


        [ColumnName("B_all_all"), LoadColumn(42)]
        public float B_all_all { get; set; }


        [ColumnName("Scratch_B_S2"), LoadColumn(43)]
        public float Scratch_B_S2 { get; set; }


        [ColumnName("Scratch_B_Other2"), LoadColumn(44)]
        public float Scratch_B_Other2 { get; set; }


        [ColumnName("C_all_all"), LoadColumn(45)]
        public float C_all_all { get; set; }


        [ColumnName("Scratch_C_S2"), LoadColumn(46)]
        public float Scratch_C_S2 { get; set; }


        [ColumnName("Scratch_C_Other2"), LoadColumn(47)]
        public float Scratch_C_Other2 { get; set; }


        [ColumnName("AA_all_all"), LoadColumn(48)]
        public float AA_all_all { get; set; }


        [ColumnName("AA_major_all"), LoadColumn(49)]
        public float AA_major_all { get; set; }


        [ColumnName("AA_region_all"), LoadColumn(50)]
        public float AA_region_all { get; set; }


        [ColumnName("Scratch_AA_Minor"), LoadColumn(51)]
        public float Scratch_AA_Minor { get; set; }


        [ColumnName("Scratch_AA_Major"), LoadColumn(52)]
        public float Scratch_AA_Major { get; set; }


        [ColumnName("Scratch_AA_Other2"), LoadColumn(53)]
        public float Scratch_AA_Other2 { get; set; }


        [ColumnName("Nick_AA_Minor"), LoadColumn(54)]
        public float Nick_AA_Minor { get; set; }


        [ColumnName("Nick_AA_Major"), LoadColumn(55)]
        public float Nick_AA_Major { get; set; }


        [ColumnName("Nick_AA_Other2"), LoadColumn(56)]
        public float Nick_AA_Other2 { get; set; }


        [ColumnName("A_major_all"), LoadColumn(57)]
        public float A_major_all { get; set; }


        [ColumnName("Scratch_A_Minor"), LoadColumn(58)]
        public float Scratch_A_Minor { get; set; }


        [ColumnName("Scratch_A_Major"), LoadColumn(59)]
        public float Scratch_A_Major { get; set; }


        [ColumnName("Scratch_A_Other3"), LoadColumn(60)]
        public float Scratch_A_Other3 { get; set; }


        [ColumnName("Nick_A_Minor"), LoadColumn(61)]
        public float Nick_A_Minor { get; set; }


        [ColumnName("Nick_A_Major"), LoadColumn(62)]
        public float Nick_A_Major { get; set; }


        [ColumnName("Nick_A_Other2"), LoadColumn(63)]
        public float Nick_A_Other2 { get; set; }


        [ColumnName("Discoloration_A_Minor"), LoadColumn(64)]
        public float Discoloration_A_Minor { get; set; }


        [ColumnName("Discoloration_A_Major"), LoadColumn(65)]
        public float Discoloration_A_Major { get; set; }


        [ColumnName("Discoloration_A_Other2"), LoadColumn(66)]
        public float Discoloration_A_Other2 { get; set; }


        [ColumnName("B_major_all"), LoadColumn(67)]
        public float B_major_all { get; set; }


        [ColumnName("Scratch_B_Minor"), LoadColumn(68)]
        public float Scratch_B_Minor { get; set; }


        [ColumnName("Scratch_B_Major"), LoadColumn(69)]
        public float Scratch_B_Major { get; set; }

        [ColumnName("Scratch_B_Other3"), LoadColumn(70)]
        public float Scratch_B_Other3 { get; set; }

        [ColumnName("Nick_B_Minor"), LoadColumn(71)]
        public float Nick_B_Minor { get; set; }

        [ColumnName("Nick_B_Major"), LoadColumn(72)]
        public float Nick_B_Major { get; set; }

        [ColumnName("Nick_B_Other2"), LoadColumn(73)]
        public float Nick_B_Other2 { get; set; }

        [ColumnName("C_major_all"), LoadColumn(74)]
        public float C_major_all { get; set; }

        [ColumnName("Scratch_C_Minor"), LoadColumn(75)]
        public float Scratch_C_Minor { get; set; }

        [ColumnName("Scratch_C_Major"), LoadColumn(76)]
        public float Scratch_C_Major { get; set; }

        [ColumnName("Scratch_C_Other3"), LoadColumn(77)]
        public float Scratch_C_Other3 { get; set; }

        [ColumnName("Nick_C_Minor"), LoadColumn(78)]
        public float Nick_C_Minor { get; set; }

        [ColumnName("Nick_C_Major"), LoadColumn(79)]
        public float Nick_C_Major { get; set; }

        [ColumnName("Nick_C_Other2"), LoadColumn(80)]
        public float Nick_C_Other2 { get; set; }

        [ColumnName("Discoloration_C_Minor"), LoadColumn(81)]
        public float Discoloration_C_Minor { get; set; }

        [ColumnName("Discoloration_C_Major"), LoadColumn(82)]
        public float Discoloration_C_Major { get; set; }

        [ColumnName("Discoloration_C_Other2"), LoadColumn(83)]
        public float Discoloration_C_Other2 { get; set; }
    }
    public class ModelOutput
    {
        // ColumnName attribute is used to change the column name from
        // its default value, which is the name of the field.
        [ColumnName("PredictedLabel")]
        public Single Prediction { get; set; }
        public float[] Score { get; set; }
    }

    #endregion

    class msml
    {
        static string[] gradeing_label = new string[] { "A+", "A", "B", "C", "D+", "D" };
        static void Main(string[] args)
        {
            //MLContext mlContext = new MLContext();
            //DataViewSchema modelInputSchema;
            //ITransformer mlModel = mlContext.Model.Load(@"C:\Users\qa\source\repos\mlApp1\mlApp1\bin\Debug\netcoreapp2.1\MLModel.zip", out modelInputSchema);
            //var predEngine = mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(mlModel);

            test();
            //System.Collections.Specialized.StringDictionary param = new System.Collections.Specialized.StringDictionary();
            //param.Add("folder", @"C:\Tools\avia\ClassifyLog");
            //param.Add("output", @"C:\Tools\avia\tmp\read_for_traininf.json");
            //gen_train_data(param);
        }

        static void gen_train_data(System.Collections.Specialized.StringDictionary args)
        {
            List<Dictionary<string, object>> ret = new List<Dictionary<string, object>>();
            Dictionary<string, int>[] device_data = load_counts_from_folder(args["folder"]);
            System.Collections.Specialized.StringDictionary[] vdata = read_verizon_data();
            string[] keys = load_all_keys();
            foreach(Dictionary<string, int> r in device_data)
            {
                System.Collections.Specialized.StringDictionary v = find_verizon_data_by_last_4_digit(vdata, $"{r["id"]:D4}");
                if (v != null && v.ContainsKey("VZW"))
                {
                    Dictionary<string, object> d = new Dictionary<string, object>();
                    d.Add("VZW", Array.IndexOf(gradeing_label, v["VZW"]));
                    foreach (string k in keys)
                    {
                        if (r.ContainsKey(k))
                        {
                            d.Add(k, r[k]);
                        }
                        else
                            d.Add(k, 0);
                    }
                    ret.Add(d);
                }
            }
            try
            {
                var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                //ret = jss.Deserialize<List<Dictionary<string, object>>>(str);
                string str = jss.Serialize(ret);
                System.IO.File.WriteAllText(args["output"], str);
            }
            catch (Exception) { }
        }
        public static void predict_main(System.Collections.Specialized.StringDictionary args)
        {
            flaw f = new flaw(args["input"]);
            string root = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            System.IO.Directory.CreateDirectory(System.IO.Path.Combine(root, "tmp"));
            try
            {
                string[] keys = load_all_keys();
                Dictionary<string, object> d = new Dictionary<string, object>();
                foreach (string k in keys)
                {
                    if (f.Counts.ContainsKey(k))
                    {
                        d.Add(k, f.Counts[k]);
                    }
                    else
                        d.Add(k, 0);
                }
                var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                //ret = jss.Deserialize<List<Dictionary<string, object>>>(str);
                string str = jss.Serialize(d);
                System.IO.File.WriteAllText(System.IO.Path.Combine(root, "tmp", "test.json"), str);
                int ret;
                string[] lines = runExe("dotnet.exe", $@"mlApp1.dll predict --input {System.IO.Path.Combine(root, "tmp", "test.json")}", out ret, systemapp: true, 
                    workdir: $@"{System.IO.Path.Combine(root, "publish")}");
                System.Console.WriteLine($"{string.Join(System.Environment.NewLine, lines)}");
            }
            catch (Exception) { }
        }
        static void test()
        {
            string fn = @"C:\Tools\avia\ClassifyLog\classify-8738.txt";
            flaw f = new flaw(fn);
            try
            {
                string[] keys = load_all_keys();
                Dictionary<string, object> d = new Dictionary<string, object>();
                foreach (string k in keys)
                {
                    if (f.Counts.ContainsKey(k))
                    {
                        d.Add(k, f.Counts[k]);
                    }
                    else
                        d.Add(k, 0);
                }
                var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                //ret = jss.Deserialize<List<Dictionary<string, object>>>(str);
                string str = jss.Serialize(d);
                System.IO.File.WriteAllText(@"C:\Tools\avia\tmp\test.json", str);
                int ret;
                string[] lines = runExe("dotnet.exe", @"mlApp1.dll predict --input C:\Tools\avia\tmp\test.json", out ret, systemapp: true, workdir: @"C:\Users\qa\source\repos\mlApp1\mlApp1\bin\Debug\netcoreapp2.2\publish");
                // prepare data
                //ModelInput mi = new ModelInput();
                //foreach (PropertyInfo pi in mi.GetType().GetProperties())
                //{
                //    string name = "";
                //    foreach(var p in pi.CustomAttributes)
                //    {
                //        if (string.Compare(p.AttributeType.Name, "ColumnNameAttribute") == 0)
                //        {
                //            name = p.ConstructorArguments[0].Value?.ToString();
                //            break;
                //        }
                //    }
                //    if (string.Compare(name, "VZW")==0)
                //    {
                //        pi.SetValue(mi, Array.IndexOf(gradeing_label, "D"));
                //    }
                //    else
                //    {
                //        name = name.Replace('_', '-');
                //        if (d.ContainsKey(name))
                //        {
                //            pi.SetValue(mi, d[name]);
                //        }
                //        else
                //            pi.SetValue(mi, 0);
                //    }
                //}
                // load model
                //MLContext mlContext = new MLContext();
                //DataViewSchema modelInputSchema;
                //ITransformer mlModel = mlContext.Model.Load(@"C:\Users\qa\source\repos\mlApp1\mlApp1ML.Model\MLModel.zip", out  modelInputSchema);
                //var predEngine = mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(mlModel);
                //ModelOutput result = predEngine.Predict(mi);
            }
            catch (Exception) { }

        }

        static System.Collections.Specialized.StringDictionary find_verizon_data_by_last_4_digit(System.Collections.Specialized.StringDictionary[] vdata, string last_4_digit)
        {
            System.Collections.Specialized.StringDictionary ret = null;
            try
            {
                ret = vdata.SingleOrDefault(x => (x["IMEI"] as string).EndsWith(last_4_digit));
            }
            catch (InvalidOperationException ex)
            {
                Program.logIt($"{ex.Message}: {last_4_digit}");
            }
            catch (Exception) { }
            return ret;
        }

        //public static Dictionary<string,object>[] read_verizon_data()
        //{
        //    List<Dictionary<string, object>> ret = new List<Dictionary<string, object>>();
        //    try
        //    {
        //        //string str = System.IO.File.ReadAllText(@"C:\Tools\avia\data\verizon_db.json");
        //        var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
        //        ret = jss.Deserialize<List<Dictionary<string, object>>>(GradeChecker.Properties.Resources.verizon_data);
        //    }
        //    catch (Exception) { }
        //    return ret.ToArray();
        //}
        public static Dictionary<string,object>[] load_all_counts()
        {
            List<Dictionary<string, object>> ret = new List<Dictionary<string, object>>();
            try
            {
                string str = System.IO.File.ReadAllText(@"C:\Tools\avia\data\all_counts_270.json");
                var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                ret = jss.Deserialize<List<Dictionary<string, object>>>(str);
            }
            catch (Exception) { }
            return ret.ToArray();
        }
        public static string[] load_all_keys()
        {
            List<string> ret = new List<string>();
            try
            {
                string str = System.IO.File.ReadAllText(@"C:\Tools\avia\data\all_spec_keys.json");
                var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                ret = jss.Deserialize<List<string>>(str);
            }
            catch (Exception) { }
            return ret.ToArray();

        }
        static Dictionary<string,int>[] load_counts_from_folder(string folder)
        {
            //string folder = @"C:\Tools\avia\ClassifyLog";
            List<Dictionary<string, int>> db = new List<Dictionary<string, int>>();
            foreach (string fn in System.IO.Directory.GetFiles(folder, "*.txt"))
            {
                //flaw f = new flaw(@"C:\projects\avia\logfiles\classify-0083.txt");
                flaw f = new flaw(fn);
                f.dump();
                //f.recount();
                // _counts to json
                // get last-4-digit
                string s = System.IO.Path.GetFileNameWithoutExtension(fn);
                s = s.Substring(s.Length - 4);
                f.Counts.Add("id", Int32.Parse(s));
                db.Add(f.Counts);
            }
            return db.ToArray();
        }
        public static System.Collections.Specialized.StringDictionary[] read_verizon_data()
        {
            List<System.Collections.Specialized.StringDictionary> vdata = new List<System.Collections.Specialized.StringDictionary>();
#if true
            string[] lines = GradeChecker.Properties.Resources.verizon_data.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
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
        public static string[] runExe(string exeFilename, string param, out int exitCode, System.Collections.Specialized.StringDictionary env = null, int timeout = 180 * 1000, bool systemapp=false, string workdir="")
        {
            List<string> ret = new List<string>();
            exitCode = 1;
            //logIt(string.Format("[runExe]: ++ exe={0}, param={1}", exeFilename, param));
            try
            {
                if (systemapp||  System.IO.File.Exists(exeFilename))
                {
                    System.Threading.AutoResetEvent ev = new System.Threading.AutoResetEvent(false);
                    Process p = new Process();
                    p.StartInfo.FileName = exeFilename;
                    p.StartInfo.Arguments = param;
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.CreateNoWindow = true;
                    if(System.IO.Directory.Exists(workdir))
                        p.StartInfo.WorkingDirectory = workdir;
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
                            //logIt(string.Format("[runExe]: {0}", args.Data));
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
                //logIt(string.Format("[runExe]: {0}", ex.Message));
                //logIt(string.Format("[runExe]: {0}", ex.StackTrace));
            }
            //logIt(string.Format("[runExe]: -- ret={0}", exitCode));
            return ret.ToArray();
        }

    }
}
