using System;
using CommandLine;
using CommandLine.Text;

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
            }
        }
    }
}
