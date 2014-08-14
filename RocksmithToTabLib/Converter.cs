using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RocksmithToolkitLib.Xml;
using RocksmithToolkitLib.Sng2014HSL;

namespace RocksmithToTabLib
{
    public class Converter
    {
        public static Track ConvertArrangement(Song2014 arrangement, int difficultyLevel = int.MaxValue)
        {
            var track = new Track();

            track.Name = arrangement.Arrangement;
            track.AverageBeatsPerMinute = arrangement.AverageTempo;
            if (arrangement.Arrangement.ToLower() == "bass")
                track.Instrument = Track.InstrumentType.Bass;
            else
                track.Instrument = Track.InstrumentType.Guitar;
            // todo: vocals

            // get tuning
            track.Tuning = GetTuning(arrangement);
            track.Capo = arrangement.Capo;

            // get chord templates
            track.ChordTemplates = GetChordTemplates(arrangement);

            // gather measures
            track.Bars = GetBars(arrangement);

            // gather notes
            track.DifficultyLevel = CollectNotesForDifficulty(arrangement, track.Bars, track.ChordTemplates, difficultyLevel);

            // figure out note durations and clean up potentially overlapping notes
            CalculateNoteDurations(track.Bars);

            // split those note values that can't or shouldn't be represented as a single note value
            SplitNotes(track.Bars);

            return track;
        }


        static int[] GetTuning(Song2014 arrangement)
        {
            bool isBass = arrangement.Title.ToLower() == "bass";
            int[] tuning = new int[isBass ? 4 : 6];
            for (byte s = 0; s < tuning.Length; ++s)
                tuning[s] = Sng2014FileWriter.GetMidiNote(arrangement.Tuning.ToShortArray(), s, 0, isBass, 0);
            return tuning;
        }


        static Dictionary<int, ChordTemplate> GetChordTemplates(Song2014 arrangement)
        {
            var templates = new Dictionary<int, ChordTemplate>();

            for (int i = 0; i < arrangement.ChordTemplates.Length; ++i)
            {
                var rsTemplate = arrangement.ChordTemplates[i];

                var template = new ChordTemplate()
                {
                    ChordId = i,
                    Name = rsTemplate.ChordName,
                    Frets = new int[] { rsTemplate.Fret0, rsTemplate.Fret1, rsTemplate.Fret2,
                        rsTemplate.Fret3, rsTemplate.Fret4, rsTemplate.Fret5 },
                    Fingers = new int[] { rsTemplate.Finger0, rsTemplate.Finger1, 
                        rsTemplate.Finger2, rsTemplate.Finger3, rsTemplate.Finger4,
                        rsTemplate.Finger5 }
                };
                if (rsTemplate.ChordId.HasValue)
                    template.ChordId = rsTemplate.ChordId.Value;

                // correct for capo position
                for (int j = 0; j < 6; ++j)
                {
                    if (template.Frets[j] > 0)
                        template.Frets[j] -= arrangement.Capo;
                }

                if (!templates.ContainsKey(template.ChordId))
                    templates.Add(template.ChordId, template);
                else
                    Console.WriteLine("Warning: ChordId {0} already present in templates list.", template.ChordId);
            }

            return templates;
        }


        static List<Bar> GetBars(Song2014 arrangement)
        {
            var bars = new List<Bar>();
            // Rocksmith keeps a list of ebeats with sub-beats, which translate to 
            // the actual beats in the measure. We can use this to make a sensible guess of
            // the measure's time.
            Bar currentMeasure = null;
            foreach (var ebeat in arrangement.Ebeats)
            {
                if (ebeat.Measure >= 0)
                {
                    // ebeat has a positive id, meaning it's a new measure
                    if (currentMeasure != null)
                    {
                        currentMeasure.End = ebeat.Time;
                        currentMeasure.BeatTimes.Add(ebeat.Time);
                        // figure out time and tempo
                        currentMeasure.GuessTimeAndBPM(arrangement.AverageTempo);
                    }

                    currentMeasure = new Bar() { TimeNominator = 1, Start = ebeat.Time };
                    currentMeasure.BeatTimes.Add(ebeat.Time);
                    bars.Add(currentMeasure);
                }
                else
                {
                    // sub-beat. Increase current measure's time nominator
                    if (currentMeasure == null)
                    {
                        Console.WriteLine("WARNING: Encountered ebeat without id with no active measure?!");
                        // ignore for now
                    }
                    else
                    {
                        currentMeasure.TimeNominator += 1;
                        currentMeasure.BeatTimes.Add(ebeat.Time);
                    }
                }
            }

            if (bars.Count > 0)
            {
                //var lastBar = bars.Last();
                //lastBar.End = arrangement.SongLength;
                //lastBar.GuessTimeAndBPM(arrangement.AverageTempo);                

                // remove last bar, as it seems to have no actual function
                bars.RemoveAt(bars.Count - 1);
            }

            return bars;
        }


