using System;
using System.Text;
using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace RocksmithToTab
{
    class CmdOptions
    {
        [Option('p', "psarc", Required = true, HelpText = "PSARC song archive to open.")]
        public string PsarcFile { get; set; }

        [Option('l', "list", HelpText = "List songs contained in the archive. No conversions are performed.")]
        public bool ListSongs { get; set; }

        [OptionList('s', "songs", Separator = ',', HelpText = "Comma-separated list of tracks to include. (default: all)")]
        public IList<string> Tracks { get; set; }

        [OptionList('a', "arr", Separator = ',', HelpText = "Comma-separated list of arrangements to include. (default: all)")]
        public IList<string> Arrangements { get; set; }

        [Option('t', "split", HelpText = "Create a separate file for each arrangement.")]
        public bool SplitArrangements { get; set; }

        [Option('d', "diff", DefaultValue = 255, HelpText = "Difficulty level. (default: max)")]
        public int DifficultyLevel { get; set; }

        [Option('o', "outdir", HelpText = "Path to the directory where tabs should be created. (default: current work dir)")]
        public string OutputDirectory { get; set; }

        [HelpOption(HelpText = "Display this help screen.")]
        public string GetUsage()
        {
            var help = new HelpText
            {
                Heading = new HeadingInfo("Rocksmith 2014 Tab Converter"),
                Copyright = new CopyrightInfo("Holger Frydrych", 2014),
                AdditionalNewLineAfterOption = true,
                AddDashesToOption = true
            };
            help.AddPreOptionsLine("\nConvert Rocksmith tracks to GuitarPro .gpx tabs.\n");
            help.AddPreOptionsLine("Usage: RocksmithToTab -p archive.psarc [-a bass,lead] [-s song1,song2]");
            help.AddOptions(this);
            return help;
        }
    }
}
