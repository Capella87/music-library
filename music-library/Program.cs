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

        public static void Main(string[] args)
        {
            // Check whether database file is exist.
            var myLibrary = new MusicLibrary.Library(Directory.GetCurrentDirectory(), _dbName);
        }
    }
}