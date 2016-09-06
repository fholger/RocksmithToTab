using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RocksmithToTabLib
{
    public class TxtExporter
    {
        const int MaxLine = 120;
        static readonly string[] noteLetters = { "C", "Db", "D", "Eb", "E", "F", "Gb", "G", "Ab", "A", "Bb", "B" };

        public void ExportTxt(Score score, string fileName)
        {
            var dirPath = Path.GetDirectoryName(fileName);
            var baseName = Path.GetFileNameWithoutExtension(fileName);
            var extName = Path.GetExtension(fileName);

            foreach (var track in score.Tracks)
            {
                var trackFileName = string.Format(@"{0}\{1} ({2}){3}",
                    dirPath, baseName, track.Name, extName);
                File.WriteAllLines(trackFileName, this.GetLines(score, track));
            }
        }

        private IEnumerable<string> GetLines(Score score, Track track)
        {
            foreach (var line in this.GetMetadataLines(score))
                yield return line.TrimEnd();

            foreach (var line in this.GetTrackHeaderLines(track))
                yield return line.TrimEnd();

            foreach (var line in this.GetTrackChordLines(track))
                yield return line.TrimEnd();

            foreach (var line in this.GetTrackTabLines(track))
                yield return line.TrimEnd();
        }

        private IEnumerable<string> GetMetadataLines(Score score)
        {
            if (!string.IsNullOrWhiteSpace(score.Title))
                yield return string.Format("Title: {0}", score.Title);

            if (!string.IsNullOrWhiteSpace(score.Artist))
                yield return string.Format("Artist: {0}", score.Artist);

            if (!string.IsNullOrWhiteSpace(score.Album))
                yield return string.Format("Album: {0}", score.Album);

            yield return string.Empty;

            foreach (var comment in score.Comments)
                yield return comment;
        }

        private IEnumerable<string> GetTrackChordLines(Track track)
        {
            var usedChords = (from bar in track.Bars
                              from chord in bar.Chords
                              where chord.ChordId != -1
                              select chord.ChordId).Distinct();
            if (usedChords.Count() == 0)
                yield break;

            var maxNameWidth = track.ChordTemplates.Max(kvp => kvp.Value.Name.Length);
            if (maxNameWidth == 0)
                yield break; // don't print chords if none of them have names

            var maxFret = track.ChordTemplates.SelectMany(ct => ct.Value.Frets).Max().ToString().Length;
            var maxWidth = Math.Max(maxFret, maxNameWidth);
            

            var generator = new TabGenerator(this.GetStringNotePrefix(track), ": ", " ", string.Empty);
            foreach (var kvp in track.ChordTemplates)
            {
                var chordId = kvp.Key;
                var chord = kvp.Value;

                if (string.IsNullOrWhiteSpace(chord.Name))
                    continue;

                if (!usedChords.Contains(chordId))
                    continue;

                var nameSpacer = new string(' ', maxWidth - chord.Name.Length);
                var name = string.Format("{0}{1}", nameSpacer, chord.Name);

                var frets = chord.Frets.Reverse().ToArray();
                var strings = new string[track.NumStrings];
                for (var stringIndex = 0; stringIndex < strings.Length; stringIndex++)
                {
                    var f = frets[stringIndex];
                    var s = f == -1 ? "-" : f.ToString();
                    var spacer = maxWidth - s.Length;
                    if (spacer < 0)
                        spacer = 0;
                    strings[stringIndex] = string.Format("{0}{1}", new string(' ', spacer), s);
                }
                generator.AddBar(name, strings);
            }

            foreach (var line in generator.GetRows(MaxLine))
                yield return line;

            yield return string.Empty;
        }

        private string[] GetStringNotePrefix(Track track)
        {
            var tuning = track.Tuning.ToArray();
            if (tuning.Length > track.NumStrings)
                tuning = tuning.Skip(tuning.Length - track.NumStrings).ToArray();

            var notes = new List<string>();

            var maxWidth = 0;
            for (var index = 0; index < tuning.Length; index++)
            {
                var tune = tuning[index];
                var noteIndex = tune % 12;
                var note = noteLetters[noteIndex];
                if (index == tuning.Length - 1 && track.Instrument == Track.InstrumentType.Guitar)
                    note = note.ToLower();

                notes.Add(note);
                maxWidth = Math.Max(maxWidth, note.Length);
            }

            return (from note in Enumerable.Reverse(notes)
                    let spacer = maxWidth - note.Length
                    select string.Format("{0}{1}", new string(' ', spacer), note)).ToArray();
        }

        private IEnumerable<string> GetTrackHeaderLines(Track track)
        {
            yield return string.Format("===== {0} =====", track.Name);

            if (track.Capo > 0)
                yield return string.Format("Capo on fret {0}", track.Capo);

            yield return string.Empty;
        }

        private NoteText ToText(Note prevNote, Note thisNote, Note nextNote)
        {
            var linkNext = false;
            if (thisNote == null)
                return new NoteText { Text = "-" };

            var f = thisNote.Fret.ToString();

            float? last = thisNote.Fret;
            foreach (var b in thisNote.BendValues)
            {
                var pos = thisNote.Fret + b.Step;
                if (last.HasValue && last.Value == pos)
                    continue;

                string s = last.HasValue && pos < last ? "r" : "b";
                f += string.Format("{0}{1}", s, pos);
                last = pos;
            }

            if (thisNote.PalmMuted)
                f = string.Format("({0})", f);

            if (thisNote.Tapped)
                f += "t";

            if (thisNote.Harmonic || thisNote.PinchHarmonic)
                f = string.Format("<{0}>", f);

            switch (thisNote.Slide)
            {
                case Note.SlideType.ToNext:
                    f += nextNote.Fret > thisNote.Fret ? "/" : "\\";
                    linkNext = true;
                    break;

                case Note.SlideType.UnpitchUp:
                    f += "/";
                    break;

                case Note.SlideType.UnpitchDown:
                    f += "\\";
                    break;
            }

            if (thisNote.Hopo)
            {
                f += nextNote == null || nextNote.Fret > thisNote.Fret ? "h" : "p";
                linkNext = true;
            }
            return new NoteText { Text = f, Linked = linkNext };
        }

        class NoteText
        {
            public string Text { get; set; }
            public bool Linked { get; set; }
        }

        private static T TryGet<T>(Dictionary<int, T> dict, int key)
        {
            if (dict == null)
                return default(T);

            T note = default(T);
            dict.TryGetValue(key, out note);
            return note;
        }

        private IEnumerable<string> GetTrackTabLines(Track track)
        {
            var generator = new TabGenerator(GetStringNotePrefix(track), "|-", "-|-", "-|");

            Chord lastChord = null;

            bool linkNext = false;

            for (var barIndex = 0; barIndex < track.Bars.Count; barIndex++)
            {
                var lines = Enumerable.Range(0, track.NumStrings).Select(x => string.Empty).ToArray();

                var bar = track.Bars[barIndex];
                var header = string.Empty;
                var lastHeader = string.Empty;

                // print out each note
                for (int chordIndex = 0; chordIndex < bar.Chords.Count; chordIndex++)
                {
                    var thisChord = bar.Chords[chordIndex];
                    var nextChord =
                        chordIndex + 1 < bar.Chords.Count ? bar.Chords[chordIndex + 1] :
                        barIndex + 1 < track.Bars.Count ? track.Bars[barIndex + 1].Chords[0] :
                        null;

                    var notes = (from stringIndex in Enumerable.Range(0, track.NumStrings).Reverse()
                                 let lastNote = lastChord == null ? null : TryGet(lastChord.Notes, stringIndex)
                                 let thisNote = TryGet(thisChord.Notes, stringIndex)
                                 let nextNote = nextChord == null ? null : TryGet(nextChord.Notes, stringIndex)
                                 select ToText(lastNote, thisNote, nextNote)).ToArray();
                    var chordHeader = thisChord.ChordId == -1 ? string.Empty : track.ChordTemplates[thisChord.ChordId].Name;
                    var width = notes.Max(t => t.Text.Length);
                    if (!linkNext)
                        width += 1;

                    if (chordHeader != lastHeader)
                    {
                        var lineLength = lines[0].Length;
                        if (lineLength < header.Length)
                            lines = lines.Select(l => l += new string('-', header.Length - lineLength)).ToArray();
                        else if (lineLength > header.Length)
                            header += new string(' ', lineLength - header.Length);
                            
                        header += chordHeader + " ";
                        lastHeader = chordHeader;
                    }

                    for (int x = 0; x < lines.Length; x++)
                    {
                        var spacer = width - notes[x].Text.Length;
                        lines[x] += string.Format("{0}{1}", new string('-', spacer), notes[x].Text);
                    }

                    linkNext = notes.Any(t => t.Linked);
                    lastChord = thisChord;
                }

                if (header.Length < lines[0].Length)
                    header += new string(' ', lines[0].Length - header.Length);
                else if (header.Length > lines[0].Length)
                    lines = lines.Select(l => l += new string('-', header.Length - lines[0].Length)).ToArray();
                generator.AddBar(header, lines);
            }

            foreach (var line in generator.GetRows(MaxLine))
                yield return line;
        }
    }

    class TabGenerator
    {
        public string[] Strings { get; private set; }
        public List<string[]> Bars { get; private set; }
        public List<string> Headers { get; private set; }

        public string Prefix { get; set; }
        public string Connector { get; set; }
        public string Suffix { get; set; }

        public TabGenerator(string[] strings, string prefix, string connector, string suffix)
        {
            this.Prefix = prefix;
            this.Connector = connector;
            this.Suffix = suffix;

            this.Strings = strings;
            this.Bars = new List<string[]>();
            this.Headers = new List<string>();
        }

        public void AddBar(string header, string[] strings)
        {
            if (header.Length != strings[0].Length)
                throw new ArgumentOutOfRangeException(
                    "header", header.Length, "Header length does not match line length");

            if (strings.Length != this.Strings.Length)
                throw new ArgumentOutOfRangeException(
                    "strings", strings.Length, 
                    "Notes should have a length of " + this.Strings.Length);

            var firstLength = strings[0].Length;
            var allSameLength = strings.Skip(1).All(n => n.Length == firstLength);
            if (!allSameLength)
                throw new ArgumentOutOfRangeException(
                    "strings", string.Join("  || ", strings),
                    "All strings were not the same length");

            this.Headers.Add(header);
            this.Bars.Add(strings);
        }

        public IEnumerable<string> GetRows(int maxLength)
        {
            var prefixLength = this.Strings[0].Length + this.Prefix.Length; // add one for the first "|"
            var suffixLength = this.Suffix.Length;
            var bars = new List<string[]>();
            var headers = new List<string>();

            for (var index = 0; index < this.Bars.Count; index++)
            {
                var bar = this.Bars[index];
                var header = this.Headers[index];

                bars.Add(bar);
                headers.Add(header);

                var noteLength = bars.Sum(b => b[0].Length);                
                var connectorLength = (bars.Count - 1) * this.Connector.Length;
                var totalLength = prefixLength + noteLength + suffixLength + connectorLength;
                if (totalLength <= maxLength)
                    continue;

                headers.Remove(header);
                bars.Remove(bar);
                var headerSpacer = new string(' ', prefixLength);
                var headerJoin = new string(' ', this.Connector.Length);
                yield return string.Format("{0}{1}", headerSpacer, string.Join(headerJoin, headers));
                foreach (var line in this.MakeRow(bars))
                    yield return line;

                bars.Clear();
                headers.Clear();
            }
        }

        private IEnumerable<string> MakeRow(IEnumerable<string[]> bars)
        {
            for (var stringIndex = 0; stringIndex < this.Strings.Length; stringIndex++)
            {
                var line = string.Join(this.Connector, bars.Select(bar => bar[stringIndex]));

                yield return string.Format(
                    "{0}{2}{1}{3}", 
                    this.Strings[stringIndex], line, 
                    this.Prefix, this.Suffix);
            }
        }
    }
}
