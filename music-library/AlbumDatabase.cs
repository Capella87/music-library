using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Data.Sqlite;

namespace MusicLibrary.Database
{
    public class AlbumDatabase
    {
        private Library _library;

        public AlbumDatabase(Library library)
        {
            if (library == null || library.DBConnection == null)
                throw new ArgumentNullException("There's an error in database connection.");
            _library = library;
        }

        public long? GetAlbumId(string? album)
        {
            if (album == null || album == "") return null;

            _library.DBConnection.Open();

            var command = new SqliteCommand();
            command.CommandText = "SELECT id FROM albums WHERE album = @keyword";
            command.Parameters.Add(new SqliteParameter("@keyword", album));

            var result = command.ExecuteReader();
            _library.DBConnection.Close();

            return result?.GetInt64(0);
        }

        public long? GetAlbumArtistId(string? albumArtist)
        {
            if (albumArtist == null || albumArtist == "") return null;

            _library.DBConnection.Open();

            var command = new SqliteCommand();
            command.CommandText = "SELECT id FROM album_artists WHERE album_artist = @keyword";
            command.Parameters.Add(new SqliteParameter("@keyword", albumArtist));

            var result = command.ExecuteReader();
            _library.DBConnection.Close();

            return result?.GetInt64(0);
        }

        public long AddAlbumArtist(string albumArtist)
        {
            _library.DBConnection.Open();

            var command = new SqliteCommand();
            command.CommandText = "INSERT INTO album_artists (album_artist) " +
                "VALUES (@alb);";
            command.Parameters.Add(new SqliteParameter("@alb", albumArtist));

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

        public long AddAlbum(string album, long? albumArtistId)
        {
            _library.DBConnection.Open();

            var command = new SqliteCommand();
            command.CommandText = "INSERT INTO albums (album, album_artist_id) " +
                "VALUES (@alb, @alb_id);";
            command.Parameters.Add(new SqliteParameter("@alb", album));
            command.Parameters.Add(new SqliteParameter("@alb_id", albumArtistId));

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