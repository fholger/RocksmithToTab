using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RocksmithToolkitLib.Xml;
using RocksmithToolkitLib.Sng2014HSL;


namespace RSTabConverterLib
{
    public class MusicXmlExporter
    {
        private scorepartwise score;
        private int partIndex = 0;

        public MusicXmlExporter(string title, string artist)
        {
            // write basic information
            score = new scorepartwise();
            score.version = "3.0";
            score.work = new work() { worktitle = title };
            score.identification = new identification();
            score.identification.creator.Add(new typedtext() { type = "composer", TypedValue = artist });
            score.partlist = new partlist();
        }

        public void AddArrangement(RocksmithSongConverter arrangement)
        {
            score.partlist.scorepart.Add(new scorepart() { id = "P" + partIndex, partname = new partname() { TypedValue = "P" + partIndex } });
            var part = new scorepartwise.partLocalType() { id = "P" + partIndex };
            ++partIndex;
            var measure = new scorepartwise.partLocalType.measureLocalType();
            var attr = new attributes();
            attr.staffdetails.Add(arrangement.GetTuning());
            measure.attributes.Add(attr);
            part.measure.Add(measure);
            score.part.Add(part);
        }

        public void SaveToFile(string fileName)
        {
            score.Save(fileName);
        }
    }


    public class RocksmithSongConverter
    {
        private Song2014 song;
        bool isBass;
        byte capo;
       
        public RocksmithSongConverter(Song2014 song)
        {
            this.song = song;
            isBass = song.Arrangement.ToLower() == "bass";
            capo = song.Capo;
        }


        public staffdetails GetTuning()
        {
            int lines = isBass ? 4 : 6;
            var tuning = new staffdetails();
            tuning.stafflines = lines;
            
            var noteNames = new String[] { "C", "Db", "D", "Eb", "E", "F", "Gb", "G", "Ab", "A", "Bb", "B" };

            // The tuning is given as an offset from standard tuning. We can use the
            // utility function GetMidiNote to get to the midi note of each open string,
            // then turn that into note name and octave.
            var rsTuning = song.Tuning.ToShortArray();
            for (Byte i = 0; i < lines; ++i)
            {
                int midiNote = Sng2014FileWriter.GetMidiNote(rsTuning, i, 0, isBass, capo);
                int note = midiNote % 12;
                int octave = midiNote / 12 - 1;

                tuning.stafftuning.Add(new stafftuning() { line = i + 1, tuningstep = noteNames[note], tuningoctave = octave });
            }

            return tuning;
        }
    }
}
