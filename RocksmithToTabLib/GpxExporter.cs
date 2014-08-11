using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gpif;

namespace RocksmithToTabLib
{
    public class GpxExporter
    {
        private GPIF gpif;
        private bool[] hopo;
        private bool[] link;
        private int prevChordId = -1;

        public void ExportGpif(Score score, string fileName)
        {
            Export(score);
            gpif.Save(fileName);
        }

        public void ExportGPX(Score score, string fileName, bool compress = true)
        {
            Export(score);
        }


        void Export(Score score)
        {
            gpif = new GPIF();
            // set basic properties
            gpif.Score.Title = score.Title;
            gpif.Score.Artist = score.Artist;
            gpif.Score.Album = score.Album;

            foreach (var track in score.Tracks)
            {
                ExportTrack(track);
            }
        }

        void ExportTrack(Track track)
        {
            hopo = new bool[] { false, false, false, false, false, false };
            link = new bool[] { false, false, false, false, false, false };

            var gpTrack = new Gpif.Track();
            gpTrack.Id = gpif.Tracks.Count;
            gpTrack.Name = track.Name + " Level " + track.DifficultyLevel.ToString();
            gpTrack.ShortName = track.Name;

            // export tuning
            var tuningProp = new Gpif.Property();
            tuningProp.Name = "Tuning";
            tuningProp.Pitches = track.Tuning.ToList();
            if (track.Instrument == Track.InstrumentType.Bass)
            {
                // remove last two entries, as they are not used in bass and confuse Guitar Pro
                tuningProp.Pitches.RemoveRange(4, 2);
            }
            gpTrack.Properties.Add(tuningProp);
            // add capo?
            if (track.Capo > 0)
            {
                gpTrack.Properties.Add(new Gpif.Property()
                    {
                        Name = "CapoFret",
                        Fret = track.Capo
                    }
                );
            }

            if (track.Instrument == Track.InstrumentType.Guitar)
            {
                gpTrack.Instrument = new Instrument() { Ref = "e-gtr6" };
                gpTrack.GeneralMidi.Program = 29;
                gpTrack.GeneralMidi.Port = 0;
                gpTrack.GeneralMidi.PrimaryChannel = 0;
                gpTrack.GeneralMidi.SecondaryChannel = 1;
                gpTrack.GeneralMidi.ForeOneChannelPerString = false;
            }
            else if (track.Instrument == Track.InstrumentType.Bass)
            {
                gpTrack.Instrument = new Instrument() { Ref = "e-bass4" };
                gpTrack.GeneralMidi.Program = 33;
                gpTrack.GeneralMidi.Port = 0;
                gpTrack.GeneralMidi.PrimaryChannel = 2;
                gpTrack.GeneralMidi.SecondaryChannel = 3;
                gpTrack.GeneralMidi.ForeOneChannelPerString = false;
            }
            else
            {
                // TODO: Vocals
            }

            // add chord diagrams
            ExportChordDiagrams(gpTrack, track);

            gpif.Tracks.Add(gpTrack);
            gpif.MasterTrack.Tracks.Add(gpTrack.Id);

            ExportBars(track);
        }


        void ExportChordDiagrams(Gpif.Track gpTrack, Track track)
        {
            var diagrams = new Property() { Name = "DiagramCollection", Items = new List<Item>() };
            foreach (var kvp in track.ChordTemplates)
            {
                // only display those with an actual name
                if (kvp.Value.Name == string.Empty)
                    continue;

                // first, we need to determine at what base fret to start the chord diagram,
                // as all fret values are then relative to that
                int minFret = 100;
                int maxFret = 0;
                for (int i = 0; i < 6; ++i)
                {
                    if (kvp.Value.Frets[i] != -1)
                    {
                        minFret = Math.Min(kvp.Value.Frets[i], minFret);
                        maxFret = Math.Max(kvp.Value.Frets[i], maxFret);
                    }
                }
                if (maxFret > 5)
                    minFret = Math.Max(0, minFret - 1);
                else
                    minFret = 0;

                var diagram = new Item() { Id = kvp.Value.ChordId, Name = kvp.Value.Name };
                diagram.Diagram.StringCount = (track.Instrument == Track.InstrumentType.Bass ? 4 : 6);
                diagram.Diagram.FretCount = 5;
                diagram.Diagram.BaseFret = minFret;
                for (int i = 0; i < 6; ++i)
                {
                    if (kvp.Value.Frets[i] != -1)
                    {
                        diagram.Diagram.Frets.Add(new Diagram.FretType()
                        {
                            String = i,
                            Fret = kvp.Value.Frets[i] - minFret
                        });
                    }

                    var position = new Diagram.Position()
                    {
                        String = i,
                        Fret = kvp.Value.Frets[i] - minFret
                    };
                    switch (kvp.Value.Fingers[i])
                    {
                        case 1:
                            position.Finger = "Index"; break;
                        case 2:
                            position.Finger = "Middle"; break;
                        case 3:
                            position.Finger = "Ring"; break;
                        case 4:
                            position.Finger = "Pinky"; break;
                        default:
                            position.Finger = "None"; break;
                    }
                    diagram.Diagram.Fingering.Add(position);
                }

                diagrams.Items.Add(diagram);
            }
            gpTrack.Properties.Add(diagrams);
        }


