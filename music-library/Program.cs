using System;
using System.Linq;
using System.IO;
using System.Threading;
using System.Collections.Generic;

using System.CommandLine;
using System.CommandLine.Help;

using MusicLibrary.Utilities;
using MusicLibrary.Database;
using System.CommandLine.Invocation;

namespace MusicLibrary
{
    public static class Program
    {
        private static string _dbPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic
            , Environment.SpecialFolderOption.Create);
        private static string _dbName = @"musiclibrary.db";

        public static async Task<int> Main(string[] args)
        {
            return await Run(args);
        }

        public static async Task<int> Run(string[] args)
        {
            if (!File.Exists(PathTools.GetPath(_dbPath, _dbName)))
            {
                Console.WriteLine("Welcome to mulib!");
                var myLibrary = new Library(_dbPath, _dbName, false);
                Console.WriteLine("New database file is generated.\n");
                myLibrary.Disconnect();
            }

            return await ParseCommand(args);
        }

        private static async Task<int> ParseCommand(string[] args)
        {
            var rootCommand = new RootCommand("A music library implementation in C#");

            var listCommand = new Command("list", "Show entries in the library.")
            {
            };

            var updateCommand = new Command("update", "Update database in specified directries.")
            {

            };

            var directoryOption = new Option<string?>(
                name: "--dir",
                // getDefaultValue: () => null,
                description: "Import a directory."
                )
            {
                Arity = ArgumentArity.ExactlyOne,
                AllowMultipleArgumentsPerToken = false
            };
            directoryOption.AddValidator(result =>
            {
                string? t = result.GetValueForOption(directoryOption);
                if (t == null)
                {
                    result.ErrorMessage = "Empty argument";
                    return;
                }

                if (t.Contains("file:"))
                {
                    if (!Uri.IsWellFormedUriString(t, UriKind.RelativeOrAbsolute))
                        result.ErrorMessage = "Invalid URI";
                    return;
                }

                if (!Directory.Exists(t))
                    result.ErrorMessage = "Invalid directory path";
            });

            var fileOption = new Option<string?>(
                name: "--file",
                // getDefaultValue: () => null,
                description: "Import a file."
                )
            {
                Arity = ArgumentArity.ExactlyOne,
                AllowMultipleArgumentsPerToken = false
            };
            fileOption.AddValidator(result =>
            {
                string? t = result.GetValueForOption(fileOption);
                if (t == null)
                {
                    result.ErrorMessage = "Empty argument";
                    return;
                }

                if (t.Contains("file:"))
                {
                    if (!Uri.IsWellFormedUriString(t, UriKind.RelativeOrAbsolute))
                        result.ErrorMessage = "Invalid URI";
                    return;
                }

                if (!File.Exists(t))
                    result.ErrorMessage = "Invalid file path";
            });

            var importCommand = new Command("import", "Import a file, playlist or files in specific directory.")
            {
                directoryOption,
                fileOption
                // playlistOption
            };
            importCommand.SetHandler((string? directoryTarget, string? fileTarget) =>
            {
                try
                {
                    if (directoryTarget != null)
                    {
                        // Scan Directory
                    }
                    else if (fileTarget != null)
                    {
                        // Scan Directory
                    }
                    else
                    {
                        // Need to be reviewed.
                        Console.WriteLine("No valid input.");
                        return;
                    }
                }
                catch (DirectoryNotFoundException e)
                {

                }
                catch (FileNotFoundException e)
                {

                }
            }, directoryOption, fileOption);

            var removeCommand = new Command("remove", "Remove an entry or specified directory from the library.")
            {      

            };

            var searchCommand = new Command("search", "Search library.")
            {

            };

            var resetCommand = new Command("reset", "Reset library.")
            {

            };

            var exportCommand = new Command("export", "Export library as a database file or playlist.")
            {

            };

            var playlistCommand = new Command("playlist", "Run playlist tools.")
            {

            };

            var moveCommand = new Command("move", "Move or copy a item.")
            {

            };

            var configCommand = new Command("config", "Show or edit user settings.")
            {

            };

            var statsCommand = new Command("stats", "Show statistics.");

            rootCommand.AddCommand(configCommand);
            rootCommand.AddCommand(exportCommand);
            rootCommand.AddCommand(importCommand);
            rootCommand.AddCommand(listCommand);
            rootCommand.AddCommand(moveCommand);
            rootCommand.AddCommand(playlistCommand);
            rootCommand.AddCommand(removeCommand);
            rootCommand.AddCommand(resetCommand);
            rootCommand.AddCommand(searchCommand);
            rootCommand.AddCommand(statsCommand);
            rootCommand.AddCommand(updateCommand);

            return await rootCommand.InvokeAsync(args);
        }
    }
}