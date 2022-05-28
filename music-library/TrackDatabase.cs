using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Data.Sqlite;

namespace MusicLibrary.Database
{
    public class TrackDatabase
    {
        private Library _library;

        public TrackDatabase(Library library)
        {
            if (library == null || library.DBConnection == null)
                throw new ArgumentNullException("There's an error in database connection.");
            _library = library;
        }

        public List<Uri> GetMusicUris(List<Uri>? targets = null)
        {
            var rt = new List<Uri>();

            if (_library.DBConnection == null) throw new NullReferenceException("There's an error in database connection.");
            _library.DBConnection.Open();

            try
            {
                var command = _library.DBConnection.CreateCommand();

                if (targets != null)
                {
                    command.CommandText = @"SELECT uri FROM tracks WHERE uri LIKE @keyword";
                    foreach (var t in targets)
                    {
                        // Need to measure performance.
                        var keyword = new SqliteParameter("@keyword", "%" + t.AbsoluteUri + "%");
                        command.Parameters.Add(keyword);
                    }

                    // Need to be reviewed.
                        var result = command.ExecuteReader();
                        while (result.Read())
                        {
                            try
                            {
                                rt.Add(new Uri(result.GetString(0)));
                            }
                            catch (UriFormatException e)
                            {
                                continue;
                            }
                        }
                        // command.Parameters.Remove(keyword);
                }
                else
                {
                    command.CommandText = @"SELECT uri FROM tracks";
                    var result = command.ExecuteReader();
                    while (result.Read())
                    {
                        try
                        {
                            rt.Add(new Uri(result.GetString(0)));
                        }
                        catch (UriFormatException e)
                        {
                            continue;
                        }
                    }
                }
                _library.DBConnection.Close();
            }
            catch (SqliteException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.ResetColor();
                Console.WriteLine(e.StackTrace);
            }

            return rt;
        }

        /*
        public Dictionary<Uri, DateTime> GetTracksModifiedTime(List<Uri> targets)
        {
            var rt = new Dictionary<Uri, DateTime>();

            if (_library.DBConnection == null) throw new NullReferenceException("There's an error in database connection.");
            _library.DBConnection.Open();

            var command = _library.DBConnection.CreateCommand();
            command.CommandText = "SELECT "

            _library.DBConnection.Close();

            return rt;
        }
        */
    }
}