        void ExportBars(Track track)
        {
            int lastTempo = -1;
            prevChordId = -1;
            for (int i = 0; i < track.Bars.Count; ++i)
            {
                var bar = track.Bars[i];
                if (gpif.MasterBars.Count <= i)
                {
                    // this only has to be done for the first track, all other tracks
                    // are assumed to have the same bar layout (which makes sense, if
                    // they are supposed to fit together :) ).
                    var masterBar = new MasterBar();
                    masterBar.Time = string.Format("{0}/{1}", bar.TimeNominator, bar.TimeDenominator);
                    gpif.MasterBars.Add(masterBar);
                    if (bar.BeatsPerMinute != lastTempo)
                    {
                        // set tempo for this bar
                        var tempo = new Automation();
                        tempo.Bar = i;
                        tempo.Position = 0;
                        tempo.Linear = false;
                        tempo.Value[0] = bar.BeatsPerMinute;
                        tempo.Value[1] = 2; // no idea what this represents
                        gpif.MasterTrack.Automations.Add(tempo);
                        lastTempo = bar.BeatsPerMinute;
                    }
                }

                // construct a voice for this bar
                var voice = new Voice();
                voice.Id = gpif.Voices.Count;
                foreach (var chord in bar.Chords)
                {
                    int id = ExportOrFindBeat(chord);
                    voice.Beats.Add(id);
                }

                // see if this voice is already available, otherwise add
                var searchVoice = gpif.Voices.Find(x => x.Equals(voice));
                if (searchVoice != null)
                    voice = searchVoice;
                else
                    gpif.Voices.Add(voice);

                // construct the bar
                var gpBar = new Gpif.Bar();
                gpBar.Id = gpif.Bars.Count;
                if (track.Instrument == Track.InstrumentType.Bass)
                    gpBar.Clef = "F4";
                else
                    gpBar.Clef = "G2";
                gpBar.Voices[0] = voice.Id;
                // see if this bar is already available, otherwise add
                var searchBar = gpif.Bars.Find(x => x.Equals(gpBar));
                if (searchBar != null)
                    gpBar = searchBar;
                else
                    gpif.Bars.Add(gpBar);

                // add to master bar
                gpif.MasterBars[i].Bars.Add(gpBar.Id);
            }
        }


        int ExportOrFindBeat(Chord chord)
        {
            var beat = new Beat();
            beat.Id = gpif.Beats.Count;
            foreach (var note in chord.Notes)
            {
                int id = ExportOrFindNote(note.Value);
                beat.Notes.Add(id);
            }

            // should we display a strum hint?
            if (chord.BrushDirection != Chord.BrushType.None)
            {
                var brushProp = new Property() { Name = "Brush" };
                if (chord.BrushDirection == Chord.BrushType.Down)
                    brushProp.Direction = "Down";
                else
                    brushProp.Direction = "Up";
                if (beat.Properties == null)
                    beat.Properties = new List<Property>();
                beat.Properties.Add(brushProp);
            }

            // tremolo picking
            if (chord.Tremolo)
            {
                // 32nd notes tremolo picking (should be appropriate)
                beat.Tremolo = "1/8"; 
            }

            // construct rhythm
            var rhythm = new Rhythm();
            rhythm.Id = gpif.Rhythms.Count;
            switch (chord.Duration)
            {
                case 192:
                    rhythm.NoteValue = "Whole";
                    break;
                case 168:  // should avoid this, split note instead (TODO)
                    rhythm.NoteValue = "Half";
                    rhythm.AugmentationDot = new Rhythm.Dot() { Count = 2 };
                    break;
                case 144:
                    rhythm.NoteValue = "Half";
                    rhythm.AugmentationDot = new Rhythm.Dot() { Count = 1 };
                    break;
                case 96:
                    rhythm.NoteValue = "Half";
                    break;
                case 84:  // should avoid this, split note instead (TODO)
                    rhythm.NoteValue = "Quarter";
                    rhythm.AugmentationDot = new Rhythm.Dot() { Count = 2 };
                    break;
                case 72:
                    rhythm.NoteValue = "Quarter";
                    rhythm.AugmentationDot = new Rhythm.Dot() { Count = 1 };
                    break;
                case 48:
                    rhythm.NoteValue = "Quarter";
                    break;
                case 36:
                    rhythm.NoteValue = "Eighth";
                    rhythm.AugmentationDot = new Rhythm.Dot() { Count = 1 };
                    break;
                case 32:
                    rhythm.NoteValue = "Quarter";
                    rhythm.PrimaryTuplet = new Rhythm.Tuplet() { Den = 2, Num = 3 };
                    break;
                case 24:
                    rhythm.NoteValue = "Eighth";
                    break;
                case 18:
                    rhythm.NoteValue = "16th";
                    rhythm.AugmentationDot = new Rhythm.Dot() { Count = 1 };
                    break;
                case 16:
                    rhythm.NoteValue = "Eighth";
                    rhythm.PrimaryTuplet = new Rhythm.Tuplet() { Den = 2, Num = 3 };
                    break;
                case 12:
                    rhythm.NoteValue = "16th";
                    break;
                case 9:
                    rhythm.NoteValue = "32nd";
                    rhythm.AugmentationDot = new Rhythm.Dot() { Count = 1 };
                    break;
                case 8:
                    rhythm.NoteValue = "16th";
                    rhythm.PrimaryTuplet = new Rhythm.Tuplet() { Den = 2, Num = 3 };
                    break;
                case 6:
                    rhythm.NoteValue = "32nd";
                    break;
                case 4:
                    rhythm.NoteValue = "32nd";
                    rhythm.PrimaryTuplet = new Rhythm.Tuplet() { Den = 2, Num = 3 };
                    break;
                case 3:
                    rhythm.NoteValue = "64th";
                    break;
                case 2:
                    rhythm.NoteValue = "64th";
                    rhythm.PrimaryTuplet = new Rhythm.Tuplet() { Den = 2, Num = 3 };
                    break;
                default:
                    Console.WriteLine("Warning: Rhythm Duration {0} not handled, defaulting to quarter note.", chord.Duration);
                    rhythm.NoteValue = "Quarter";
                    break;
            }
            // see if this rhythm already exists, otherwise add
            var searchRhythm = gpif.Rhythms.Find(x => x.Equals(rhythm));
            if (searchRhythm != null)
                rhythm = searchRhythm;
            else
                gpif.Rhythms.Add(rhythm);

            beat.Rhythm.Ref = rhythm.Id;

            // should we display a chord name?
            if (chord.ChordId != -1 && chord.ChordId != prevChordId)
            {
                beat.Chord = chord.ChordId.ToString();
            }
            prevChordId = chord.ChordId;

            // see if this beat already exists, otherwise add
            var searchBeat = gpif.Beats.Find(x => x.Equals(beat));
            if (searchBeat != null)
                beat = searchBeat;
            else
                gpif.Beats.Add(beat);

            return beat.Id;
        }

