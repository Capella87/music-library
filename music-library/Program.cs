using System;
using System.Linq;
using System.IO;
using System.Threading;
using System.Collections.Generic;

using System.CommandLine;
using System.CommandLine.Help;

using MusicLibrary.Utilities;

namespace MusicLibrary
{
    public static class Program
    {
        private static string _dbPath = @"\";
        private static string _dbName = @"musiclibrary.db";

        public static async Task<int> Main(string[] args)
        {
            return await Run(args);
        }

        public static async Task<int> Run(string[] args)
        {
            if (!File.Exists(Utilities.PathTools.GetPath(Directory.GetCurrentDirectory(), _dbName)))
            {
                Console.WriteLine("Welcome to mulib!");
                var myLibrary = new Library(Directory.GetCurrentDirectory(), _dbName, false);
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

            var importCommand = new Command("import", "Import a file, playlist or files in specific directory.")
            {

            };

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