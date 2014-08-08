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

        [OptionList('a', "arr", Separator = ',', HelpText = "Comma-separated list of arrangements to include. (default: all)")]
        public IList<string> Arrangements { get; set; }

        [Option('s', "sep-arr", HelpText = "Create a separate file for each arrangement.")]
        public bool SeparateFilePerArrangement { get; set; }

        [OptionList('d', "levels", Separator = ',', HelpText = "Comma-separated list of difficulty levels to include. (default: all)")]
        public IList<string> DifficultyLevels { get; set; }

        [Option('h', "sep-levels", HelpText = "Create a separate file for each difficulty level. (Implies -s)")]
        public bool SeparateFilePerLevel { get; set; }

        [OptionList('t', "tracks", Separator = ',', HelpText = "Comma-separated list of tracks to include. (default: all)")]
        public IList<string> Tracks { get; set; }

        [OptionList('o', "outdir", HelpText = "Path to the directory where tabs should be created. (default: current work dir)")]
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
            help.AddPreOptionsLine("\nConvert Rocksmith tracks to MusicXML tabs.\n");
            help.AddPreOptionsLine("Usage: RSTabConverter -p archive.psarc [-a bass,lead] [-t rumine,savior]");
            help.AddOptions(this);
            return help;
        }
    }
}
