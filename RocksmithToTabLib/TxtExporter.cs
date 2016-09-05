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
                yield return line;

            foreach (var line in this.GetTrackHeaderLines(track))
                yield return line;

            foreach (var line in this.GetTrackChordLines(track))
                yield return line;

            foreach (var line in this.GetTrackTabLines(track))
                yield return line;
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

            var names = "   "; // width of chord string prompt
            var strings = (from n in GetStringNotePrefix(track)
                           select string.Format("{0}: ", n)).ToArray();
            var chordWidth = track.ChordTemplates.Max(kvp => kvp.Value.Name.Length);
            if (chordWidth == 0)
                yield break;

            foreach (var kvp in track.ChordTemplates)
            {
                var chordId = kvp.Key;
                var chord = kvp.Value;

                if (string.IsNullOrWhiteSpace(chord.Name))
                    continue;

                if (!usedChords.Contains(chordId))
                    continue;

                var nameSpacer = new string(' ', chordWidth - chord.Name.Length);
                names += string.Format("{0}{1}", nameSpacer, chord.Name);

                var frets = chord.Frets.Reverse().ToArray();
                for (var stringIndex = 0; stringIndex < strings.Length; stringIndex++)
                {
                    var f = frets[stringIndex];
                    var s = f == -1 ? "-" : f.ToString();
                    var spacer = chordWidth - s.Length;
                    if (spacer < 0) spacer = 0;
                    strings[stringIndex] += string.Format("{0}{1}", new string(' ', spacer), s);
                }
            }

            yield return names;
            foreach (var line in strings)
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

            foreach (var b in thisNote.BendValues)
                f += string.Format("b{0}", thisNote.Fret + b.Step);

            if (thisNote.PalmMuted)
                f = string.Format("({0})", f);

            switch (thisNote.Slide)
            {
                case Note.SlideType.ToNext:
                    f += nextNote.Fret > thisNote.Fret ? "/" : "\\";
                    linkNext = true;
                    break;

                case Note.SlideType.UnpitchUp:
                    f = "/" + f;
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
            Func<string[]> newLines = () => GetStringNotePrefix(track).Select(n => n + "|-").ToArray();

            var finalLines = newLines();

            Chord lastChord = null;

            bool linkNext = false;

            for (var barIndex = 0; barIndex < track.Bars.Count; barIndex++)
            {
                var lines = Enumerable.Range(0, track.NumStrings).Select(x => "|-").ToArray();

                var bar = track.Bars[barIndex];

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
                    var width = notes.Max(t => t.Text.Length);
                    if (!linkNext)
                        width += 1;

                    for (int x = 0; x < lines.Length; x++)
                    {
                        var spacer = width - notes[x].Text.Length;
                        lines[x] += string.Format("{0}{1}", new string('-', spacer), notes[x].Text);
                    }

                    linkNext = notes.Any(t => t.Linked);
                    lastChord = thisChord;
                }

                if (finalLines[0].Length + lines[0].Length > MaxLine)
                {
                    foreach (var line in finalLines)
                        yield return line + "-|";
                    yield return string.Empty;
                    finalLines = newLines();
                }

                for (int x = 0; x < lines.Length; x++)
                    finalLines[x] += lines[x];
            }

            foreach (var line in finalLines)
                yield return line + "-|";
        }
    }
}
