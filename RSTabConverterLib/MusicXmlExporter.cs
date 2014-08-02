using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RocksmithToolkitLib.Xml;


namespace RSTabConverterLib
{
    public class MusicXmlExporter
    {
        private scorepartwise score;

        public MusicXmlExporter(string title, string artist)
        {
            // write basic information
            score = new scorepartwise();
            score.version = "3.0";
            score.work = new work() { worktitle = title };
            score.identification = new identification();
            score.identification.creator.Add(new typedtext() { type = "composer", TypedValue = artist });              
        }

        public void SaveToFile(string fileName)
        {
            score.Save(fileName);
        }
    }
}
