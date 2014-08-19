using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RocksmithToTabLib
{
    public class GP5File
    {
        private const string FILE_VERSION = "FICHIER GUITAR PRO v5.00";
 	    private static readonly string[] PAGE_SETUP_LINES = {
		    "%TITLE%",
		    "%SUBTITLE%",
		    "%ARTIST%",
		    "%ALBUM%",
		    "Words by %WORDS%",
		    "Music by %MUSIC%",
		    "Words & Music by %WORDSMUSIC%",
		    "Copyright %COPYRIGHT%",
		    "All Rights Reserved - International Copyright Secured",
		    "Page %N%/%P%",
    	};
        
        public static void ExportScore(Score score, string fileName)
        {
            using (var file = File.Open(fileName, FileMode.Create))
            {
                ExportScore(score, file);
            }
        }

        public static void ExportScore(Score score, Stream stream)
        {
            using (var writer = new BinaryWriter(stream))
            {
                ExportScore(score, writer);
            }

        }
             
        public static void ExportScore(Score score, BinaryWriter writer)
        {
            WriteHeader(writer);
            WriteScoreInfo(writer, score);
            WriteLyrics(writer, score);
            WritePageSetup(writer);
            WriteTempo(writer, score);
            WriteKey(writer, score);
            WriteChannels(writer, score);
            WriteMusicalDirections(writer, score);
            writer.Write((Int32)0);  // master reverb setting
            WriteMeasuresAndTracks(writer, score);
        }



        private static void WriteHeader(BinaryWriter writer)
        {
            writer.Write(FILE_VERSION);
            // need to pad version string to 30 bytes
            for (int i = FILE_VERSION.Length; i < 30; ++i)
                writer.Write((byte)0);
        }


        private static void WriteScoreInfo(BinaryWriter writer, Score score)
        {
            WriteDoublePrefixedString(writer, score.Title);
            WriteDoublePrefixedString(writer, "");  // subtitle
            WriteDoublePrefixedString(writer, score.Artist);
            WriteDoublePrefixedString(writer, score.Album);
            WriteDoublePrefixedString(writer, "");  // words by
            WriteDoublePrefixedString(writer, "");  // music by
            WriteDoublePrefixedString(writer, "");  // copyright
            WriteDoublePrefixedString(writer, "");  // tabber
            WriteDoublePrefixedString(writer, "");  // instructions
            writer.Write((Int32)1);  // number of comments, followed by comments as strings
            WriteDoublePrefixedString(writer, "");
        }


        private static void WriteLyrics(BinaryWriter writer, Score score)
        {
            // placeholder for now, just write empty data
            writer.Write((Int32)0);  // associated track
            for (int i = 0; i < 5; ++i)  // once for each lyrics line
            {
                writer.Write((Int32)0);  // starting from bar
                WriteIntPrefixedString(writer, "");  // lyrics string
            }
        }


        private static void WritePageSetup(BinaryWriter writer)
        {
            writer.Write((Int32)210);  // page width
            writer.Write((Int32)297);  // page height
            writer.Write((Int32)10);  // left margin
            writer.Write((Int32)10);  // right margin
            writer.Write((Int32)15);  // top margin
            writer.Write((Int32)10);  // bottom margin
            writer.Write((Int32)100);  // score size percentage
            writer.Write((Byte)0xff);  // view flags
            writer.Write((Byte)0x01);  // view flags

            for (int i = 0; i < PAGE_SETUP_LINES.Length; ++i)
            {
                WriteDoublePrefixedString(writer, PAGE_SETUP_LINES[i]);
            }
        }


        private static void WriteTempo(BinaryWriter writer, Score score)
        {
            // first comes a string describing the tempo of the song
            WriteDoublePrefixedString(writer, "");
            // then actual BPM
            Int32 avgBPM = (score.Tracks.Count > 0) ? (Int32)score.Tracks[0].AverageBeatsPerMinute : 120;
            writer.Write(avgBPM);
        }


        private static void WriteKey(BinaryWriter writer, Score score)
        {
            // these fields tell the key of the song. Since we don't know that, we just fill
            // them with 0
            writer.Write((Int32)0);
            writer.Write((Byte)0);
        }


        private static void WriteChannels(BinaryWriter writer, Score score)
        {
            // this sets used program and volume / effects on each channel, we just
            // use some default values
            // the first two channels are used for guitar, the other two for bass
            // the rest we don't really care about.
            WriteChannel(writer, 0x1d);
            WriteChannel(writer, 0x1d);
            WriteChannel(writer, 0x21);
            WriteChannel(writer, 0x21);
            for (int i = 4; i < 64; ++i)
            {
                WriteChannel(writer, 0);
            }
        }

        
        private static void WriteChannel(BinaryWriter writer, Int32 program)
        {
            writer.Write(program);  // program
            writer.Write((Byte)15);  // volume (from 0 to 16)
            writer.Write((Byte)8);  // pan (from 0 to 16)
            writer.Write((Byte)0);  // chorus
            writer.Write((Byte)0);  // reverb
            writer.Write((Byte)0);  // phaser
            writer.Write((Byte)0);  // tremolo
            writer.Write((Byte)0);  // unused
            writer.Write((Byte)0);  // unused
        }


        private static void WriteMusicalDirections(BinaryWriter writer, Score score)
        {
            // these tell where the musical symbols like code, fine etc. are placed
            // we are not using those, so we set them to unused (0xffff).
            for (int i = 0; i < 38; ++i)
            {
                writer.Write((Byte)0xff);
            }
        }


        private static void WriteMeasuresAndTracks(BinaryWriter writer, Score score)
        {
            // write number of measures and number of tracks
            Int32 numBars = 0;
            if (score.Tracks.Count > 0)
                numBars = score.Tracks[0].Bars.Count;
            writer.Write(numBars);
            writer.Write((Int32)score.Tracks.Count);

            if (score.Tracks.Count > 0)
            {
                WriteMasterBars(writer, score.Tracks[0].Bars);

                foreach (var track in score.Tracks)
                {
                    WriteTrack(writer, track);
                }

                // padding
                writer.Write((short)0);

                // now for the actual contents of the measures
                int currentBPM = (int)score.Tracks[0].AverageBeatsPerMinute;
                for (int b = 0; b < score.Tracks[0].Bars.Count; ++b)
                {
                    foreach (var track in score.Tracks)
                    {
                        // it can happen that an arrangement has fewer bars than the first one.
                        // This shouldn't normally happen, and as a simple hack we simply reuse the 
                        // bar of the first track instead, hoping that it will be silence.
                        // Might need a better approach if this doesn't hold.
                        var bar = (b < track.Bars.Count) ? track.Bars[b] : score.Tracks[0].Bars[b];
                        WriteBar(writer, bar, track.Instrument == Track.InstrumentType.Bass, bar.BeatsPerMinute != currentBPM);
                        writer.Write((Byte)0);  // padding
                    }
                }
            }
        }


        private static void WriteMasterBars(BinaryWriter writer, List<Bar> bars)
        {
            const Byte KEY_CHANGE = 1 << 6;
            const Byte TIME_CHANGE = (1 << 0) | (1 << 1);
            int timeNom = 0;
            int timeDenom = 0;
            for (int i = 0; i < bars.Count; ++i)
            {
                var bar = bars[i];

                if (i > 0)
                    writer.Write((Byte)0);  // padding between bars

                Byte flags = 0;
                if (bar == bars.First())
                    flags |= KEY_CHANGE;
                if (timeNom != bar.TimeNominator || timeDenom != bar.TimeDenominator)
                    flags |= TIME_CHANGE;
                timeNom = bar.TimeNominator;
                timeDenom = bar.TimeDenominator;

                writer.Write(flags);
                if ((flags & TIME_CHANGE) != 0)
                {
                    writer.Write((Byte)timeNom);
                    writer.Write((Byte)timeDenom);
                }
                if ((flags & KEY_CHANGE) != 0)
                {
                    // first bar needs to define a key signature. since we don't know that,
                    // we'll just set a default
                    writer.Write((short)0);
                }
                if ((flags & TIME_CHANGE) != 0)
                {
                    // write beam eighth notes
                    int eighthsInDenominator = 8 / timeDenom;
                    int total = eighthsInDenominator * timeNom;
                    Byte val = (Byte)(total / 4);
                    Byte missing = (Byte)(total - 4 * val);
                    Byte[] output = new Byte[] { val, val, val, val };
                    if (missing > 0)
                        output[0] += missing;

                    writer.Write(output);
                }

                writer.Write((Byte)0);  // triplet feel == NONE
                writer.Write((Byte)0);  // padding
            }
        }


        private static void WriteTrack(BinaryWriter writer, Track track)
        {
            Byte flags = 0;
            writer.Write(flags);
            writer.Write((Byte)(8 | flags));
            // track name padded to 40 bytes
            var trackName = track.Name + " Level " + track.DifficultyLevel;
            trackName = trackName.Substring(0, Math.Min(40, trackName.Length));
            writer.Write(trackName);
            for (int i = trackName.Length; i < 40; ++i)
                writer.Write((Byte)0);

            // tuning information
            int numStrings = (track.Instrument == Track.InstrumentType.Bass) ? 4 : 6;
            writer.Write(numStrings);
            for (int i = numStrings - 1; i >= 0; --i)
                writer.Write(track.Tuning[i]);
            for (int i = numStrings; i < 7; ++i)
                writer.Write((UInt32)0xffffffff);  // padding to fill up to 7 strings

            // MIDI channel information
            if (track.Instrument == Track.InstrumentType.Bass)
            {
                writer.Write((Int32)1);  // port
                writer.Write((Int32)3);  // primary channel
                writer.Write((Int32)4);  // secondary channel
            }
            else
            {
                writer.Write((Int32)1);  // port
                writer.Write((Int32)1);  // primary channel
                writer.Write((Int32)2);  // secondary channel
            }

            // number of frets, just set to 24 to be safe
            writer.Write((Int32)24);
            // capo position
            writer.Write((Int32)track.Capo);
            // track color in RGB0
            writer.Write((Byte)255);
            writer.Write((Byte)0);
            writer.Write((Byte)0);
            writer.Write((Byte)0);

            // unknown byte sequence, taken from TuxGuitar
            Byte[] fillData = new Byte[]{ 67, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 0, 0, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 255, 3, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255 };
            writer.Write(fillData);
        }


        private static void WriteBar(BinaryWriter writer, Bar bar, bool bass, bool changeTempo)
        {
            writer.Write((Int32)bar.Chords.Count);
            foreach (var chord in bar.Chords)
            {
                WriteBeat(writer, chord, bass, changeTempo && chord == bar.Chords.First());
            }

            // we also need to provide the second voice, however in our case it's going 
            // to be empty
            writer.Write((Int32)0);
        }


        private static void WriteBeat(BinaryWriter writer, Chord chord, bool bass, bool changeTempo)
        {
            const Byte DOTTED_NOTE = 1;
            const Byte CHORD_DIAGRAM = (1 << 1);
            const Byte BEAT_EFFECTS = (1 << 3);
            const Byte MIX_TABLE = (1 << 4);
            const Byte TUPLET = (1 << 5);
            const Byte REST = (1 << 6);
            const Byte VIBRATO = 1;
            const Byte NATURAL_HARMONIC = (1 << 2);
            const Byte ARTIFICIAL_HARMONIC = (1 << 3);
            const Byte STRING_EFFECTS = (1 << 5);

            // figure out beat duration
            bool dotted = false;
            bool triplet = false;
            SByte duration = 0;

            switch (chord.Duration)
            {
                case 192:
                    duration = -2;
                    break;
                case 144:
                    duration = -1;
                    dotted = true;
                    break;
                case 96:
                    duration = -1;
                    break;
                case 72:
                    duration = 0;
                    dotted = true;
                    break;
                case 48:
                    duration = 0;
                    break;
                case 36:
                    duration = 1;
                    dotted = true;
                    break;
                case 32:
                    duration = 0;
                    triplet = true;
                    break;
                case 24:
                    duration = 1;
                    break;
                case 18:
                    duration = 2;
                    dotted = true;
                    break;
                case 16:
                    duration = 1;
                    triplet = true;
                    break;
                case 12:
                    duration = 2;
                    break;
                case 9:
                    duration = 3;
                    dotted = true;
                    break;
                case 8:
                    duration = 2;
                    triplet = true;
                    break;
                case 6:
                    duration = 3;
                    break;
                case 4:
                    duration = 3;
                    triplet = true;
                    break;
                case 3:
                    duration = 4;
                    break;
                case 2:
                    duration = 4;
                    triplet = true;
                    break;
                default:
                    Console.WriteLine("  Warning: Rhythm Duration {0} not handled, defaulting to quarter note.", chord.Duration);
                    duration = 0;
                    break;
            }

            Byte flags = 0;
            if (chord.Notes.Count == 0)
                flags |= REST;
            if (dotted)
                flags |= DOTTED_NOTE;
            if (triplet)
                flags |= TUPLET;
            //if (changeTempo)
            //    flags |= MIX_TABLE;

            writer.Write(flags);
            if (chord.Notes.Count == 0)
                writer.Write((Byte)2);  // 2 is an actual rest, 0 is silent
            writer.Write(duration);
            if (triplet)
                writer.Write((Int32)3);  // declare a triplet beat

            // now write the actual notes. a flag indicates which strings are being played
            Byte stringFlags = 0;
            int stringOffset = bass ? 2 : 0;
            foreach (var kvp in chord.Notes)
                stringFlags |= (Byte)(1 << (kvp.Key+1+stringOffset));
            writer.Write(stringFlags);
            var notes = chord.Notes.Values.OrderByDescending(x => x.String);
            foreach (var note in notes)
            {
                WriteNote(writer, note);
            }

            writer.Write((short)0);  // padding
        }


        private static void WriteNote(BinaryWriter writer, Note note)
        {
            const Byte NOTE_TYPE = (1 << 4);
            const Byte NOTE_DYNAMICS = (1 << 5);
            Byte flags = NOTE_TYPE | NOTE_DYNAMICS;

            writer.Write(flags);
            writer.Write((Byte)1);  // normal note (TODO)
            writer.Write((Byte)5);  // mezzo-forte

            writer.Write((Byte)note.Fret);

            writer.Write((Byte)0);  // padding
        }




        private static void WriteIntPrefixedString(BinaryWriter writer, string text)
        {
            writer.Write((Int32)text.Length);
            writer.Write(text.ToArray());
        }
        private static void WriteDoublePrefixedString(BinaryWriter writer, string text)
        {
            // GP5 has a weird habit of doubly prefixing strings.
            if (text != null)
            {
                writer.Write((Int32)text.Length + 1);
                writer.Write(text);
            }
            else
            {
                writer.Write((Int32)1);
                writer.Write("");
            }
        }
    }
}
