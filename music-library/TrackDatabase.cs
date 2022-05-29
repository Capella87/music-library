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

        public Dictionary<Uri, DateTime> GetTracksModifiedTime()
        {
            var rt = new Dictionary<Uri, DateTime>();

            if (_library.DBConnection == null) throw new NullReferenceException("There's an error in database connection.");
            _library.DBConnection.Open();

            var command = _library.DBConnection.CreateCommand();
            command.CommandText = "SELECT DISTINCT uri, modified_time from tracks";

            var result = command.ExecuteReader();
            while (result.Read())
            {
                try
                {
                    rt.Add(new Uri(result.GetString(0)), result.GetDateTime(1));
                }
                catch (ArgumentNullException e)
                {

                }
                catch (UriFormatException e)
                {

                }
            }

            _library.DBConnection.Close();

            return rt;
        }

        public long? GetTrackId(Uri uri)
        {
            _library.DBConnection.Open();

            var command = new SqliteCommand();
            command.CommandText = "SELECT id FROM tracks WHERE uri = @keyword";
            command.Parameters.Add(new SqliteParameter("@keyword", uri.AbsoluteUri));

            var result = command.ExecuteReader();
            _library.DBConnection.Close();

            return result?.GetInt64(0);
        }

        public long? GetTrackId(string? title)
        {
            _library.DBConnection.Open();

            var command = new SqliteCommand();
            command.CommandText = "SELECT id FROM tracks WHERE title = @keyword";
            command.Parameters.Add(new SqliteParameter("@keyword", title));

            var result = command.ExecuteReader();
            _library.DBConnection.Close();

            return result?.GetInt64(0);
        }

        public long? GetGenreId(string? genre)
        {
            if (genre == null || genre == "") return null;

            _library.DBConnection.Open();

            var command = new SqliteCommand();
            command.CommandText = "SELECT id FROM genres WHERE genre = @keyword";
            command.Parameters.Add(new SqliteParameter("@keyword", genre));

            var result = command.ExecuteReader();
            _library.DBConnection.Close();

            return result?.GetInt64(0);
        }

        public long AddGenre(string genre)
        {
            _library.DBConnection.Open();

            var command = new SqliteCommand();
            command.CommandText = "INSERT INTO genres (genre) " +
                "VALUES (@gen);";
            command.Parameters.Add(new SqliteParameter("@gen", genre));

            var result = command.ExecuteNonQuery();

            long rt = -1;
            if (result == 1)
            {
                command.CommandText = "SELECT last_insert_rowid();";
                rt = (long)command.ExecuteScalar();
            }

            _library.DBConnection.Close();
            return rt;
        }

        public long AddTrack(Database.Tag tag, long? albumId, long? artistId, long? genreId)
        {
            _library.DBConnection.Open();

            var command = new SqliteCommand();
            command.CommandText = "INSERT INTO tracks " +
                "(title, year, uri, imported_time, modified_time" +
                "duration, absolute_path, disc_no, track_no, bitrates, audio_samplerates, " +
                "audio_channels, lyrics, album_id, artist_id, genre_id) " +
                "VALUES (@tit, @ye, @imptTime, @modiTime, @dur, @absoPath, @disc, @trackNo, @bitra, @audioSampleRate, " +
                "@audioChannel, @lyri, @albId, @artistId, @genreId)";
            
            command.Parameters.Add(new SqliteParameter("@tit", tag.Title));
            command.Parameters.Add(new SqliteParameter("@ye", tag.Year));
            command.Parameters.Add(new SqliteParameter("@imptTime", tag.ImportedTime));
            command.Parameters.Add(new SqliteParameter("@modiTime", tag.ModifiedTime));
            command.Parameters.Add(new SqliteParameter("@dur", tag.Duration));
            command.Parameters.Add(new SqliteParameter("@absoPath", tag.AbsolutePath));
            command.Parameters.Add(new SqliteParameter("@disc", tag.DiscNumber));
            command.Parameters.Add(new SqliteParameter("@trackNo", tag.TrackNumber));
            command.Parameters.Add(new SqliteParameter("@bitra", tag.Bitrates));
            command.Parameters.Add(new SqliteParameter("@audioSampleRate", tag.AudioSampleRates));
            command.Parameters.Add(new SqliteParameter("@audioChannel", tag.AudioChannels));
            command.Parameters.Add(new SqliteParameter("@lyri", tag.Lyrics));

            command.Parameters.Add(new SqliteParameter("@albId", albumId));
            command.Parameters.Add(new SqliteParameter("@artistId", artistId));
            command.Parameters.Add(new SqliteParameter("@genreId", genreId));

            var result = command.ExecuteNonQuery();

            long rt = -1;
            if (result == 1)
            {
                command.CommandText = "SELECT last_insert_rowid();";
                rt = (long)command.ExecuteScalar();
            }

            _library.DBConnection.Close();
            return rt;
        }
    }
}
