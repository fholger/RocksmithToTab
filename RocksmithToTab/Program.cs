using System;
using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;
using RocksmithToTabLib;


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
                Console.WriteLine("Opening archive {0} ...", options.PsarcFile);
                try
                {
                    var browser = new PsarcBrowser(options.PsarcFile);
            
                    if (options.ListSongs)
                    {
                        ListSongs(browser);
                        return;
                    }
            
                    foreach (var song in options.Tracks)
                    {
                        var score = new Score();
                        bool titleSet = false;
                        foreach (var arr in options.Arrangements)
                        {
                            var arrangement = browser.GetArrangement(song, arr);
                            var track = Converter.ConvertArrangement(arrangement);
                            score.Tracks.Add(track);
                            if (!titleSet)
                            {
                                score.Title = arrangement.Title;
                                score.Artist = arrangement.ArtistName;
                                score.Album = arrangement.AlbumName;
                                score.Year = arrangement.AlbumYear;
                            }
                        }
            
                        var exporter = new GpxExporter();
                        exporter.ExportGpif(score, song + ".gpif");
                    }
                    //exporter.SaveToFile("nonsensical.xml");
                }
                catch (System.IO.FileNotFoundException e)
                {
                    Console.WriteLine("File does not exist: {0}", e.FileName);
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
