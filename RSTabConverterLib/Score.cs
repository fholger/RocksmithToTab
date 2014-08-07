using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RocksmithToolkitLib.Xml;

namespace RSTabConverterLib
{
    /// <summary>
    /// An intermediary representation of a score. The score keeps information about
    /// title, artist, etc. as well as the individual tracks contained in the score.
    /// A track in our context is a Rocksmith arrangement at a particular difficulty
    /// level.
    /// This intermediary format is primarily intended to convert the Rocksmith format
    /// into something with actual rhythmic information.
    /// </summary>
    public class Score
    {
        public Score()
        {
            Tracks = new List<Track>();
        }

        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Year { get; set; }

        public List<Track> Tracks { get; set; }
    }


    /// <summary>
    /// A single track in the score. Represents a single arrangement at a particular
    /// difficulty level. It identifies the instrument and keeps a list of bars.
    /// </summary>
    public class Track
    {
        public Track()
        {
            Bars = new List<Bar>();
            ChordTemplates = new Dictionary<int, ChordTemplate>();
        }

        public enum InstrumentType
        {
            Guitar,
            Bass,
            Vocals
        }

        public string Name { get; set; }
        public InstrumentType Instrument { get; set; }
        public List<Bar> Bars { get; set; }
        public Dictionary<int, ChordTemplate> ChordTemplates { get; set; }

        public Single AverageBeatsPerMinute { get; set; }
    }


    /// <summary>
    /// A chord template. Used to generate chord diagrams and also to determine
    /// the actual notes in a chord referencing this template.
    /// </summary>
    public class ChordTemplate
    {
        public string Name { get; set; }
        public int[] Frets { get; set; }
        public int[] Fingers { get; set; }
        public int ChordId { get; set; }
    }


    /// <summary>
    /// A single bar in a track. Specifies time and tempo and contains the actual 
    /// notes in the bar.
    /// </summary>
    public class Bar
    {
        public Bar()
        {
            Chords = new List<Chord>();
        }

        public int BeatsPerMinute { get; set; }
        public int TimeDenominator { get; set; }
        public int TimeNominator { get; set; }

        public List<Chord> Chords { get; set; }

        // start and end times in Rocksmith
        public Single Start { get; set; }
        public Single End { get; set; }

        public bool ContainsTime(Single time)
        {
            return Start <= time && time < End;
        }

        /// <summary>
        /// Approximates the given absolute time length in terms of a note value.
        /// The note duration is represented as an int in multiples of
        /// 1/48 of a quarter note.
        /// </summary>
        public int GetDuration(Single start, Single length)
        {
            Single quarterNoteLength = (End - Start) / TimeNominator * TimeDenominator / 4;
            return (int) Math.Round(length / quarterNoteLength * 48);
        }

        public Single GetDurationLength(Single start, int duration)
        {
            Single quarterNoteLength = (End - Start) / TimeNominator * TimeDenominator / 4;
            return duration / 48.0f * quarterNoteLength;
        }

        /// <summary>
        /// Requires that Start, End and TimeNominator have been set. Will try to figure out a
        /// fitting TimeDenominator and BPM.
        /// </summary>
        /// <param name="averageBPM">Average BPM in the track.</param>
        public void GuessTimeAndBPM(Single averageBPM)
        {
            var length = End - Start;
            var avgTimePerBeat = length / TimeNominator;
            if (Math.Abs(averageBPM - 60.0 / avgTimePerBeat)
                < Math.Abs(averageBPM - 30.0 / avgTimePerBeat))
            {
                // we are closer to the score's average BPM if we assume each
                // beat in this measure is a quarter note long.
                TimeDenominator = 4;
            }
            else
            {
                // in this case, eighth notes are a better match.
                TimeDenominator = 8;
            }
            // these are all the possibilities we consider. anything else is just too
            // weird, I think.

            BeatsPerMinute = (int)Math.Round(4.0/TimeDenominator * 60.0 / avgTimePerBeat);

            Console.WriteLine("Found measure with {0}/{1} time and {2} BPM.", TimeNominator, TimeDenominator, BeatsPerMinute);
        }
    }


    public class Chord
    {
        public Chord()
        {
            Notes = new Dictionary<int, Note>();
            ChordId = -1;
        }

        public int ChordId { get; set; }
        // Duration is set as a multiple of 1/48th of a quarter note,
        // i.e., 48 = quarter, 24 = eighth, 16 = eighth triplet etc.
        public int Duration { get; set; }
        // index a note by its string
        public Dictionary<int, Note> Notes { get; set; }

        // start time in Rocksmith
        public Single Start { get; set; }
    }


    public class Note
    {
        public int String { get; set; }
        public int Fret { get; set; }
    }
}
