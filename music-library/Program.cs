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

            var updateCommand = new Command("update", "Update database in specified directories.")
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
                    result.ErrorMessage = "Empty argument.";
                    return;
                }

                if (t.Contains("file:"))
                {
                    if (!Uri.IsWellFormedUriString(t, UriKind.RelativeOrAbsolute))
                        result.ErrorMessage = "Invalid URI.";
                    else
                    {
                        var target = new Uri(t);
                        if (!Directory.Exists(PathTools.GetUnescapedAbsolutePath(target)))
                            result.ErrorMessage = "Invalid URI.";
                    }
                    return;
                }

                t = PathTools.GetPath(t);
                if (PathTools.IsRelativePath(t))
                    t = Path.GetFullPath(t);
                if (!Directory.Exists(t))
                    result.ErrorMessage = "Invalid directory path.";
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
                    result.ErrorMessage = "Empty argument.";
                    return;
                }

                if (t.Contains("file:"))
                {
                    Uri target;

                    if (!Uri.IsWellFormedUriString(t, UriKind.RelativeOrAbsolute))
                    {
                        result.ErrorMessage = "Invalid URI.";
                        return;
                    }
                    else
                    {
                        target = new Uri(t);
                        if (!File.Exists(PathTools.GetUnescapedAbsolutePath(target)))
                        {
                            result.ErrorMessage = "Invalid URI.";
                            return;
                        }
                    }

                    // Extension check
                    string path = PathTools.GetUnescapedAbsolutePath(target);
                    if (!Array.Exists(FileTools.MusicExtensions, e => e == Path.GetExtension(path)))
                    {
                        result.ErrorMessage = $"{Path.GetExtension(path)} - Invalid file extension.";
                        return;
                    }
                }

                t = PathTools.GetPath(t);
                if (PathTools.IsRelativePath(t))
                    t = Path.GetFullPath(t);
                if (!File.Exists(t))
                {
                    result.ErrorMessage = "Invalid file path.";
                    return;
                }

                // Extension check
                if (!Array.Exists(FileTools.MusicExtensions, e => e == Path.GetExtension(t)))
                    result.ErrorMessage = $"{Path.GetExtension(t)} - Invalid file extension";
            });

            var importCommand = new Command("import", "Import a file, playlist or files in specific directory.")
            {
                directoryOption,
                fileOption
                // playlistOption
            };
            importCommand.AddValidator(result =>
            {
                if (result.Children.Count == 0)
                    result.ErrorMessage = "At least one valid option is required.";
                else if (result.Children.Count > 1)
                    result.ErrorMessage = "Only one target type option for the command is accepted.";
            });
            importCommand.SetHandler(async (string? directoryTarget, string? fileTarget) =>
            {
                if (directoryTarget != null)
                {
                    var comm = new Commands.ImportDirectory(_dbPath, _dbName);
                    await comm.Import(directoryTarget);
                }
                else if (fileTarget != null)
                {
                    var comm = new Commands.ImportFile(_dbPath, _dbName);
                    await comm.Import(fileTarget);
                }
            }, directoryOption, fileOption);

            var removeCommand = new Command("remove", "Remove an entry or specified directory from the library.")
            {      

            };

            var searchCommand = new Command("search", "Search library.")
            {

            };

            var silentOption = new Option("--silent", "Running command without questions.");

            var resetCommand = new Command("reset", "Reset library.")
            {

            };

            resetCommand.SetHandler(async () =>
            {
                await Commands.Reset.ResetDatabase(_dbPath, _dbName);
            });

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