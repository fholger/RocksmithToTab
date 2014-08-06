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
            //var allNotes = GetNotesForDifficulty(arrangement, difficultyLevel);

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


        //static List<Chord> GetNotesForDifficulty(Song2014 arrangement, int difficultyLevel)
        //{
            // Rocksmith keeps its notes separated by the difficulty levels. Higher difficulty
            // levels only contain notes for phrases where the notes differ from lower levels.
            // This makes collection a little awkward, as we have to go phrase by phrase, then
            // through each phrase iteration, to extract all the right notes.
            //foreach (int phraseId = 0; phraseId < arrangement.Phrases.Length; ++phraseId)
            //{
            //    phrase.
            //}
        //}
    }
}
