using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using CommandLine;
using CommandLine.Text;
using RocksmithToTabLib;


namespace RocksmithToTab
{
    class Program
    {
        static void TestGP5Output()
        {
            var score = new Score();
            var track = new Track()
            {
                AverageBeatsPerMinute = 120,
                Capo = 0,
                Name = "Track 1",
                Instrument = Track.InstrumentType.Guitar,
                Tuning = new int[] { 40, 45, 50, 55, 59, 64 }
            };
            score.Tracks.Add(track);
            var bar = new Bar()
            {
                BeatsPerMinute = 120,
                TimeNominator = 4,
                TimeDenominator = 4
            };
            track.Bars.Add(bar);
            track.Bars.Add(bar);

            for (int i = 0; i < 4; ++i)
            {
                var chord = new Chord()
                {
                    Duration = 48
                };
                chord.Notes.Add(i, new Note()
                {
                    String = i,
                    Fret = 0,
                });
                bar.Chords.Add(chord);
            }

            GP5File.ExportScore(score, "gp5test.gp5");
        }


        static void Main(string[] args)
        {
            TestGP5Output();

            // parse command line arguments
            var options = new CmdOptions();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                if (options.PsarcFile == null)
                {
                    Console.Write(options.GetUsage());
                    return;
                }

                if (options.OutputDirectory == null)
                {
                    // default output directory is derived from the given archive filename
                    options.OutputDirectory = Path.GetFileNameWithoutExtension(options.PsarcFile);
                }

                // create output dir, if necessary
                Directory.CreateDirectory(options.OutputDirectory);
                    
                Console.WriteLine("Opening archive {0} ...", options.PsarcFile);
                try
                {
                    var browser = new PsarcBrowser(options.PsarcFile);

                    var songList = browser.GetSongList();
            
                    if (options.ListSongs)
                    {
                        foreach (var song in songList)
                        {
                            Console.WriteLine("[{0}] {1} - {2}  ({3}, {4})   {{{5}}}", song.Identifier,
                                song.Artist, song.Title, song.Album, song.Year,
                                string.Join(", ", song.Arrangements));
                        }
                        return;
                    }

                    // collect all songs to convert
                    var toConvert = new List<SongInfo>();
                    if (options.Tracks == null || options.Tracks.Count == 0)
                    {
                        // if nothing was specified, convert all songs
                        toConvert = toConvert.Concat(songList).ToList();
                    }
                    else
                    {
                        foreach (var songId in options.Tracks)
                        {
                            var songInfo = songList.FirstOrDefault(x => x.Identifier == songId);
                            if (songInfo != null)
                                toConvert.Add(songInfo);
                        }
                    }

                    foreach(var song in toConvert)
                    {
                        var score = new Score();
                        var exporter = new GpxExporter();
                        // figure out which arrangements to convert
                        var arrangements = song.Arrangements;
                        if (options.Arrangements != null && options.Arrangements.Count > 0)
                            arrangements = arrangements.Intersect(options.Arrangements).ToList();

                        Console.WriteLine("Converting song " + song.Identifier + "...");
                        foreach (var arr in arrangements)
                        {
                            var arrangement = browser.GetArrangement(song.Identifier, arr);
                            if (arrangement == null)
                            {
                                Console.WriteLine("  Failed to get arrangement {0}", arr);
                                continue;
                            }
                            var track = Converter.ConvertArrangement(arrangement, options.DifficultyLevel);
                            score.Tracks.Add(track);
                            score.Title = arrangement.Title;
                            score.Artist = arrangement.ArtistName;
                            score.Album = arrangement.AlbumName;
                            score.Year = arrangement.AlbumYear;
                            if (options.SplitArrangements)
                            {
                                string baseFileName = Path.Combine(options.OutputDirectory, song.Identifier + "_" + arr);
                                // create a separate file for each arrangement
                                if (options.OutputFormat == "gp5")
                                {
                                    GP5File.ExportScore(score, baseFileName + ".gp5");
                                }
                                else if (options.OutputFormat == "gpif")
                                {
                                    exporter.ExportGpif(score, baseFileName + ".gpif");
                                }
                                else
                                {
                                    exporter.ExportGPX(score, baseFileName + ".gpx");
                                }
                                // remember to remove the track from the score again
                                score.Tracks.Clear();
                            }
                        }

                        if (!options.SplitArrangements)
                        {
                            string baseFileName = Path.Combine(options.OutputDirectory, song.Identifier);
                            if (options.OutputFormat == "gp5")
                            {
                                GP5File.ExportScore(score, baseFileName + ".gp5");
                            }
                            else if (options.OutputFormat == "gpif")
                            {
                                exporter.ExportGpif(score, baseFileName + ".gpif");
                            }
                            else
                            {
                                exporter.ExportGPX(score, baseFileName + ".gpx");
                            }
                        }
                    }
                }
                catch (IOException e)
                {
                    Console.WriteLine("Error encountered:");
                    Console.WriteLine(e.Message);
                }
            }
        }


        static void ListSongs(PsarcBrowser browser)
        {
            var songList = browser.GetSongList();
            foreach (var song in songList)
            {
                Console.WriteLine("[{0}] {1} - {2}  ({3}, {4})   {{{5}}}", song.Identifier,
                    song.Artist, song.Title, song.Album, song.Year,
                    string.Join(", ", song.Arrangements));
            }
        }

    }
}
