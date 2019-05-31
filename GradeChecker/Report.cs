using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GradeChecker
{
    class Report
    {
        public string m_IMEI;
        public string m_Model;
        public List<string> m_Flaws;
        public List<string> m_Counts;
        public List<string> m_AASurface;
        public List<string> m_ASurface;
        public List<string> m_BSurface;
        public List<string> m_CSurface;
        public List<string> m_GradeAPlusDecision;
        public List<string> m_GradeADecision;
        public List<string> m_GradeBDecision;
        public List<string> m_GradeCDecision;
        public List<string> m_GradeDPlusDecision;
        public List<string> m_GradeDDecision;
        public string m_FinalGrade;
        public string m_TargetGrade;

        public Report()
        {
            m_IMEI = string.Empty;
            m_Model = string.Empty;
            m_Flaws = new List<String>();
            m_Counts = new List<String>();
            m_AASurface = new List<String>();
            m_ASurface = new List<String>();
            m_BSurface = new List<String>();
            m_CSurface = new List<String>();
            m_GradeAPlusDecision = new List<String>();
            m_GradeADecision = new List<String>();
            m_GradeBDecision = new List<String>();
            m_GradeCDecision = new List<String>();
            m_GradeDPlusDecision = new List<String>();
            m_GradeDDecision = new List<String>();
            m_FinalGrade = string.Empty;
            m_TargetGrade = string.Empty;
        }
        public bool OutputToFile(string f_DestinationPath)
        {
            try
            {
                System.IO.FileStream t_FileStream = new System.IO.FileStream(f_DestinationPath, System.IO.FileMode.Create);
                System.IO.StreamWriter t_StreamWriter = new System.IO.StreamWriter(t_FileStream, Encoding.Default);
                t_StreamWriter.WriteLine($"IMEI = {m_IMEI}");
                t_StreamWriter.WriteLine($"Model = {m_Model}");
                t_StreamWriter.WriteLine(t_StreamWriter.NewLine);
                t_StreamWriter.WriteLine("Flaws:");
                for (int i =0; i < m_Flaws.Count;i++)
                {
                    t_StreamWriter.WriteLine($"{m_Flaws[i]}");

                }
                t_StreamWriter.WriteLine(t_StreamWriter.NewLine);

                t_StreamWriter.WriteLine("Counts:");
                for (int i = 0; i < m_Counts.Count; i++)
                {
                    t_StreamWriter.WriteLine($"{m_Counts[i]}");
                }
                t_StreamWriter.WriteLine(t_StreamWriter.NewLine);

                t_StreamWriter.WriteLine("AA Surface");
                for (int i = 0; i < m_AASurface.Count; i++)
                {
                    t_StreamWriter.WriteLine($"{m_AASurface[i]}");
                }
                t_StreamWriter.WriteLine("A Surface");
                for (int i = 0; i < m_ASurface.Count; i++)
                {
                    t_StreamWriter.WriteLine($"{m_ASurface[i]}");
                }
                t_StreamWriter.WriteLine("B Surface");
                for (int i = 0; i < m_BSurface.Count; i++)
                {
                    t_StreamWriter.WriteLine($"{m_BSurface[i]}");
                }
                t_StreamWriter.WriteLine("C Surface");
                for (int i = 0; i < m_CSurface.Count; i++)
                {
                    t_StreamWriter.WriteLine($"{m_CSurface[i]}");
                }
                t_StreamWriter.WriteLine(t_StreamWriter.NewLine);

                for (int i = 0; i < m_GradeAPlusDecision.Count; i++)
                {
                    t_StreamWriter.WriteLine($"Grade A+  Decision = {m_GradeAPlusDecision[i]}");
                }
                for (int i = 0; i < m_GradeADecision.Count; i++)
                {
                    t_StreamWriter.WriteLine($"Grade A  Decision = {m_GradeADecision[i]}");
                }
                for (int i = 0; i < m_GradeBDecision.Count; i++)
                {

                    t_StreamWriter.WriteLine($"Grade B  Decision = {m_GradeBDecision[i]}");
                }
                for (int i = 0; i < m_GradeCDecision.Count; i++)
                {
                    t_StreamWriter.WriteLine($"Grade C  Decision = {m_GradeCDecision[i]}");

                }
                for (int i = 0; i < m_GradeDPlusDecision.Count; i++)
                {
                    t_StreamWriter.WriteLine($"Grade D+  Decision = {m_GradeDPlusDecision[i]}");
                }
                for (int i = 0; i < m_GradeDDecision.Count; i++)
                {
                    t_StreamWriter.WriteLine($"Grade D  Decision = {m_GradeDDecision[i]}");
                }

                t_StreamWriter.WriteLine(t_StreamWriter.NewLine);
                t_StreamWriter.WriteLine($"FDGrade = {m_FinalGrade}");

                t_StreamWriter.WriteLine(t_StreamWriter.NewLine);
                t_StreamWriter.WriteLine($"Target Grade = {m_TargetGrade}");

                t_StreamWriter.Flush();
                t_StreamWriter.Close();
                t_FileStream.Close();
            }
            catch(Exception Ex)
            {
                return false;
            }
            return true;
        }
    }
}
