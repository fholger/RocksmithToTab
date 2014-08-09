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
        public List<MasterBar> MasterBars = new List<MasterBar>();
        public List<Note> Notes = new List<Note>();
        public List<Rhythm> Rhythms = new List<Rhythm>();

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
        public List<int> Tracks = new List<int>();

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
                Tracks = value.Split(new Char[]{' '}).Select(n => int.Parse(n)).ToList();
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
        [XmlAttribute("id")]
        public int Id;
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

    public class MasterBar
    {
        public class KeyType
        {
            public int AccidentalCount = 0;
            public string Mode = "Major"; // "Major" or "Minor"
        }

        public KeyType Key = new KeyType();
        public string Time; // written as "4/4" etc.

        [XmlIgnore]
        public List<int> Bars = new List<int>();

        [XmlElement("Bars")]
        public string BarsString
        {
            get
            {
                return string.Join(" ", Bars);
            }
            set
            {
                Bars = value.Split(new Char[] { ' ' }).Select(n => int.Parse(n)).ToList();
            }
        }
    }

    public class Bar
    {
        [XmlAttribute("id")]
        public int Id;
        public string Clef;  // "G2", "F4", ...

        [XmlIgnore]
        public List<int> Voices = new List<int>();

        [XmlElement("Voices")]
        public string VoicesString
        {
            get
            {
                return string.Join(" ", Voices);
            }
            set
            {
                Voices = value.Split(new Char[] { ' ' }).Select(n => int.Parse(n)).ToList();
            }
        }
    }

    public class Voice
    {
        [XmlAttribute("id")]
        public int Id;

        [XmlIgnore]
        public List<int> Beats = new List<int>();

        [XmlElement("Beats")]
        public string BeatsString
        {
            get
            {
                return string.Join(" ", Beats);
            }
            set
            {
                Beats = value.Split(new Char[] { ' ' }).Select(n => int.Parse(n)).ToList();
            }
        }
    }


    public class Beat
    {
        [XmlAttribute("id")]
        public int Id;
        public string Bank = null;  // e.g. "Strat-Guitar"
        public string Dynamic = "MF";

        public class RhythmType
        {
            [XmlAttribute("ref")]
            public int Ref;
        }

        public RhythmType Rhythm = new RhythmType();

        [XmlIgnore]
        public List<int> Notes = new List<int>();

        [XmlElement("Notes")]
        public string NotesString
        {
            get
            {
                return string.Join(" ", Notes);
            }
            set
            {
                Notes = value.Split(new Char[] { ' ' }).Select(n => int.Parse(n)).ToList();
            }
        }

        public class Property
        {
            [XmlAttribute("name")]
            public string Name;  // "Brush"
            public string Direction = null;  // "Up" or "Down"
        }
        public List<Property> Properties;
    }


    public class Note
    {
        [XmlAttribute("id")]
        public int Id;
        public string Vibrato; // "Slight" or "Wide"

        public class TieType
        {
            [XmlAttribute("origin")]
            public bool Origin;
            [XmlAttribute("destination")]
            public bool Destination;
        }

        public TieType Tie;

        public class Property
        {
            [XmlAttribute("name")]
            public string Name;  // "String", "Fret", "Slide", "HopoOrigin", "HopoDestination"
            public int? String;
            public int? Fret;
            public int? Flags;  // used in slides

            public class EnableType
            { }

            public EnableType @Enable;  // initialize this for HopoOrigin or HopoDestination
        }

        public List<Property> Properties = new List<Property>();
    }

    public class Rhythm
    {
        [XmlAttribute("id")]
        public int Id;
        public string NoteValue;  // "Whole", "Half", "Quarther", "Eighth", "16th", "32nd", "64th"

        public class Tuplet
        {
            [XmlAttribute("num")]
            public int Num;  // e.g. for triplets, set to 3
            [XmlAttribute("den")]
            public int Den;  // e.g. for triplets, set to 2
        }
        public Tuplet PrimaryTuplet;
    }

}
