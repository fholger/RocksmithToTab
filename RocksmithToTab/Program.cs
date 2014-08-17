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
        static void TestRhythmDetector()
        {
            var testValues = new List<float>() { 168.05f, 11.92f, 12.03f };
            //testValues = new List<float>() { 24.0f, 36.03f, 12.03f, 23.94f, 24.0f, 11.94f, 23.87f, 11.68f, 24.51f };
            //testValues = new List<float>() { 23.57f, 36.59f, 12.16f, 23.69f, 24.0f, 11.94f, 24.06f, 12.0f, 24.0f };
            //testValues = new List<float>() { 24.67f, 11.73f, 23.63f, 11.91f, 24.06f, 24.13f, 12.0f, 23.57f, 11.69f, 24.62f };
            testValues = new List<float>() { 32.05f, 21.95f, 12.0f, 12.0f, 18.0f, 5.98f, 18.1f, 11.96f, 23.96f, 12.0f, 12.0f, 6.0f, 6.0f };
            var results = RhythmDetector.GetRhythm(testValues, 192, 48);
            for (int i = 0; i < results.Count; ++i)
            {
                Console.WriteLine("Note {0} for duration {1}   (was {2})", results[i].NoteIndex, results[i].Duration, testValues[results[i].NoteIndex]);
            }
        }


        static void Main(string[] args)
        {
            TestRhythmDetector();
            return;

            // parse command line arguments
            var options = new CmdOptions();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                // first, create output dir, if necessary
                if (options.OutputDirectory != string.Empty)
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
                                // create a separate file for each arrangement
                                if (options.OutputFormat == "gpif")
                                {
                                    exporter.ExportGpif(score, Path.Combine(options.OutputDirectory,
                                        song.Identifier + "_" + arr + ".gpif"));
                                }
                                else
                                {
                                    exporter.ExportGPX(score, Path.Combine(options.OutputDirectory,
                                        song.Identifier + "_" + arr + ".gpx"));
                                }
                                // remember to remove the track from the score again
                                score.Tracks.Clear();
                            }
                        }

                        if (!options.SplitArrangements)
                        {
                            if (options.OutputFormat == "gpif")
                            {
                                exporter.ExportGpif(score, Path.Combine(options.OutputDirectory, 
                                    song.Identifier + ".gpif"));
                            }
                            else
                            {
                                exporter.ExportGPX(score, Path.Combine(options.OutputDirectory,
                                    song.Identifier + ".gpx"));
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
