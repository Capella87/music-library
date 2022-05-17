using System;
using System.Linq;
using System.IO;
using System.Threading;
using System.Collections.Generic;

using System.CommandLine;

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
            return await ParseCommand(args);
        }

        private static async Task<int> ParseCommand(string[] args)
        {
            var rootCommand = new RootCommand("A music library implementation in C#")
            {

            };

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

            var searchCommand = new Command("search", "Search database.")
            {

            };

            rootCommand.AddCommand(listCommand);
            rootCommand.AddCommand(updateCommand);
            rootCommand.AddCommand(importCommand);
            rootCommand.AddCommand(removeCommand);
            rootCommand.AddCommand(searchCommand);

            return await rootCommand.InvokeAsync(args);
        }
    }
}