using Microsoft.Extensions.Hosting;
using MusicLibrary.Database;
using MusicLibrary.Utilities;
using Serilog;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using Microsoft.Extensions.Configuration;
using System.Numerics;
using MusicLibrary.Config.Settings;
using System.Text;
using System.Diagnostics;
using TagLib.Matroska;
using System.Reflection;
using System.Diagnostics.Eventing.Reader;

namespace MusicLibrary
{
    public static class Program
    {
        private static string _dbPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic
            , Environment.SpecialFolderOption.Create);

        private static string _dbName = @"musiclibrary.db";

        public static void Main(string[] args)
        {
            // Force output encoding to UTF-8 on Windows
            if (Console.IsOutputRedirected && OperatingSystem.IsWindows())
            {
                Console.OutputEncoding = Encoding.UTF8;
            }

            // Measure the startup time
            DateTime mainNowTimeStamp = DateTime.Now;
            var startupTimeStamp = mainNowTimeStamp - Process.GetCurrentProcess().StartTime;

            Environment.ExitCode = ProcessArguments(args);
        }

        internal static int ProcessArguments(string[] args)
        {

            ParseResult? parseResult = null;
            try
            {
                parseResult = CommandLineParser.Parser.Parse(args);

                if (parseResult.Errors.Count > 0 && parseResult.Tokens.Count > 0)
                {
                    Environment.ExitCode = 1;
                    Environment.Exit(Environment.ExitCode);
                }
                else
                {
                    Environment.ExitCode = parseResult.Invoke();
                }
            }
            catch (Exception e)
            {
                Environment.ExitCode = ExceptionHandler(e, parseResult);
            }

            return Environment.ExitCode;
        }

        internal static int ExceptionHandler(Exception exception, ParseResult? parseResult)
        {
            if (exception is TargetInvocationException)
            {
                exception = exception.InnerException;
            }
            else
            {
                // Should be changed to
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("Unhandled Exception: ");
                Console.WriteLine(exception.Message);
                Console.WriteLine(exception.StackTrace);
                Console.ResetColor();
            }

            return 1;
        }

        /*
        public static async Task Main(string[] args)
        {
            // Get appsettings.json to check
            var appconfig = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            try
            {
                var hasPath = Settings.HasConfigFiles(appconfig);
                if (hasPath != (true, true))
                {
                    Settings.InitializeSettings(appconfig, hasPath);
                    appconfig.Reload();
                    Log.Information("Reloaded {appconfig}", "appsettings.json");
                }
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                Log.Fatal(e.Message);
                Log.Fatal(e.StackTrace!);
                Log.Fatal("Program is terminated by unexpected exception");
                return;
            }
            catch (UnauthorizedAccessException e)
            {
                return;
            }
            catch (ArgumentNullException e)
            {
                return;
            }

            var loggingConf = new ConfigurationBuilder()
                .AddJsonFile(appconfig["MulibLoggingSettingsPath"], optional: false, reloadOnChange: true)
                .Build();

            var userConf = new ConfigurationBuilder()
                .AddJsonFile(appconfig["MulibUserSettingsPath"], optional: false, reloadOnChange: true)
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(loggingConf)
                .CreateLogger();

            // Create database file. This should be removed after EF Core migration.
            if (!File.Exists(userConf["Library:DatabasePath"]))
            {
                Console.WriteLine("Welcome to mulib!");
                Log.Information("Welcome to Mulib!");
                var myLibrary = new Library(userConf["Library:DatabasePath"], _dbName, false);
                Log.Information("Create a new database file in {path}", userConf["Library:DatabasePath"]);
                Console.WriteLine("New database file is generated.\n");
                myLibrary.Disconnect();
            }

           

            await ParseCommand()
            .UseHost(_ => Host.CreateDefaultBuilder(),
            host =>
            {
                host.ConfigureAppConfiguration((hostingContext, conf) =>
                {
                    conf.AddConfiguration(appconfig);
                    conf.AddConfiguration(loggingConf);
                    conf.AddConfiguration(userConf);
                    conf.Build();
                });
                host.ConfigureServices((hostContext, settingsService) =>
                {
                });
                host.UseSerilog();
            })
            .UseDefaults()
            .Build()
            .InvokeAsync(args);

            Log.CloseAndFlush();
        }

        private static CommandLineBuilder ParseCommand()
        {
            var rootCommand = new RootCommand("A music library implementation in C#");

            var listCommand = new Command("list", "Show entries in the library.")
            {
            };

            listCommand.SetHandler(async () =>
            {
                await Commands.List.PrintEntries(_dbPath, _dbName);
            });

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

            var trackOption = new Option<string?>(
                name: "--track",
                description: "Search a keyword by track."
                )
            {
                Arity = ArgumentArity.ExactlyOne,
                AllowMultipleArgumentsPerToken = false
            };

            var playCommand = new Command("play", "Play music using FFmpeg. Requires FFmpeg.")
            {
                trackOption
            };
            playCommand.SetHandler(async (string query) =>
            {
                var library = new Library(_dbPath, _dbName, true);
                var play = new MusicLibrary.Commands.Play(library);

                if (play.PlayerPath == null) return;

                await play.SearchTrack(query);
            }, trackOption);

            rootCommand.AddCommand(configCommand);
            rootCommand.AddCommand(exportCommand);
            rootCommand.AddCommand(importCommand);
            rootCommand.AddCommand(listCommand);
            rootCommand.AddCommand(moveCommand);
            rootCommand.AddCommand(playCommand);
            rootCommand.AddCommand(playlistCommand);
            rootCommand.AddCommand(removeCommand);
            rootCommand.AddCommand(resetCommand);
            rootCommand.AddCommand(searchCommand);
            rootCommand.AddCommand(statsCommand);
            rootCommand.AddCommand(updateCommand);

            return new CommandLineBuilder(rootCommand);
        }
        */
    }
}
