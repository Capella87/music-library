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

        public Play(Database.Library library, string playerName = "ffplay")
        {
            if (library == null || library.DBConnection == null) throw new ArgumentNullException("No library connection.");
            _library = library;
            _playerName = playerName;

            try
            {
                _playerPath = Utilities.ExecutableTools.GetExecutablePath(_playerName);

            }
            catch (FileNotFoundException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{_playerName} was NOT found in your computer. Please check whether the player is installed.");
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

        public async Task<int> PlayMusic(DataTable target)
        {
            /*
             * Columns : id, title, albums, artist, album_artist, years, lyrics, year, file location
             */
            _playArgument = Utilities.ExecutableTools.GetArguments(
                new string[] { "-i", $"\"{target.Rows[0][9]}\"", "-volume 50", "-autoexit", "-loglevel error", "-stats" });

            try
            {
                Console.WriteLine("Playing music...");
                for (int i = 1; i < target.Columns.Count; i++)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"{target.Columns[i]}");
                    for (int j = 0; j < target.Columns[i].ColumnName.Length; j++)
                        Console.Write("-");
                    Console.WriteLine();
                    Console.ResetColor();

                    Console.WriteLine(target.Rows[0][i]);
                    Console.WriteLine();
                }
                string origTitle = Console.Title;
                Console.Title = $"{target.Rows[0][3]} - {target.Rows[0][1]}";
                var process = Process.Start(_playerPath, _playArgument);
                await process.WaitForExitAsync();
                Console.Title = origTitle;
            }
            catch (InvalidOperationException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid Path.");
                Console.ResetColor();
                Console.WriteLine(e.StackTrace);

                return 1;
            }

            return 0;
        }

        // These methods will be moved to Search class after v0.1.0 version to implement Search fully.
        private DataTable RetrieveData(DataTable t, int idx)
        {
            var rt = new DataTable();
            rt.Columns.Add(new DataColumn(t.Columns[9].ColumnName)); // track_id

            for (int i = 1; i <= 8; i++)
            {
                rt.Columns.Add(new DataColumn(t.Columns[i].ColumnName));
            }
            rt.Columns.Add(new DataColumn(t.Columns[10].ColumnName)); // absolute_path
            rt.Columns.Add(new DataColumn(t.Columns[11].ColumnName)); // lyrics

            DataRow target = rt.NewRow();
            target[rt.Columns[0]] = t.Rows[idx][9];
            for (int i = 1; i <= 8; i++)
                target[rt.Columns[i]] = t.Rows[idx][i];
            target[rt.Columns[9]] = t.Rows[idx][10];
            target[rt.Columns[10]] = t.Rows[idx][11];
            rt.Rows.Add(target);

            return rt;
        }

        public async Task<int> SearchTrack(string query)
        {
            try
            {
                using (var connection = _library.DBConnection)
                {
                    if (connection == null) throw new NullReferenceException("Connection Error.");
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText =
                        @"
                                SELECT ROW_NUMBER () OVER ( ORDER BY  t.title ASC ) as 'No', t.title as 'Title', alb.album as 'Album', a.artist as 'Artist', 
                                aa.album_artist as 'Album Artist', g.genre as 'Genre', t.year as 'Year', t.disc_no as 'Disc No.' , t.track_no as 'Track No.',
                                t.id as 'trackId', t.absolute_path as 'Path', t.lyrics as 'Lyrics' 
                                FROM tracks as 't', artists as 'a', albums as 'alb', album_artists as 'aa', genres as 'g'
                                WHERE t.artist_id = a.id AND t.album_id = alb.id AND t.genre_id = g.id AND alb.album_artist_id = aa.id AND t.title like @keyword
                                ORDER BY t.title ASC
                            ";
                    command.Parameters.Add("@keyword", SqliteType.Text).Value =  "%" + query + "%";
                    var result = command.ExecuteReader();

                    var table = new DataTable();
                    table.Load(result);
                    connection.Close();

                    // No result.
                    if (table.Rows.Count == 0)
                    {
                        Console.WriteLine("There's no result.");
                    }
                    else if (table.Rows.Count == 1)
                    {
                        await PlayMusic(RetrieveData(table, 0));
                    }
                    else
                    {
                        // Show results
                        var columns = new string[5];
                        for (int i = 0; i < 4; i++)
                            columns[i] = table.Columns[i].ToString();
                        columns[4] = table.Columns[6].ToString();
                        var selectTable = new Utilities.Table(columns);

                        var rows = table.Rows;
                        for (int i = 0; i < rows.Count; i++)
                        {
                            var rowElements = rows[i].ItemArray[0..4].Concat(rows[i].ItemArray[6..7]).ToArray();
                            selectTable.Add(rowElements);
                        }
                        selectTable.Print();

                        int selected = -1;
                        // selection
                        // selected = Utilities.AskDialog("Select Music", 123);
                        // if (selected == -1)
                        do
                        {
                            try
                            {
                                Console.Write("What track to run : ");
                                selected = int.Parse(Console.ReadLine());
                                if (selected < -1 || selected > rows.Count) throw new ArgumentOutOfRangeException();
                                else break;
                            }
                            catch (FormatException e)
                            {
                                Console.WriteLine("Wrong input.");
                            }
                            catch (ArgumentOutOfRangeException e)
                            {
                                Console.WriteLine("Your input is out of range.");
                            }
                        } while (true);

                        // It would be changed to use CancellationToken?
                        if (selected == -1)
                        {
                            Console.WriteLine("Abort.");
                            return 1;
                        }

                        await PlayMusic(RetrieveData(table, selected - 1));
                    }

                    return 0;
                }
            }
            catch (SqliteException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.ResetColor();

                Console.WriteLine(e.StackTrace);

                return 1;
            }
            catch (NullReferenceException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.ResetColor();

                Console.WriteLine(e.StackTrace);

                return 1;
            }
        }
    }
}
