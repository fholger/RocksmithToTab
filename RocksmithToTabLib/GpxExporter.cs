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
            gpif.Score.Title = "<![CDATA[" + score.Title + "]]>";
            gpif.Score.Artist = "<![CDATA[" + score.Artist + "]]>";
            gpif.Score.Album = "<![CDATA[" + score.Album + "]]>";

            foreach (var track in score.Tracks)
            {
                ExportTrack(track);
            }
        }

        void ExportTrack(Track track)
        {
            var gpTrack = new Gpif.Track();
            gpTrack.Id = gpif.Tracks.Count;
            gpTrack.Name = "<![CDATA[" + track.Name + " Level " + track.DifficultyLevel.ToString() + "]]>";
            gpTrack.ShortName = "<![CDATA[" + track.Name + "]]>";

            // export tuning
            var tuningProp = new Gpif.Track.Property();
            tuningProp.Pitches = track.Tuning.ToList();
            gpTrack.Properties.Add(tuningProp);

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

            gpif.Tracks.Add(gpTrack);
            gpif.MasterTrack.Tracks.Add(gpTrack.Id);

            ExportBars(track.Bars);
        }

        void ExportBars(List<Bar> bars)
        {
            int lastTempo = -1;
            for (int i = 0; i < bars.Count; ++i)
            {
                var bar = bars[i];
                if (gpif.MasterBars.Count <= i)
                {
                    // this only has to be done for the first track, all other tracks
                    // are assumed to have the same bar layout (which makes sense, if
                    // they are supposed to fit together :) ).
                    var masterBar = new MasterBar();
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
                    }
                }
            }
        }
    }
}
