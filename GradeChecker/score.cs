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
        Dictionary<string, double> _scores = new Dictionary<string, double>();
        XmlDocument _doc;
        public score()
        {
            try
            {
                _doc = new XmlDocument();
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
    }
}
