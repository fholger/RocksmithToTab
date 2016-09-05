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
                // file matching or by scanning a given directory for eligible files
                List<string> inputFiles = new List<string>();
                foreach (var input in options.InputFiles)
                {
                    if (Directory.Exists(input))
                        inputFiles.AddRange(ScanDirectory(input, options.Recursive));
                    else
                        inputFiles.AddRange(SimpleGlob(input));
                }

                // create output dir, if necessary
                Directory.CreateDirectory(options.OutputDirectory);

                if (!options.XmlMode)
                {
                    if (options.Incremental)
                    {
                        // only process files which were modified since the last run, we do this
                        // by comparing their last modified date against a timestamp file we store
                        // in the output directory
                        inputFiles = FilterOldFiles(inputFiles, options.OutputDirectory);
                    }

                    for (int i = 0; i < inputFiles.Count; ++i)
                    {
                        Console.WriteLine("[{1}/{2}] Opening archive {0} ...", Path.GetFileName(inputFiles[i]), i+1, inputFiles.Count);
                        ExportPsarc(inputFiles[i], options);
                    }

                    if (inputFiles.Count == 0)
                    {
                        Console.WriteLine("All files up to date. Nothing to do :)");
                    }
                    else
                    {
                        // finally, create a timestamp file in the output directory for future reference
                        var stream = File.CreateText(Path.Combine(options.OutputDirectory, ".rs2tab.timestamp"));
                        stream.Write(System.DateTime.UtcNow);
                        stream.Close();
                    }
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


        static List<string> ScanDirectory(string path, bool recursive)
        {
            SearchOption so = (recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            var inputFiles = Directory.EnumerateFiles(path, "*.psarc", SearchOption.AllDirectories).ToList();

            // next, we need to filter for duplicates. Rocksmith dlcs usually feature two file versions,
            // one for Mac with _m ending and one for PC with _p ending. We only need to convert either one,
            // since the arrangements contained inside are identical.
            var baseNames = new HashSet<string>();
            var files = new List<string>();
            foreach (var file in inputFiles)
            {
                var baseName = Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file));
                if (baseName.Length > 2)
                {
                    var lastTwo = baseName.Substring(baseName.Length - 2);
                    if (lastTwo == "_m" || lastTwo == "_p")
                        baseName = baseName.Substring(0, baseName.Length - 2);
                }
                if (!baseNames.Contains(baseName))
                {
                    baseNames.Add(baseName);
                    files.Add(file);
                }
            }

            return files;
        }


        static List<string> FilterOldFiles(List<string> inputFiles, string outputPath)
        {
            // remove any file from the list whose last modification time is older than
            // the timestamp in the given output directory.

            string timestampPath = Path.Combine(outputPath, ".rs2tab.timestamp");
            var tsInfo = new FileInfo(timestampPath);
            if (!tsInfo.Exists)
                return inputFiles;
            var timestamp = tsInfo.LastWriteTimeUtc;

            var files = new List<string>();
            foreach (var file in inputFiles)
            {
                // take either creation or last write time, whichever happened more recently
                var info = new FileInfo(file);
                var modified = info.LastWriteTimeUtc;
                if (info.CreationTimeUtc.CompareTo(modified) > 0)
                    modified = info.CreationTimeUtc;

                if (modified.CompareTo(timestamp) > 0)
                    files.Add(file);
            }

            return files;
        }


        static void ExportPsarc(string psarcFile, CmdOptions options)
        {
            var archiveName = Path.GetFileNameWithoutExtension(psarcFile);
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

                for (int i = 0; i < toConvert.Count; ++i)
                {
                    var song = toConvert[i];
                    var score = new Score();
                    // figure out which arrangements to convert
                    var arrangements = song.Arrangements;
                    if (options.Arrangements != null && options.Arrangements.Count > 0)
                        arrangements = arrangements.Intersect(options.Arrangements).ToList();

                    Console.WriteLine("({1}/{2}) Converting song {0} ...", song.Identifier, i+1, toConvert.Count);
                    foreach (var arr in arrangements)
                    {
                        var arrangement = browser.GetArrangement(song.Identifier, arr);
                        if (arrangement == null)
                        {
                            Console.WriteLine(" Failed to get arrangement {0}", arr);
                            continue;
                        }
                        if (arrangement.ArrangementProperties.Metronome == 1)
                        {
                            // CDLC feature: optional arrangements can be generated which feature only
                            // metronome ticks, no music. However, the tab is identical to the normal
                            // track, so we don't need this unless it was explicitly requested.
                            if (options.Arrangements == null || options.Arrangements.Count == 0)
                            {
                                Console.WriteLine(" Arrangement {0} is a metronome track, ignore.", arr);
                                continue;
                            }
                        }
                        ExportArrangement(score, arrangement, arr, options.DifficultyLevel, psarcFile, toolkitInfo);

                        if (options.SplitArrangements)
                        {
                            string baseFileName = ConstructFileName(options.FileNameFormat, score, song.Identifier,
                                archiveName, toolkitInfo);
                            baseFileName = CleanFileName(string.Format("{0} ({1})", baseFileName, arr));
                            SaveScore(score, baseFileName, options.OutputDirectory, options.OutputFormat);
                            // remember to remove the track from the score again
                            score.Tracks.Clear();
                        }
                    }

                    if (!options.SplitArrangements)
                    {
                        score.SortTracks();
                        string baseFileName = CleanFileName(
                            ConstructFileName(options.FileNameFormat, score, song.Identifier, archiveName, toolkitInfo));
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
            string identifier = "none";
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
                    // xml files should be named "songidentifier_arrangement.xml", 
                    // extract the arrangement identifier, which we use to set
                    // the track name and track color as well as output file name
                    string baseFileName = Path.GetFileNameWithoutExtension(xmlFile);
                    var identifiers = baseFileName.Split(new char[] { '_' });
                    string arr = "";
                    if (identifiers.Length >= 2)
                        arr = identifiers.Last();
                    identifier = identifiers.First();

                    ExportArrangement(score, arrangement, arr, options.DifficultyLevel, xmlFile, null);
                    if (options.SplitArrangements)
                    {
                        baseFileName = CleanFileName(
                            ConstructFileName(options.FileNameFormat, score, identifier, identifier, null));
                        baseFileName += " (" + arr + ")";
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
                    ConstructFileName(options.FileNameFormat, score, identifier, identifier, null));
                SaveScore(score, baseFileName, options.OutputDirectory, options.OutputFormat);
            }
        }


        static void ExportArrangement(Score score, Song2014 arrangement, string identifier, int difficulty, 
            string originalFile, ToolkitInfo toolkitInfo)
        {
            var track = Converter.ConvertArrangement(arrangement, identifier, difficulty);
            score.Tracks.Add(track);
            score.Title = arrangement.Title;
            score.Artist = arrangement.ArtistName;
            score.ArtistSort = arrangement.ArtistNameSort;
            score.Album = arrangement.AlbumName;
            score.Year = arrangement.AlbumYear;
            score.Comments = new List<string>();
            score.Comments.Add("Generated by RocksmithToTab v" + VersionInfo.VERSION);
            score.Comments.Add("=> http://www.rocksmithtotab.de");
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
        static TxtExporter txtExporter = new TxtExporter();

        static void SaveScore(Score score, string baseFileName, string outputDirectory, string outputFormat)
        {
            string basePath = Path.Combine(outputDirectory, baseFileName);
            // make sure that the directory where the path points to exists
            string dir = Path.GetDirectoryName(basePath);
            Directory.CreateDirectory(dir);

            if (outputFormat == "gp5")
            {
                gp5Exporter.ExportScore(score, basePath + ".gp5");
            }
            else if (outputFormat == "gpif")
            {
                gpxExporter.ExportGpif(score, basePath + ".gpif");
            }
            else if (outputFormat == "txt")
            {
                txtExporter.ExportTxt(score, basePath + ".txt");
            }
            else
            {
                gpxExporter.ExportGPX(score, basePath + ".gpx");
            }

        }


        /// <summary>
        /// Replaces occurences of "{attributes}" within the supplied file name templates with
        /// their gathered values. Note that this is just doing simple string replacement, so there
        /// is no fancy character escaping like "{{attributes}}" or something similar.
        /// </summary>
        static string ConstructFileName(string template, Score score, string identifier, string archive, ToolkitInfo toolkitInfo)
        {
            var attributes = new Dictionary<string, string>();
            attributes.Add("title", score.Title);
            attributes.Add("artist", score.Artist);
            attributes.Add("artist_sort", score.ArtistSort);
            attributes.Add("album", score.Album);
            attributes.Add("year", score.Year);
            attributes.Add("tabber", score.Tabber);
            attributes.Add("identifier", identifier);
            attributes.Add("archive", archive);
            if (toolkitInfo != null && toolkitInfo.ToolkitVersion != string.Empty)
                attributes.Add("toolkit", toolkitInfo.ToolkitVersion);
            else
                attributes.Add("toolkit", "none");
            if (toolkitInfo != null && toolkitInfo.PackageVersion != string.Empty)
                attributes.Add("version", toolkitInfo.PackageVersion);
            else
                attributes.Add("version", "1.0");

            string output = template;

            foreach (var kvp in attributes)
                output = output.Replace("{"+kvp.Key+"}", kvp.Value);

            return output;
        }


        static string CleanFileName(string fileName)
        {
            // allow path separators, since we want to allow to construct subdirectories. they shouldn't 
            // occur in song names or similar, anyway
            var invalidChars = Path.GetInvalidFileNameChars().Where(x => x != Path.DirectorySeparatorChar);
            var cleaned = fileName.Where(x => !invalidChars.Contains(x)).ToArray();
            return new string(cleaned);
        }

    }
}
