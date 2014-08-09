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

            foreach (var rsTemplate in arrangement.ChordTemplates)
            {
                if (rsTemplate.ChordId.HasValue)
                {
                    // only store those with a ChordId, we have no use for any other
                    var template = new ChordTemplate()
                    {
                        ChordId = rsTemplate.ChordId.Value,
                        Name = rsTemplate.ChordName,
                        Frets = new int[] { rsTemplate.Fret0, rsTemplate.Fret1, rsTemplate.Fret2,
                            rsTemplate.Fret3, rsTemplate.Fret4, rsTemplate.Fret5 },
                        Fingers = new int[] { rsTemplate.Finger0, rsTemplate.Finger1, 
                            rsTemplate.Finger2, rsTemplate.Finger3, rsTemplate.Finger4,
                            rsTemplate.Finger5 }
                    };
                    templates.Add(template.ChordId, template);
                }
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
                        // figure out time and tempo
                        currentMeasure.GuessTimeAndBPM(arrangement.AverageTempo);
                    }

                    currentMeasure = new Bar() { TimeNominator = 1, Start = ebeat.Time };
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
                    }
                }
            }

            if (bars.Count > 0)
            {
                var lastBar = bars.Last();
                lastBar.End = arrangement.SongLength;
                lastBar.GuessTimeAndBPM(arrangement.AverageTempo);                
            }

            return bars;
        }


        static int CollectNotesForDifficulty(Song2014 arrangement, List<Bar> bars, Dictionary<int, ChordTemplate> chordTemplates, int difficultyLevel)
        {
            // Rocksmith keeps its notes separated by the difficulty levels. Higher difficulty
            // levels only contain notes for phrases where the notes differ from lower levels.
            // This makes collection a little awkward, as we have to go phrase by phrase, 
            // to extract all the right notes.
            int currentBar = 0;
            int maxDifficulty = 0;
            for (int pit = 0; pit < arrangement.PhraseIterations.Length; ++pit)
            {
                var phraseIteration = arrangement.PhraseIterations[pit];
                var phrase = arrangement.Phrases[phraseIteration.PhraseId];
                int difficulty = Math.Min(difficultyLevel, phrase.MaxDifficulty);
                var level = arrangement.Levels.FirstOrDefault(x => x.Difficulty == difficulty);
                maxDifficulty = Math.Max(difficulty, maxDifficulty);
                
                while (currentBar < bars.Count && 
                    (pit == arrangement.PhraseIterations.Length-1 ||
                    bars[currentBar].Start < arrangement.PhraseIterations[pit+1].Time))
                {
                    var bar = bars[currentBar];
                    // gather notes and chords for the selected difficulty level that lie within this bar
                    var notes = from n in level.Notes where bar.ContainsTime(n.Time) select CreateChord(n);
                    var chords = from c in level.Chords where bar.ContainsTime(c.Time) select CreateChord(c, chordTemplates);
                    bar.Chords = notes.Union(chords).OrderBy(x => x.Start).ToList();
                    Console.WriteLine("Bar {0}: Added {1} chords.", currentBar, bar.Chords.Count);

                    // in case that the bar is empty or the first note does not coincide with the start
                    // of the bar, we add an empty chord to the beginning indicating silence.
                    if (bar.Chords.Count == 0 || bar.Chords.First().Start > bar.Start)
                    {
                        bar.Chords.Insert(0, new Chord() { Start = bar.Start });
                        Console.WriteLine("Bar {0}: Added silence at the beginning.", currentBar);
                    }

                    ++currentBar;
                }
            }
            return maxDifficulty;
        }


        static Chord CreateChord(SongNote2014 note)
        {
            var chord = new Chord();
            chord.Start = note.Time;
            chord.Notes.Add(note.String, CreateNote(note));
            return chord;
        }


        static Chord CreateChord(SongChord2014 rsChord, Dictionary<int, ChordTemplate> chordTemplates)
        {
            var chord = new Chord();
            chord.Start = rsChord.Time;
            chord.ChordId = rsChord.ChordId;
            if (rsChord.ChordNotes != null)
            {
                foreach (var note in rsChord.ChordNotes)
                {
                    chord.Notes.Add(note.String, CreateNote(note));
                }
            }
            else if (chordTemplates.ContainsKey(chord.ChordId))
            {
                // need to determine chords from the chord template
                var template = chordTemplates[chord.ChordId];
                for (int i = 0; i < 6; ++i)
                {
                    if (template.Frets[i] >= 0)
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
            return chord;
        }


        static Note CreateNote(SongNote2014 rsNote)
        {
            var note = new Note()
            {
                String = rsNote.String,
                Fret = rsNote.Fret
            };

            return note;
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
                            Console.WriteLine("Note value too short, merging with next note in bar {0}", b);
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
                                Console.WriteLine("Note value too short, merging with first note of next bar in bar {0}", b);
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
            }
        }
    }
}
