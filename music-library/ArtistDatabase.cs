using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Data.Sqlite;

namespace MusicLibrary.Database
{
    public class ArtistDatabase
    {
        private Library _library;

        public ArtistDatabase(Library library)
        {
            if (library == null || library.DBConnection == null)
                throw new ArgumentNullException("There's an error in database connection.");
            _library = library;
        }

        public long? GetArtistId(string? artist)
        {
            if (artist == null || artist == "") return -1;

            _library.DBConnection.Open();

            var command = new SqliteCommand();
            command.CommandText = "SELECT id FROM artists WHERE artist = @keyword";
            command.Parameters.Add(new SqliteParameter("@keyword", artist));

            var result = command.ExecuteReader();
            _library.DBConnection.Close();

            return result?.GetInt64(0);
        }

        public long AddArtist(string artist)
        {
            _library.DBConnection.Open();

            var command = new SqliteCommand();
            command.CommandText = "INSERT INTO artists (artist) " +
                "VALUES (@art);";
            command.Parameters.Add(new SqliteParameter("@art", artist));

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
