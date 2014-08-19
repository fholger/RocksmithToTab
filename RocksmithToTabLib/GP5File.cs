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
            writer.Write((Int32)0);  // number of comments, followed by comments as strings
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
            WriteDoublePrefixedString(writer, "Moderate");
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
            for (int i = 0; i < 64; ++i)
            {
                writer.Write((Int32)24);  // program
                writer.Write((Byte)13);  // volume (from 0 to 16)
                writer.Write((Byte)8);  // pan (from 0 to 16)
                writer.Write((Byte)0);  // chorus
                writer.Write((Byte)0);  // reverb
                writer.Write((Byte)0);  // phaser
                writer.Write((Byte)0);  // tremolo
                writer.Write((Byte)0);  // unused
                writer.Write((Byte)0);  // unused
            }
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
                WriteMasterBars(writer, score.Tracks[0].Bars);

            foreach (var track in score.Tracks)
                WriteTrack(writer, track);

            // padding
            writer.Write((Byte)0);
            writer.Write((Byte)0);
        }


        private static void WriteMasterBars(BinaryWriter writer, List<Bar> bars)
        {
            const Byte KEY_CHANGE = 1 << 6;
            const Byte TIME_CHANGE = (1 << 0) | (1 << 1);
            int timeNom = 0;
            int timeDenom = 0;
            foreach (var bar in bars)
            {
                if (bar != bars.First())
                {
                    // 1 byte padding in-between bars
                    writer.Write((Byte)0);
                }

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
            }
        }


        private static void WriteTrack(BinaryWriter writer, Track track)
        {
            Byte flags = 0;
            writer.Write(flags);
            writer.Write((Byte)(8 | flags));
            // track name padded to 40 bytes
            writer.Write(track.Name.Substring(0, Math.Min(40, track.Name.Length)));
            for (int i = track.Name.Length; i < 40; ++i)
                writer.Write((Byte)0);

            // tuning information
            int numStrings = (track.Instrument == Track.InstrumentType.Bass) ? 4 : 6;
            writer.Write(numStrings);
            for (int i = 0; i < numStrings; ++i)
                writer.Write(track.Tuning[i]);
            for (int i = numStrings; i < 7; ++i)
                writer.Write((UInt32)0xffffffff);  // padding to fill up to 7 strings

            // MIDI channel information
            if (track.Instrument == Track.InstrumentType.Bass)
            {
                writer.Write((Int32)0);  // port
                writer.Write((Int32)2);  // primary channel
                writer.Write((Int32)3);  // secondary channel
            }
            else
            {
                writer.Write((Int32)0);  // port
                writer.Write((Int32)0);  // primary channel
                writer.Write((Int32)1);  // secondary channel
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




        private static void WriteIntPrefixedString(BinaryWriter writer, string text)
        {
            writer.Write((Int32)text.Length);
            writer.Write(text.ToArray());
        }
        private static void WriteDoublePrefixedString(BinaryWriter writer, string text)
        {
            // GP5 has a weird habit of doubly prefixing strings.
            writer.Write((Int32)text.Length + 1);
            writer.Write(text);
        }
    }
}
