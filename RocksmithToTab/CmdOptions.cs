﻿using System;
using System.Text;
using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace RocksmithToTab
{
    class CmdOptions
    {
        [ValueList(typeof(List<string>))]
        public IList<string> InputFiles { get; set; }

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

        [Option('o', "outdir", DefaultValue = "rocksmith_tabs", HelpText = "Path to the directory where tabs should be created.")]
        public string OutputDirectory { get; set; }

        [Option('f', "format", DefaultValue = "gp5", HelpText = "File output format, currently either 'gp5', 'gpx', 'gpif', or 'txt'")]
        public string OutputFormat { get; set; }

        [Option('n', "name", DefaultValue = "{artist} - {title}", HelpText = "Format of the output file names. For a list of available field names, refer to the readme.")]
        public string FileNameFormat { get; set; }

        [Option('x', "xml", HelpText = "Instead of a psarc archive, supply a number of XML files describing the arrangements.")]
        public bool XmlMode { get; set; }

        [Option('r', "recursive", HelpText = "Also scan all subdirectories of specified directories.")]
        public bool Recursive { get; set; }

        [Option('i', "incremental", HelpText = "Only convert songs which were added since the last run.")]
        public bool Incremental { get; set; }

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
            help.AddPreOptionsLine("\nConvert Rocksmith tracks to Guitar Pro tabs.\n");
            help.AddPreOptionsLine("Usage: RocksmithToTab archive.psarc [-a bass,lead] [-s song1,song2]");
            help.AddPreOptionsLine("\nYou can also batch process a folder via");
            help.AddPreOptionsLine("  RocksmithToTab path/to/folder [-r] [-i]");
            help.AddOptions(this);
            return help;
        }
    }
}
