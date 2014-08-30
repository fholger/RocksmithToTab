/// Taken and adapted from RSTabExplorer: https://github.com/andulv/RSTabExplorer
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.IO;

namespace RocksmithToTabGUI
{
    public static class RocksmithLocator
	{
        /// <summary>
        /// Retrieves the Steam main installation folder from the registry
        /// </summary>
        public static string SteamFolder()
        {
            RegistryKey steamKey = Registry.LocalMachine.OpenSubKey("Software\\Valve\\Steam") 
                ?? Registry.LocalMachine.OpenSubKey("Software\\Wow6432Node\\Valve\\Steam");
            return steamKey.GetValue("InstallPath").ToString();
        }


        /// <summary>
        /// Retrieves a list of Steam library folders on this computer.
        /// </summary>
        public static List<string> SteamLibraryFolders()
        {
        	List<string> folders = new List<string>();

            string steamFolder = SteamFolder();
            folders.Add(steamFolder);

            // the list of additional steam libraries can be found in the config.vdf file
            string configFile = Path.Combine(steamFolder, "config", "config.vdf");
            Regex regex = new Regex("BaseInstallFolder[^\"]*\"\\s*\"([^\"]*)\"");
            using (StreamReader reader = new StreamReader(configFile))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    Match match = regex.Match(line);
                    if (match.Success)
                    {
                        folders.Add(Regex.Unescape(match.Groups[1].Value));
                    }
                }
            }

            return folders;
        }


        public static string Rocksmith2014FolderFromUbisoftKey()
        {
            RegistryKey ubiKey = Registry.LocalMachine.OpenSubKey(@"Software\Ubisoft\Rocksmith2014") ?? Registry.LocalMachine.OpenSubKey(@"Software\Wow6432Node\Ubisoft\Rocksmith2014");
            return ubiKey.GetValue("installdir").ToString();
        }


        /// <summary>
        /// Returns the location of the Rocksmith 2014 folder on Windows platforms.
        /// </summary>
        public static string Rocksmith2014Folder()
        {
            PlatformID platform = Environment.OSVersion.Platform;

            if (platform == PlatformID.Win32NT)
            {
                // on Windows, get a list of the Steam library folders and check each of them
                // for Rocksmith 2014
                var appFolders = SteamLibraryFolders().Select(x => x + "\\SteamApps\\common");
                foreach (var folder in appFolders)
                {
                    try
                    {
                        var matches = Directory.GetDirectories(folder, "Rocksmith2014");
                        if (matches.Length >= 1)
                        {
                            return matches[0];
                        }
                    }
                    catch (DirectoryNotFoundException)
                    {
                        //continue;
                    }

                }

                // Couldn't find folder, attempt another method
                return Rocksmith2014FolderFromUbisoftKey();
            }

            else if (platform == PlatformID.MacOSX)
            {
                // on Mac, Steam normally installs its games in ~/Library/Application Support/Steam
                string homeDir = Environment.GetEnvironmentVariable("HOME");
                string rocksmithPathGuess = Path.Combine(homeDir, "Library", "Application Support", "Steam", "SteamApps", "common", "Rocksmith2014");
                if (Directory.Exists(rocksmithPathGuess))
                    return rocksmithPathGuess;
                else
                    return null;  // can we do something more clever here?
            }

            else
            {
                // platform not supported
                return null;
            }
        }
    }
}
