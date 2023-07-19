using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicLibrary;

using System.CommandLine;
using System.CommandLine.Parsing;
using System.CommandLine.Builder;


namespace MusicLibrary.Commands
{
    internal static class ResetCommandParser
    {

        public static Command Command { get; } = BuildCommand();

        private static Command BuildCommand()
        {
            var command = new Command("reset", "Obliterate all data in the music database.");

            return command;
        }
    }
}
