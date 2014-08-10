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
        public List<Bar> Bars = new List<Bar>();
        public List<Voice> Voices = new List<Voice>();
        public List<Beat> Beats = new List<Beat>();
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

    [XmlType]
    public class Score
    {
        public CData Title = new CData();
        public CData Artist = new CData();
        public CData Album = new CData();
    }

    [XmlType]
    public class MasterTrack
    {
        [XmlIgnore]
        public List<int> Tracks = new List<int>();

        public List<Automation> Automations = new List<Automation>();

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

    [XmlType]
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

    public class Property
    {
        [XmlAttribute("name")]
        public string Name;  // for notes: "String", "Fret", "Slide", "HopoOrigin", "HopoDestination"
        public int? String;  // for notes
        public int? Fret;  // for notes
        public int? Flags;  // used in slides
        public string Direction = null;  // "Up" or "Down", used in "Brush"
        public string HType = null;  // harmonics type
        public string HFret = null;  // harmonics fret

        [XmlIgnore]
        public List<int> Pitches;

        [XmlElement("Pitches")]
        public string PitchesString
        {
            get
            {
                if (Pitches == null)
                    return null;
                else
                    return string.Join(" ", Pitches);
            }
            set
            {
                Pitches = value.Split(new Char[] { ' ' }).Select(n => int.Parse(n)).ToList();
            }
        }
        
        public class EnableType
        { }

        public EnableType @Enable;  // initialize this for HopoOrigin or HopoDestination

        // hints to the XML serializer to keep output clean
        public bool ShouldSerializeString() { return String.HasValue; }
        public bool ShouldSerializeFret() { return Fret.HasValue; }
        public bool ShouldSerializeFlags() { return Flags.HasValue; }
        public bool ShouldSerializeDirection() { return Direction != null; }
        public bool ShouldSerializePitchesString() { return PitchesString != null; }
        public bool ShouldSerializeHType() { return HType != null; }
        public bool ShouldSerializeHFret() { return HFret != null; }

        public bool Equals(Property other)
        {
            if (other == null)
                return false;
            bool result = (Name == other.Name) && (String == other.String) && (Fret == other.Fret)
                && (Flags == other.Flags) && (Direction == other.Direction);
            if (Pitches != null && other.Pitches != null)
                result = result && Enumerable.SequenceEqual(Pitches, other.Pitches);
            else
                result = result && Pitches == other.Pitches;
            return result;
        }
    }

    public class Track
    {
        [XmlAttribute("id")]
        public int Id;
        public CData Name;
        public CData ShortName;
        public Instrument @Instrument;
        public GeneralMidi @GeneralMidi = new GeneralMidi();
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
        public int[] Voices = new int[] { -1, -1, -1, -1 };

        [XmlElement("Voices")]
        public string VoicesString
        {
            get
            {
                return string.Join(" ", Voices);
            }
            set
            {
                Voices = value.Split(new Char[] { ' ' }).Select(n => int.Parse(n)).ToArray();
            }
        }

        public bool Equals(Bar other)
        {
            return other != null && Clef == other.Clef && Enumerable.SequenceEqual(Voices, other.Voices);
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

        public bool Equals(Voice other)
        {
            return other != null && Enumerable.SequenceEqual(Beats, other.Beats);
        }
    }

    public class Beat
    {
        [XmlAttribute("id")]
        public int Id;
        public string Bank = null;  // e.g. "Strat-Guitar"
        public string Dynamic = "MF";
        public string Tremolo = null;

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

        public List<Property> Properties;

        public bool Equals(Beat other)
        {
            if (other == null)
                return false;
            bool result = (Bank == other.Bank) && (Dynamic == other.Dynamic) 
                && (Rhythm.Ref == other.Rhythm.Ref) && Enumerable.SequenceEqual(Notes, other.Notes);
            if (Properties != null && other.Properties != null)
                result = result && Enumerable.SequenceEqual(Properties, other.Properties);
            else
                result = result && Properties == other.Properties;
            return result;
        }

        public bool ShouldSerializeBank() { return Bank != null; }
        public bool ShouldSerializeTremolo() { return Tremolo != null; }
    }

    public class Note
    {
        [XmlAttribute("id")]
        public int Id;
        public int? Accent;
        public string Vibrato; // "Slight" or "Wide"

        public class TieType
        {
            [XmlAttribute("origin")]
            public bool Origin;
            [XmlAttribute("destination")]
            public bool Destination;

            public bool Equals(TieType other)
            {
                return (other != null) && (Origin == other.Origin) && (Destination == other.Destination);
            }
        }

        public TieType Tie;

        public List<Property> Properties = new List<Property>();

        public bool Equals(Note other)
        {
            return (other != null) && (Vibrato == other.Vibrato) && (Tie == other.Tie) 
                && Enumerable.SequenceEqual(Properties, other.Properties);
        }

        public bool ShouldSerializeAccent() { return Accent != null; }
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

            public bool Equals(Tuplet other)
            {
                return other != null && Num == other.Num && Den == other.Den;
            }
        }
        public Tuplet PrimaryTuplet;

        public class Dot
        {
            [XmlAttribute("count")]
            public int Count;

            public bool Equals(Dot other)
            {
                return other != null && Count == other.Count;
            }
        }
        public Dot AugmentationDot;

        public bool Equals(Rhythm other)
        {
            return (other != null) && (NoteValue == other.NoteValue) 
                && (PrimaryTuplet == other.PrimaryTuplet) && (AugmentationDot == other.AugmentationDot);
        }
    }


    /// <summary>
    /// Helper class to deal with some of the text information needing to be CDATA text.
    /// Taken from http://stackoverflow.com/questions/1379888/how-do-you-serialize-a-string-as-cdata-using-xmlserializer
    /// </summary>
    public class CData : IXmlSerializable
    {
        private string _value;

        /// <summary>
        /// Allow direct assignment from string:
        /// CData cdata = "abc";
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator CData(string value)
        {
            return new CData(value);
        }

        /// <summary>
        /// Allow direct assigment to string
        /// string str = cdata;
        /// </summary>
        /// <param name="cdata"></param>
        /// <returns></returns>
        public static implicit operator string(CData cdata)
        {
            return cdata._value;
        }

        public CData()
            : this(string.Empty)
        {
        }

        public CData(string value)
        {
            _value = value;
        }

        public override string ToString()
        {
            return _value;
        }

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            _value = reader.ReadElementString();
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteCData(_value);
        }
    }
}
