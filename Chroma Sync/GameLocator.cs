﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.IO;


namespace ChromaSync
{
    public static class GameLocator
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
        /// Retrieves a list of Steam library folders on this computer as there may be more than one.
        /// </summary>
        public static List<string> SteamLibraryFolders()
        {
            List<string> folders = new List<string>();

            try
            {
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
            }
            catch (Exception)
            {
                // if there's any error in getting the Steam directory, ignore it for now.
            }

            return folders;
        }


        /// <summary>
        /// Returns the location of a particular game on Windows platforms.
        /// </summary>
        public static string InstallFolder(string expectedFolder)
        {
            PlatformID platform = Environment.OSVersion.Platform;

            if (platform == PlatformID.Win32NT)
            {
                // on Windows, get a list of the Steam library folders and check each of them
                // for game
                var appFolders = SteamLibraryFolders().Select(x => x + "\\SteamApps\\common");
                foreach (var folder in appFolders)
                {
                    try
                    {
                        var matches = Directory.GetDirectories(folder, expectedFolder);
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


                return null;
            }

            else if (platform == PlatformID.MacOSX || platform == PlatformID.Unix)
            {
                // on Mac, Steam normally installs its games in ~/Library/Application Support/Steam
                // note: check for Unix platform id, too, since apparently OSX is identified as Unix in Mono, too.
                string homeDir = Environment.GetEnvironmentVariable("HOME");
                string rocksmithPathGuess = Path.Combine(homeDir, "Library", "Application Support", "Steam", "SteamApps", "common", "Rocksmith2014");
                if (Directory.Exists(rocksmithPathGuess))
                    return rocksmithPathGuess;
                else
                    return null;  // can we do something more clever here?
            }

            else
            {
                Console.WriteLine("Don't recognize this platform...");
                // platform not supported
                return null;
            }
        }
    }
}
