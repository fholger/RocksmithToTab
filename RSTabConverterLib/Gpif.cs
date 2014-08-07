using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;

namespace Gpif
{
    [XmlRoot]
    public class GPIF
    {
        public string GPRevision = "11621";
        public Score @Score = new Score();
        public MasterTrack @MasterTrack = new MasterTrack();
        public List<Track> Tracks = new List<Track>();

        public void Save(string fileName)
        {
            var serializer = new XmlSerializer(typeof(GPIF));
            using (TextWriter writer = new StreamWriter(fileName))
            {
                serializer.Serialize(writer, this);
            }
        }
    }

    public class Score
    {
        public string Title;
        public string Artist;
        public string Album;
    }

    public class MasterTrack
    {
        [XmlIgnore]
        public List<uint> Tracks = new List<uint>();

        public List<Automation> Automations;

        [XmlElement("Tracks")]
        public string TracksString
        {
            get
            {
                return string.Join(" ", Tracks);
            }
            set
            {
                Tracks = value.Split(new Char[]{' '}).Select(n => uint.Parse(n)).ToList();
            }
        }
    }

    public class Automation
    {
        public string Type = "Tempo";
        public bool Linear = false;
        public int Bar;
        public int Position;
        public bool Visible = true;

        [XmlIgnore]
        public Single[] Value = new Single[2];

        [XmlElement("Value")]
        public string ValueString
        {
            get
            {
                return string.Join(" ", Value);
            }
            set
            {
                Value = value.Split(new Char[] { ' ' }).Select(n => Single.Parse(n)).ToArray();
            }
        }
    }

    public class Track
    {
        public string Name;
        public string ShortName;
        public Instrument @Instrument;
        public GeneralMidi @GeneralMidi = new GeneralMidi();

        public class Property
        {
            [XmlAttribute("name")]
            public string Name = "Tuning";
            [XmlIgnore]
            public List<int> Pitches = new List<int>();

            [XmlElement("Pitches")]
            public string PitchesString
            {
                get
                {
                    return string.Join(" ", Pitches);
                }
                set
                {
                    Pitches = value.Split(new Char[] { ' ' }).Select(n => int.Parse(n)).ToList();
                }
            }
        }

        public List<Property> Properties = new List<Property>();
    }

    public class Instrument
    {
        [XmlAttribute(AttributeName = "ref")]
        public string Ref;
    }

    public class GeneralMidi
    {
        [XmlAttribute("table")]
        public string Table = "Instrument";
        public int Program;
        public int Port;
        public int PrimaryChannel;
        public int SecondaryChannel;
        public bool ForeOneChannelPerString = false;
    }
}
