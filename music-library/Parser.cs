using MusicLibrary.EasterEggs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MusicLibrary.Commands;

namespace MusicLibrary
{
    public static class CommandLineParser
    {
        public static RootCommand RootCommand { get; } = new();

        public static readonly Command[] Commands = new Command[] 
        {
            ResetCommandParser.Command 
        };

        public static readonly Option<bool> VersionOption = new("--version", "Show the version information.");

        public static readonly List<Option> EasterEggOptions = EasterEggOption.ConfigureEasterEggOptions();

        public static Parser Parser { get; } = new(new CommandLineConfiguration(ConfigureRootCommand(RootCommand),
            true,
            true,
            false,
            false));

        private static RootCommand ConfigureRootCommand(RootCommand rootCommand)
        {
            AddSubcommands(rootCommand);
            AddOptions(rootCommand);

            return rootCommand;
        }

        private static void AddOptions(RootCommand rootCommand)
        {
            rootCommand.AddOption(VersionOption);
            if (EasterEggOptions != null)
            {
                foreach (Option option in EasterEggOptions)
                {
                    rootCommand.AddOption(option);
                }
            }
        }

        private static void AddSubcommands(RootCommand rootCommand)
        {
            foreach (Command command in Commands)
            {
                rootCommand.AddCommand(command);
            }
        }
    }

}
