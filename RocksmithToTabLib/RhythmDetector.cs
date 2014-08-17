using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocksmithToTabLib
{
    public class RhythmValue
    {
        public float Duration;
        public int NoteIndex;
    }


    public class RhythmDetector
    {
        public static List<RhythmValue> GetRhythm(List<float> noteDurations, int measureDuration, int beatDuration)
        {
            float scaling = measureDuration / noteDurations.Sum();
            Console.WriteLine("Scaling notes: {0}", scaling);
            var noteEnds = new List<float>();
            float total = 0;
            for (int i = 0; i < noteDurations.Count; ++i)
            {
                noteDurations[i] *= scaling;
                total += noteDurations[i];
                noteEnds.Add(total);
            }
            Console.Write("Initial note ends:  ");
            for (int i = 0; i < noteEnds.Count; ++i)
            {
                Console.Write("{0:f2}  ", noteEnds[i]);
            }
            Console.WriteLine();

            MatchRhythm(noteEnds, 0, noteEnds.Count, 0, measureDuration, beatDuration);

            Console.Write("Final endings:  ");
            for (int i = 0; i < noteEnds.Count; ++i)
            {
                Console.Write("{0:f2}  ", noteEnds[i]);
            }
            Console.WriteLine();

            // determine final note values
            var ret = new List<RhythmValue>();
            float offset = 0;
            foreach (var end in noteEnds)
            {
                var rhythm = new RhythmValue()
                {
                    Duration = (int)Math.Round(end - offset),
                    NoteIndex = ret.Count
                };
                offset = end;
                ret.Add(rhythm);
            }
            return ret;
        }


        static void MatchRhythm(List<float> noteEnds, int start, int end, float offset, float length, int beatDuration)
        {
            Console.WriteLine("MatchRhythm(start: {0}, end: {1}, offset: {2}, length: {3}, beatDuration: {4})", start, end, offset, length, beatDuration);
            // recursion condition: end if only one note is left in the current interval
            if (end - start <= 1)
                return;

            if (length <= 3)
            {
                // we can't divide this part any further, so all notes here need to be merged
                // i.e. all but the last note are set to a length of 0
                for (int i = start; i < end-1; ++i)
                {
                    noteEnds[i] = offset;
                }
                return;
            }

            int tripletBeat = beatDuration * 2 / 3;

            // we will now go through the note list and compare the end of each note with 
            // any multiple of the beat duration or the corresponding triplet. the closest
            // match will be taken, the note durations will be shifted accordingly, and then
            // the algorithm recurses left and right of the match.
            // the rationale behind the algorithm is as follows: even though every single note
            // will probably be slightly off in its length, in summary there is a good chance
            // to recognize the passing of e.g. two beats. So once we find that, we can look
            // deeper to approximately construct a fitting rhythm to the given note durations.
            const float PRECISION = 1.0f;
            int minMatchPos = 0;
            float minMatchEnd = 0;
            float minMatchDiff = length+1;

            for (int i = start; i < end-1; ++i)
            {
                var noteEnd = noteEnds[i] - offset;
                // try even rhythm
                float mult = (float)Math.Round(noteEnds[i] / beatDuration);
                float diff = Math.Abs(mult * beatDuration - noteEnds[i]);
                if (diff < minMatchDiff)
                {
                    minMatchPos = i;
                    minMatchEnd = mult * beatDuration;
                    minMatchDiff = diff;
                }

                // try the triplet variant
                mult = (float)Math.Round(noteEnds[i] / tripletBeat);
                diff = Math.Abs(mult * tripletBeat - noteEnds[i]);
                if (diff < minMatchDiff)
                {
                    minMatchPos = i;
                    minMatchEnd = mult * tripletBeat;
                    minMatchDiff = diff;
                }                
            }

            if (minMatchDiff < PRECISION || beatDuration <= 3)
            {
                // take the closest match and correct it to the determined value,
                // then rescale the other note ends accordingly and recurse
                float originalLeftLength = noteEnds[minMatchPos] - offset;
                float correctedLeftLength = minMatchEnd - offset;
                float originalRightLength = length - noteEnds[minMatchPos] + offset;
                float correctedRightLength = length - minMatchEnd + offset;
                float leftScaling = correctedLeftLength / originalLeftLength;
                float rightScaling = correctedRightLength / originalRightLength;
                noteEnds[minMatchPos] = minMatchEnd;
                Console.WriteLine("Corrected note {0} to length {1}", minMatchPos, minMatchEnd);
                for (int i = start; i < minMatchPos; ++i)
                {
                    // rescale left side
                    noteEnds[i] = offset + (noteEnds[i] - offset) * leftScaling;
                }
                for (int i = minMatchPos + 1; i < end-1; ++i)
                {
                    // rescale right side
                    noteEnds[i] = offset + (noteEnds[i] - offset) * rightScaling;
                }
                Console.Write("Current endings:  ");
                for (int i = 0; i < noteEnds.Count; ++i)
                {
                    Console.Write("{0:f2}  ", noteEnds[i]);
                }
                Console.WriteLine();
                // recurse left
                MatchRhythm(noteEnds, start, minMatchPos + 1, offset, correctedLeftLength, beatDuration);
                // recurse right
                MatchRhythm(noteEnds, minMatchPos + 1, end, minMatchEnd, correctedRightLength, beatDuration);
            }
            else
            {
                // no luck, try matching to a smaller beat value
                MatchRhythm(noteEnds, start, end, offset, length, beatDuration / 2);
            }
        }

    }
}