        int ExportOrFindNote(Note note)
        {
            var gpNote = new Gpif.Note();
            gpNote.Id = gpif.Notes.Count;
            // add string and fret numbers
            gpNote.Properties.Add(new Property() { Name = "String", String = note.String });
            gpNote.Properties.Add(new Property() { Name = "Fret", Fret = note.Fret });
            // should we add muting?
            if (note.PalmMuted)
                gpNote.Properties.Add(new Property() { Name = "PalmMuted", Enable = new Property.EnableType() });
            if (note.Muted)
                gpNote.Properties.Add(new Property() { Name = "Muted", Enable = new Property.EnableType() });

            // handle ties with previous/next note
            if (link[note.String] || note.LinkNext)
            {
                gpNote.Tie = new Gpif.Note.TieType() { Destination = link[note.String], Origin = note.LinkNext && note.Slide != Note.SlideType.ToNext };
                link[note.String] = note.LinkNext && note.Slide != Note.SlideType.ToNext;
            }
            // handle hammer-on / pull-off
            if (hopo[note.String])
            {
                gpNote.Properties.Add(new Property() { Name = "HopoDestination", Enable = new Property.EnableType() });
                hopo[note.String] = false;
            }
            if (note.Hopo)
            {
                gpNote.Properties.Add(new Property() { Name = "HopoOrigin", Enable = new Property.EnableType() });
                hopo[note.String] = true;
            }

            // handle vibrato
            if (note.Vibrato)
                gpNote.Vibrato = "Slight";

            // handle accent
            if (note.Accent)
                gpNote.Accent = 4;

            // handle harmonics
            if (note.Harmonic)
            {
                gpNote.Properties.Add(new Property()
                {
                    Name = "HarmonicType",
                    HType = "Natural"
                });
                gpNote.Properties.Add(new Property()
                {
                    Name = "HarmonicFret",
                    HFret = note.Fret == 3 ? "3.2" : note.Fret.ToString()
                });
            }

            // handle slights
            int slideFlag = 0;
            switch (note.Slide)
            {
                case Note.SlideType.ToNext:
                    slideFlag = 2; break;
                case Note.SlideType.UnpitchDown:
                    slideFlag = 4; break;
                case Note.SlideType.UnpitchUp:
                    slideFlag = 8; break;
            }
            if (slideFlag != 0)
                gpNote.Properties.Add(new Property() { Name = "Slide", Flags = slideFlag });

            // see if this note already exists, otherwise add
            var searchNote = gpif.Notes.Find(x => x.Equals(gpNote));
            if (searchNote != null)
                gpNote = searchNote;
            else
                gpif.Notes.Add(gpNote);

            return gpNote.Id;
        }
    }
}
