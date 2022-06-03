using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

using Microsoft.Data.Sqlite;

using MusicLibrary;

namespace MusicLibrary.Commands
{
    public static class List
    {
        public static async Task<int> PrintEntries(string dbPath, string dbName)
        {
            var library = new Database.Library(dbPath, dbName, true);

            try
            {
                using (var connection = library.DBConnection)
                {
                    if (connection == null) throw new NullReferenceException("Connection Error.");
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText =
                        @"
                            SELECT ROW_NUMBER () OVER ( ORDER BY  t.title ASC ) as 'No', t.title as 'Title', alb.album as 'Album', a.artist as 'Artist', 
                            aa.album_artist as 'Album Artist', g.genre as 'Genre', t.year as 'Year', t.disc_no as 'Disc No.' , t.track_no as 'Track No.'
                            FROM tracks as 't', artists as 'a', albums as 'alb', album_artists as 'aa', genres as 'g'
                            WHERE t.artist_id = a.id AND t.album_id = alb.id AND t.genre_id = g.id AND alb.album_artist_id = aa.id
                            ORDER BY t.title ASC
                        ";
                    var result = command.ExecuteReader();

                    var table = new DataTable();
                    table.Load(result);
                    connection.Close();

                    var columns = table.Columns;
                    var columnStrings = new string[columns.Count];
                    for (int i = 0; i < columns.Count; i++)
                        columnStrings[i] = columns[i].ToString();

                    var resultTable = new MusicLibrary.Utilities.Table(columnStrings);
                    var rows = table.Rows;
                    for (int i = 0; i < rows.Count; i++)
                    {
                        var rowValues = rows[i].ItemArray;
                        resultTable.Add(rowValues);
                    }

                    resultTable.Print();
                    var report = new Report.SearchReport<int>(rows.Count);
                    report.PrintReport();
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

            return 0;
        }
    }
}

