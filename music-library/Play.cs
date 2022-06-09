using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Data;

using Microsoft.Data.Sqlite;

namespace MusicLibrary.Commands
{
    public class Play
    {
        Database.Library _library;
        private long _playTargetId;

        private string? _playerPath;
        private string _playerName;
        private string _playArgument;

        public Play(Database.Library library, long trackId, string playerName = "ffplay")
        {
            if (_library == null || _library.DBConnection == null) throw new ArgumentNullException("No library connection.");
            _library = library;
            _playTargetId = trackId;
            _playerName = playerName;

            try
            {
                _playerPath = Utilities.ExecutableTools.GetExecutablePath(_playerName);

            }
            catch (FileNotFoundException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{_playerName} is NOT found in your computer. Please check whether the player is installed.");
                Console.ResetColor();
                Console.WriteLine(e.StackTrace);

                return;
            }
            catch (PlatformNotSupportedException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("This feature is NOT supported Operating System yet.");
                Console.ResetColor();
                Console.WriteLine(e.StackTrace);

                return;
            }
            catch (ArgumentNullException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.ResetColor();
                Console.WriteLine(e.StackTrace);

                return;
            }
        }

        public async Task<int> PlayMusic(DataRow target, DataColumnCollection columns)
        {
            /*
             * Columns : id, title, albums, artist, album_artist, years, lyrics, year, file location
             */
            _playArgument = Utilities.ExecutableTools.GetArguments(
                new string[] { "-i", $"\"{(string)target[8]}\"", "-volume 50", "-autoexit", "-loglevel error", "-stats" });

            try
            {
                Console.WriteLine("Playing music...");
                for (int i = 0; i < columns.Count; i++)
                {
                    Console.WriteLine($"{columns[i]}\n----");
                    Console.WriteLine(target[i]);
                }
                var process = Process.Start(_playerPath, _playArgument);
                await process.WaitForExitAsync();
            }
            catch (InvalidOperationException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid Path");
                Console.ResetColor();
                Console.WriteLine(e.StackTrace);

                return 1;
            }

            return 0;
        }
    }
}
