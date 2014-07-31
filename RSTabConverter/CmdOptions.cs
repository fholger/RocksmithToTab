using System;
using System.Text;
using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace RSTabConverter
{
    class CmdOptions
    {
        [Option('p', "psarc", Required = true, HelpText = "PSARC song archive to open.")]
        public string PsarcFile { get; set; }

        [Option('l', null, HelpText = "List songs contained in the archive.")]
        public bool ListSongs { get; set; }

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
            help.AddPreOptionsLine("Usage: RSTabConverter -p archive.psarc");
            help.AddOptions(this);
            return help;
        }
    }
}
