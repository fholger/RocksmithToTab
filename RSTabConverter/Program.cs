using System;
using CommandLine;
using CommandLine.Text;
using RSTabConverterLib;


namespace RSTabConverter
{
    class Program
    {
        static void testXml()
        {
            var gpif = new Gpif.GPIF();
            gpif.Score.Title = "Test Title";
            gpif.Score.Artist = "Iron Maiden";
            gpif.Score.Album = "Whatever";
            gpif.MasterTrack.Tracks.Add(0);
            gpif.MasterTrack.Tracks.Add(1);

            gpif.Save("test.xml");
        }

        static void Main(string[] args)
        {
            testXml();

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
                        foreach (var arr in options.Arrangements)
                        {
                            var arrangement = browser.GetArrangement(song, arr);
                            var track = Converter.ConvertArrangement(arrangement);
                        }
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
