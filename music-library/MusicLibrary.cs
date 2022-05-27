using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

using MusicLibrary.Utilities;

namespace MusicLibrary.Database
{
    public class Library
    {
        private SqliteConnection? _connection = null;
        private string? _options = null;

        public SqliteConnection? DBConnection { get { return _connection; } }
        public String FileName { get; set; }
        public String LibraryName { get; set; }
        public String Path { get; set; }

        public Library(string path, string fileName, bool hasDatabaseFile, string name = "main")
        {
            FileName = fileName;
            LibraryName = name;
            Path = Utilities.PathTools.GetPath(path, fileName);

            try
            {
                var opts = SetConnectionOptions(hasDatabaseFile);
                this.ConnectDatabase(opts);
            }
            catch (SqliteException e)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                if (e.Message == "")
                    Console.WriteLine("Cannot Connected to the database.");
                else Console.WriteLine(e.Message);
                Console.ResetColor();
                Console.WriteLine(e.StackTrace);
                return;
            }
            catch (NullReferenceException e)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.ResetColor();

                Console.WriteLine(e.StackTrace);
                return;
            }
        }

        /// <summary>
        /// Set SQLite connection options.
        /// </summary>
        /// <param name="hasDatabaseFile">Database file existence.</param>
        /// <returns>Return SqliteConnectionStringBuilder if setting was successful.</returns>
        private SqliteConnectionStringBuilder? SetConnectionOptions(bool hasDatabaseFile)
        {
            var scsb = new SqliteConnectionStringBuilder()
            {
                DataSource = this.Path
            };
            scsb.Mode = !hasDatabaseFile ? SqliteOpenMode.ReadWriteCreate : SqliteOpenMode.ReadWrite;
            _options = scsb.ToString();

            return scsb;
        }

        /// <summary>
        /// Connect to database with options.
        /// </summary>
        /// <param name="connectionOptionStringBuilder">A StringBuilder which contains Connection options</param>
        /// <exception cref="NullReferenceException"></exception>
        private void ConnectDatabase(SqliteConnectionStringBuilder connectionOptionStringBuilder)
        {
            if (connectionOptionStringBuilder == null)
                throw new NullReferenceException("NULL options is not accepted.");

            _connection = new SqliteConnection(connectionOptionStringBuilder.ToString());
            _connection.Open();
            if (connectionOptionStringBuilder.Mode == SqliteOpenMode.ReadWriteCreate)
            {
                ResetDatabase();
                connectionOptionStringBuilder.Mode = SqliteOpenMode.ReadWrite;
                _options = connectionOptionStringBuilder.ToString();
                _connection.Close();
                _connection.ConnectionString = _options;
                _connection.Open();
            }
        }

        /// <summary>
        /// Reset all tables and create tables.
        /// </summary>
        /// <exception cref="NullReferenceException"></exception>
        private void ResetDatabase()
        {
            if (_connection == null) throw new NullReferenceException("There's an error in connection.");

            var dropTables = _connection.CreateCommand();
            dropTables.CommandText =
                @"
                 DROP TABLE IF EXISTS artists;
                 DROP TABLE IF EXISTS album_artists;
                 DROP TABLE IF EXISTS albums;
                 DROP TABLE IF EXISTS genres;
                 DROP TABLE IF EXISTS tracks
                 ";

            dropTables.ExecuteNonQuery();

            var addTables = _connection.CreateCommand();
            addTables.CommandText =
                @"
                CREATE TABLE artists
                (
                    id                  INTEGER PRIMARY KEY NOT NULL,
                    artist              TEXT
                );
                CREATE TABLE album_artists
                (
                    id                  INTEGER PRIMARY KEY NOT NULL,
                    album_artist        TEXT
                );
                CREATE TABLE albums
                (
                    id                  INTEGER PRIMARY KEY NOT NULL,
                    album               TEXT,
                    album_artist_id     INTEGER,
                    FOREIGN KEY (album_artist_id) REFERENCES album_artists(id)
                );
                CREATE TABLE genres
                (
                    id                  INTEGER PRIMARY KEY NOT NULL,
                    genre               TEXT
                );
                CREATE TABLE tracks
                (
                    id                  INTEGER PRIMARY KEY NOT NULL,
                    title               TEXT,
                    year                INTEGER,
                    uri                 TEXT NOT NULL,
                    modified_time       INTEGER NOT NULL,
                    directory           TEXT,
                    disc_no             INTEGER,
                    track_no            INTEGER,
                    lyrics              TEXT,

                    album_id            INTEGER,
                    artist_id           INTEGER,
                    genre_id            INTEGER,
                    FOREIGN KEY (album_id) REFERENCES albums(id),
                    FOREIGN KEY (artist_id) REFERENCES artists(id),
                    FOREIGN KEY (genre_id)  REFERENCES genres(id)
                )
                 ";
            addTables.ExecuteNonQuery();
        }

        /// <summary>
        /// Scan files in directory or individual file.
        /// </summary>
        private void Scan()
        {

        }

        /// <summary>
        /// Update database through scanning files.
        /// </summary>
        private void Update()
        {

        }

        /// <summary>
        /// Disconnect to database.
        /// </summary>
        public void Disconnect()
        {
            if (_connection != null)
                _connection.Dispose();
        }

        /// <summary>
        /// Connect to database.
        /// </summary>
        /// <returns></returns>
        public bool Connect()
        {
            try
            {
                bool isFileExist = File.Exists(Path) ? true : false;
                ConnectDatabase(SetConnectionOptions(isFileExist));
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return false;
            }

            return true;
        }
    }
}
