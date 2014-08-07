using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RocksmithToolkitLib.Xml;

namespace RSTabConverterLib
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

            // gather measures
            track.Bars = GetBars(arrangement);

            // gather notes 
            GetNotesForDifficulty(arrangement, track.Bars, difficultyLevel);

            return track;
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


        static void GetNotesForDifficulty(Song2014 arrangement, List<Bar> bars, int difficultyLevel)
        {
            // Rocksmith keeps its notes separated by the difficulty levels. Higher difficulty
            // levels only contain notes for phrases where the notes differ from lower levels.
            // This makes collection a little awkward, as we have to go phrase by phrase, 
            // to extract all the right notes.
            int currentBar = 0;
            for (int pit = 0; pit < arrangement.PhraseIterations.Length; ++pit)
            {
                var phraseIteration = arrangement.PhraseIterations[pit];
                var phrase = arrangement.Phrases[phraseIteration.PhraseId];
                int difficulty = Math.Min(difficultyLevel, phrase.MaxDifficulty);
                var level = arrangement.Levels.FirstOrDefault(x => x.Difficulty == difficulty);
                
                while (currentBar < bars.Count && 
                    (pit == arrangement.PhraseIterations.Length-1 ||
                    bars[currentBar].Start < arrangement.PhraseIterations[pit+1].Time))
                {
                    var bar = bars[currentBar];
                    // gather notes and chords for the selected difficulty level that lie within this bar
                    var notes = from n in level.Notes where bar.ContainsTime(n.Time) select new Chord(n);
                    var chords = from c in level.Chords where bar.ContainsTime(c.Time) select new Chord(c);
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
        }
    }
}