        static int CollectNotesForDifficulty(Song2014 arrangement, List<Bar> bars, Dictionary<int, ChordTemplate> chordTemplates, int difficultyLevel)
        {
            // Rocksmith keeps its notes separated by the difficulty levels. Higher difficulty
            // levels only contain notes for phrases where the notes differ from lower levels.
            // This makes collection a little awkward, as we have to go phrase by phrase, 
            // to extract all the right notes.
            int maxDifficulty = 0;
            IEnumerable<Chord> allNotes = new List<Chord>();
            for (int pit = 0; pit < arrangement.PhraseIterations.Length; ++pit)
            {
                var phraseIteration = arrangement.PhraseIterations[pit];
                var phrase = arrangement.Phrases[phraseIteration.PhraseId];
                int difficulty = Math.Min(difficultyLevel, phrase.MaxDifficulty);
                var level = arrangement.Levels.FirstOrDefault(x => x.Difficulty == difficulty);
                maxDifficulty = Math.Max(difficulty, maxDifficulty);
                float startTime = phraseIteration.Time;
                float endTime = float.MaxValue;
                if (pit < arrangement.PhraseIterations.Length - 1)
                    endTime = arrangement.PhraseIterations[pit + 1].Time;

                // gather single notes and chords inside this phrase iteration
                var notes = from n in level.Notes where n.Time >= startTime && n.Time < endTime 
                            select CreateChord(n, arrangement.Capo);
                var chords = from c in level.Chords where c.Time >= startTime && c.Time < endTime
                             select CreateChord(c, chordTemplates, arrangement.Capo);
                allNotes = allNotes.Concat(notes.Concat(chords));
            }

            for (int b = 0; b < bars.Count; ++b)
            {
                var bar = bars[b];
                float endTime = (b < bars.Count - 1) ? bars[b + 1].Start : float.MaxValue;
                // gather chords that lie within this bar
                bar.Chords = allNotes.Where(x => x.Start >= bar.Start && x.Start < endTime)
                    .OrderBy(x => x.Start).ToList();

                // in case that the bar is empty or the first note does not coincide with the start
                // of the bar, we add an empty chord to the beginning indicating silence.
                if (bar.Chords.Count == 0 || bar.Chords.First().Start > bar.Start)
                {
                    bar.Chords.Insert(0, new Chord() { Start = bar.Start });
                }
            }
            

            TransferTechniques(bars);

            return maxDifficulty;
        }


        static Chord CreateChord(SongNote2014 note, int capo)
        {
            var chord = new Chord();
            chord.Start = note.Time;
            var convertedNote = CreateNote(note, capo);
            chord.Notes.Add(note.String, convertedNote);
            chord.Tremolo = convertedNote.Tremolo;
            chord.Slapped = convertedNote.Slapped;
            chord.Popped = convertedNote.Popped;
            return chord;
        }


        static Chord CreateChord(SongChord2014 rsChord, Dictionary<int, ChordTemplate> chordTemplates, int capo)
        {
            var chord = new Chord();
            chord.Start = rsChord.Time;
            chord.ChordId = rsChord.ChordId;
            chord.Tremolo = false;
            if (rsChord.ChordNotes != null)
            {
                foreach (var note in rsChord.ChordNotes)
                {
                    chord.Notes.Add(note.String, CreateNote(note, capo));
                }
            }
            if (chordTemplates.ContainsKey(chord.ChordId))
            {
                // need to determine chords from the chord template
                var template = chordTemplates[chord.ChordId];
                for (int i = 0; i < 6; ++i)
                {
                    if (template.Frets[i] >= 0 && !chord.Notes.ContainsKey(i))
                    {
                        var note = new Note()
                        {
                            Fret = template.Frets[i],
                            String = i
                        };
                        chord.Notes.Add(i, note);
                    }
                }
            }
            if (chord.Notes.Count == 0)
            {
                Console.WriteLine("Warning: Empty chord. Cannot find chord with chordId {0}.", chord.ChordId);
            }

            // some properties set on the chord in Rocksmith need to be passed down to the individual notes
            // and vice versa
            foreach (var kvp in chord.Notes)
            {
                if (rsChord.PalmMute != 0)
                    kvp.Value.PalmMuted = true;
                if (rsChord.FretHandMute != 0)
                    kvp.Value.Muted = true;
                if (rsChord.Accent != 0)
                    kvp.Value.Accent = true;
                if (kvp.Value.Tremolo)
                    chord.Tremolo = true;
                if (kvp.Value.Slapped)
                    chord.Slapped = true;
                if (kvp.Value.Popped)
                    chord.Popped = true;
            }

            // we will show a strum hint for all chords played with an up-stroke,
            // and a down-stroke hint for all chords with more than 3 notes (to exclude power-chords)
            //if (rsChord.Strum.ToLower() == "up")
            //    chord.BrushDirection = Chord.BrushType.Up;
            //else if (chord.Notes.Count > 3 && rsChord.Strum.ToLower() == "down")
            //    chord.BrushDirection = Chord.BrushType.Down;
            // disabled, since apparently the strum hints aren't really useful. I might have
            // misunderstood the parameter.

            return chord;
        }


