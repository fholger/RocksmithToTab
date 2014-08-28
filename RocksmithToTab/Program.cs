using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using CommandLine;
using CommandLine.Text;
using RocksmithToTabLib;
using RocksmithToolkitLib.Xml;


namespace RocksmithToTab
{
    class Program
    {
        static void Main(string[] args)
        {
            // parse command line arguments
            var options = new CmdOptions();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                if (options.InputFiles == null || options.InputFiles.Count == 0)
                {
                    Console.Write(options.GetUsage());
                    return;
                }

                // collect a full file list from the inputs by doing a simple glob style
                // file search.
                List<string> inputFiles = new List<string>();
                foreach (var input in options.InputFiles)
                {
                    inputFiles.AddRange(SimpleGlob(input));
                }

                // create output dir, if necessary
                Directory.CreateDirectory(options.OutputDirectory);

                if (!options.XmlMode)
                {
                    foreach (var inputFile in inputFiles)
                        ExportPsarc(inputFile, options);
                }
                else
                {
                    ExportXml(inputFiles, options);
                }
            }
        }



        /// <summary>
        /// Performs a simple glob search for files matching the pattern in path.
        /// Note this only works at the file level, not at the directory level.
        /// </summary>
        static string[] SimpleGlob(string path)
        {
            string directory = Path.GetDirectoryName(path); 
            string filePattern = Path.GetFileName(path); 

            // if path only contains pattern then use current directory
            if (String.IsNullOrEmpty(directory))
                directory = Directory.GetCurrentDirectory();

            if (!Directory.Exists(directory))
                return new string[0];

            var files = Directory.GetFiles(directory, filePattern);
            return files;
        }




        static void ExportPsarc(string psarcFile, CmdOptions options)
        {
            Console.WriteLine("Opening archive {0} ...", psarcFile);
            try
            {
                var browser = new PsarcBrowser(psarcFile);

                var songList = browser.GetSongList();
                var toolkitInfo = browser.GetToolkitInfo();

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

                foreach (var song in toConvert)
                {
                    var score = new Score();
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
                        ExportArrangement(score, arrangement, options.DifficultyLevel, psarcFile, toolkitInfo);

                        if (options.SplitArrangements)
                        {
                            string baseFileName = CleanFileName(
                                string.Format("{0} - {1} ({2})", score.Artist, score.Title, arr));
                            SaveScore(score, baseFileName, options.OutputDirectory, options.OutputFormat);
                            // remember to remove the track from the score again
                            score.Tracks.Clear();
                        }
                    }

                    if (!options.SplitArrangements)
                    {
                        score.SortTracks();
                        string baseFileName = CleanFileName(
                            string.Format("{0} - {1}", score.Artist, score.Title));
                        SaveScore(score, baseFileName, options.OutputDirectory, options.OutputFormat);
                    }
                }
                Console.WriteLine();
            }
            catch (IOException e)
            {
                Console.WriteLine("Error encountered:");
                Console.WriteLine(e.Message);
            }

        }


        static void ExportXml(List<string> inputFiles, CmdOptions options)
        {
            Score score = new Score();
            foreach (var xmlFile in inputFiles)
            {
                Console.WriteLine("Processing {0} ...", xmlFile);
                Song2014 arrangement = null;
                try
                {
                    arrangement = Song2014.LoadFromFile(xmlFile);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to parse XML input file: " + e.Message);
                }

                if (arrangement != null)
                {
                    ExportArrangement(score, arrangement, options.DifficultyLevel, xmlFile, null);
                    if (options.SplitArrangements)
                    {
                        string baseFileName = Path.GetFileNameWithoutExtension(xmlFile);
                        SaveScore(score, baseFileName, options.OutputDirectory, options.OutputFormat);
                        // remember to remove the track from the score again
                        score.Tracks.Clear();
                    }
                }
            }

            if (!options.SplitArrangements)
            {
                score.SortTracks();
                string baseFileName = CleanFileName(
                    string.Format("{0} - {1}", score.Artist, score.Title));
                SaveScore(score, baseFileName, options.OutputDirectory, options.OutputFormat);
            }
        }


        static void ExportArrangement(Score score, Song2014 arrangement, int difficulty, 
            string originalFile, ToolkitInfo toolkitInfo)
        {
            var track = Converter.ConvertArrangement(arrangement, difficulty);
            score.Tracks.Add(track);
            score.Title = arrangement.Title;
            score.Artist = arrangement.ArtistName;
            score.Album = arrangement.AlbumName;
            score.Year = arrangement.AlbumYear;
            score.Comments = new List<string>();
            score.Comments.Add("Generated by RocksmithToTab v" + VersionInfo.VERSION);
            score.Comments.Add("=> https://github.com/fholger/RocksmithToTab");
            score.Comments.Add("Created from archive: " + Path.GetFileName(originalFile));
            if (toolkitInfo != null && toolkitInfo.PackageAuthor != string.Empty)
            {
                score.Comments.Add("CDLC author:  " + toolkitInfo.PackageAuthor);
                score.Tabber = toolkitInfo.PackageAuthor;
            }
            if (toolkitInfo != null && toolkitInfo.PackageVersion != string.Empty)
                score.Comments.Add("CDLC version: " + toolkitInfo.PackageVersion);
        }


        static GpxExporter gpxExporter = new GpxExporter();
        static GP5File gp5Exporter = new GP5File();

        static void SaveScore(Score score, string baseFileName, string outputDirectory, string outputFormat)
        {
            string basePath = Path.Combine(outputDirectory, baseFileName);
            // create a separate file for each arrangement
            if (outputFormat == "gp5")
            {
                gp5Exporter.ExportScore(score, basePath + ".gp5");
            }
            else if (outputFormat == "gpif")
            {
                gpxExporter.ExportGpif(score, basePath + ".gpif");
            }
            else
            {
                gpxExporter.ExportGPX(score, basePath + ".gpx");
            }

        }


        static string CleanFileName(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var cleaned = fileName.Where(x => !invalidChars.Contains(x)).ToArray();
            return new string(cleaned);
        }

    }
}
