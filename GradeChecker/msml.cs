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
    class msml
    {
        static string[] gradeing_label = new string[] { "A+", "A", "B", "C", "D+", "D" };
        static void Main(string[] args)
        {
            //MLContext mlContext = new MLContext();
            //DataViewSchema modelInputSchema;
            //ITransformer mlModel = mlContext.Model.Load(@"C:\Users\qa\source\repos\mlApp1\mlApp1\bin\Debug\netcoreapp2.1\MLModel.zip", out modelInputSchema);
            //var predEngine = mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(mlModel);

            //test();
            System.Collections.Specialized.StringDictionary param = new System.Collections.Specialized.StringDictionary();
            param.Add("folder", @"C:\Tools\avia\ClassifyLog");
            param.Add("spec", @"C:\Tools\avia\classify.xml");
            param.Add("output", @"C:\Tools\avia\tmp\ready_for_training.json");
            gen_train_data(param);
        }

        static void gen_train_data(System.Collections.Specialized.StringDictionary args)
        {
            List<Dictionary<string, object>> ret = new List<Dictionary<string, object>>();
            string specfn = args.ContainsKey("spec") ? args["spec"] : @"data\classify.xml";
            standard spec = standard.LoadSpec(specfn);
            Dictionary<string, Tuple<int,int>>[] device_data = load_counts_from_folder(args["folder"]);
            System.Collections.Specialized.StringDictionary[] vdata = read_verizon_data();
            string[] keys = load_all_keys();
            foreach(Dictionary<string, Tuple<int, int>> r in device_data)
            {
                System.Collections.Specialized.StringDictionary v = find_verizon_data_by_last_4_digit(vdata, $"{r["id"].Item1:D4}");
                if (v != null && v.ContainsKey("VZW"))
                {
                    string fd = "Fail";
                    try
                    {
                        fd = gradeing_label[r["id"].Item2];
                    }
                    catch (Exception) { }
                    Dictionary<string, object> d = new Dictionary<string, object>();
                    //d.Add("VZW", Array.IndexOf(gradeing_label, v["VZW"]));
                    d.Add("VZW", v["VZW"]);
                    d.Add("FD", fd);
                    foreach (string k in keys)
                    {
                        if (r.ContainsKey(k))
                        {
                            d.Add(k, r[k].Item1);
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
                string str = System.IO.File.ReadAllText(@"data\all_spec_keys.json");
                var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                ret = jss.Deserialize<List<string>>(str);
            }
            catch (Exception) { }
            return ret.ToArray();

        }
        static Dictionary<string, Tuple<int, int>>[] load_counts_from_folder(string folder)
        {
            //string folder = @"C:\Tools\avia\ClassifyLog";
            List<Dictionary<string, Tuple<int,int>>> db = new List<Dictionary<string, Tuple<int, int>>>();
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
                f.Counts.Add("id", new Tuple<int, int>( Int32.Parse(s),Array.IndexOf(gradeing_label,f.Grade)));
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