        static Note CreateNote(SongNote2014 rsNote, int capo)
        {
            var note = new Note()
            {
                String = rsNote.String,
                Fret = rsNote.Fret,
                PalmMuted = rsNote.PalmMute != 0,
                Muted = rsNote.Mute != 0,
                Hopo = rsNote.HammerOn != 0 || rsNote.PullOff != 0,
                Vibrato = rsNote.Vibrato > 0,
                LinkNext = rsNote.LinkNext != 0,
                Accent = rsNote.Accent != 0,
                Harmonic = rsNote.Harmonic != 0,
                PinchHarmonic = rsNote.HarmonicPinch != 0,
                Tremolo = rsNote.Tremolo != 0,
                Tapped = rsNote.Tap != 0,
                Slapped = rsNote.Slap == 1,
                Popped = rsNote.Pluck == 1,
                LeftFingering = rsNote.LeftHand
            };
            if (rsNote.SlideTo != -1)
                note.Slide = Note.SlideType.ToNext;
            else if (rsNote.SlideUnpitchTo != -1)
            {
                if (rsNote.SlideUnpitchTo > rsNote.Fret)
                    note.Slide = Note.SlideType.UnpitchUp;
                else
                    note.Slide = Note.SlideType.UnpitchDown;
            }
            //if (rsNote.Bend > 0 || rsNote.BendValues != null)
            //{
            //    Console.WriteLine("Detected note bend at time {3} on string {0}, fret {1}. Bend value: {2}", rsNote.String, rsNote.Fret, rsNote.Bend, rsNote.Time);
            //    foreach (var val in rsNote.BendValues)
            //    {
            //        Console.WriteLine("  Bend value at {0}: {1}  (unk5: {2})  [{3}%]", val.Time, val.Step, val.Unk5, (val.Time-rsNote.Time)/rsNote.Sustain*100);
            //    }
            //}
            // adjust for capo
            if (note.Fret > 0)
                note.Fret -= capo;

            return note;
        }


        static void TransferTechniques(List<Bar> bars)
        {
            // Rocksmith places techniques like hammer-on / pull-off on the second note.
            // However, for our exporter it is much easier to handle the flag being set
            // on the first note, so we'll go through all notes and move the flag one note
            // to the left.
            bool[] hopo = new bool[] { false, false, false, false, false, false };
            for (int b = bars.Count - 1; b >= 0; --b)
            {
                var bar = bars[b];
                for (int c = bar.Chords.Count - 1; c >= 0; --c)
                {
                    var chord = bar.Chords[c];
                    foreach (var kvp in chord.Notes)
                    {
                        bool temp = kvp.Value.Hopo;
                        kvp.Value.Hopo = hopo[kvp.Key];
                        hopo[kvp.Key] = temp;
                    }
                }
            }
        }


        static void CalculateNoteDurations(List<Bar> bars)
        {
            for (int b = 0; b < bars.Count; ++b)
            {
                var bar = bars[b];

                for (int i = 0; i < bar.Chords.Count; ++i)
                {
                    var chord = bar.Chords[i];
                    Single end = (i == bar.Chords.Count - 1) ? bar.End : bar.Chords[i + 1].Start;
                    chord.Duration = bar.GetDuration(chord.Start, end - chord.Start);
                    if (chord.Duration < 2)
                    {
                        // a duration of 2 is a 64th triplet - that's the lowest we will go.
                        // If the duration is smaller than that, we are going to merge them 
                        // with the next chord.
                        chord.Duration = 0; // to be deleted on this condition after the loop
                        if (i < bar.Chords.Count - 1)
                        {
                            //Console.WriteLine("Note value too short, merging with next note in bar {0}", b);
                            var next = bar.Chords[i + 1];
                            next.Start = chord.Start;
                            foreach (var kvp in chord.Notes)
                            {
                                if (!next.Notes.ContainsKey(kvp.Key))
                                    next.Notes.Add(kvp.Key, kvp.Value);
                            }

                        }
                        else
                        {
                            // very unlikely (?) should merge with next bar
                            if (b != bars.Count-1)
                            {
                                //Console.WriteLine("Note value too short, merging with first note of next bar in bar {0}", b);
                                var next = bars[b + 1].Chords.First();
                                foreach (var kvp in chord.Notes)
                                {
                                    if (!next.Notes.ContainsKey(kvp.Key))
                                        next.Notes.Add(kvp.Key, kvp.Value);
                                }
                            }
                        }
                    }
                }
                bar.Chords.RemoveAll(x => x.Duration == 0);

                CleanRhythm(bar);
            }
        }

