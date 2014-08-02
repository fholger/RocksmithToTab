using System;
using CommandLine;
using CommandLine.Text;
using RSTabConverterLib;

namespace RSTabConverter
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
                        ListSongs(browser);
                }
                catch (System.IO.FileNotFoundException e)
                {
                    Console.WriteLine("File does not exist: {0}", e.FileName);
                }
            }
        }


        static void ListSongs(PsarcBrowser browser)
        {
            var trackList = browser.GetTrackList();
            foreach (var track in trackList)
            {
                Console.WriteLine("[{0}] {1} - {2}  ({3}, {4})   {{{5}}}", track.Identifier,
                    track.Artist, track.Title, track.Album, track.Year,
                    string.Join(", ", track.Arrangements));
            }
        }

    }
}