        static void CleanRhythm(Bar bar)
        {
            // it often happens that the Durations calculated from Rocksmith's absolute times
            // are 1 or 2 short of a "sane" note value. Try to shift such values to the previous
            // or following note to get the rhythm right.
            int[] saneDurations = new int[] { 2, 3, 4, 6, 8, 9, 12, 16, 18, 24, 32, 36, 48, 72, 96, 144, 192 };
            int[] shifts = new int[] { 1, -1, 2, -2, 3, -3 };

            int barDuration = 0;
            int expectedDuration = 48 * 4 * bar.TimeNominator / bar.TimeDenominator;

            for (int i = 0; i < bar.Chords.Count; ++i)
            {
                var chord = bar.Chords[i];
                barDuration += chord.Duration;

                if (i == bar.Chords.Count - 1)
                {
                    // see if the whole bar's duration is ok, otherwise correct by throwing away
                    // the surplus.
                    if (barDuration != expectedDuration)
                    {
                        Console.WriteLine("  {0} at end of bar does not match expected duration {1}, fixing... {2}", barDuration, expectedDuration, chord.Duration);
                        chord.Duration -= (barDuration - expectedDuration);
                        Console.WriteLine("  Now: {0}", chord.Duration);
                    }
                }

                if (saneDurations.Contains(chord.Duration))
                    continue;

                if (i < bar.Chords.Count - 1 && !saneDurations.Contains(chord.Duration))
                {
                    // now just shift to the next note
                    var next = bar.Chords[i + 1];
                    foreach (var shift in shifts)
                    {
                        if (saneDurations.Contains(chord.Duration + shift))
                        {
                            Console.WriteLine("  Shifting sloppy rhythm to next note. ({0}, {1})", chord.Duration, next.Duration);
                            chord.Duration += shift;
                            next.Duration -= shift;
                            barDuration += shift;
                            Console.WriteLine("  Now: ({0}, {1})", chord.Duration, next.Duration);
                            break;
                        }
                    }
                }
            }
        }


        static void SplitNotes(List<Bar> bars)
        {
            int[] saneDurations = new int[] { 2, 3, 4, 6, 8, 9, 12, 16, 18, 24, 32, 36, 48, 72, 96, 144, 192 };

            foreach (var bar in bars)
            {
                int beatDuration = 48 * 4 / bar.TimeDenominator;
                List<int> beatSplits = new List<int>();
                beatSplits.Add(beatDuration);
                for (int j = 2; j <= 4; ++j)
                {
                    if (beatDuration % j == 0)
                        beatSplits.Add(beatDuration / j);
                }

                int curProgress = 0;
                for (int i = 0; i < bar.Chords.Count; ++i)
                {
                    var chord = bar.Chords[i];
                    if (!saneDurations.Contains(chord.Duration))
                    {
                        // see if we can split to the next full beat / half beat, etc.
                        foreach (var split in beatSplits)
                        {
                            int toNextBeat = split - (curProgress % split);
                            if (toNextBeat <= chord.Duration && saneDurations.Contains(toNextBeat))
                            {
                                var newChord = SplitChord(chord, toNextBeat);
                                bar.Chords.Insert(i + 1, newChord);
                                break;
                            }
                        }
                    }

                    curProgress += chord.Duration;
                }
            }
        }

        static Chord SplitChord(Chord chord, int splitPoint)
        {
            var newChord = new Chord()
            {
                ChordId = chord.ChordId,
                BrushDirection = chord.BrushDirection,
                Duration = chord.Duration - splitPoint
            };
            chord.Duration = splitPoint;

            // copy over notes, but be careful to set techniques at the right points
            foreach (var kvp in chord.Notes)
            {
                var note = kvp.Value;
                var newNote = new Note()
                {
                    Fret = note.Fret,
                    String = note.String,
                    Hopo = note.Hopo,
                    LinkNext = note.LinkNext,
                    Slide = note.Slide,
                    Muted = note.Muted,
                    PalmMuted = note.PalmMuted,
                    Vibrato = note.Vibrato,
                    Harmonic = note.Harmonic,
                    Tremolo = note.Tremolo,
                    LeftFingering = -1,
                    Tapped = false
                };
                note.Hopo = false;
                note.LinkNext = true;
                note.Slide = Note.SlideType.None;
                newChord.Notes.Add(kvp.Key, newNote);
            }

            return newChord;
        }
    }
}
