using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace MusicLibrary
{
    public partial class Library
    {
        private SqliteConnection? _connection = null;
        private string? _options = null;

        public String FileName { get; set; }
        public String LibraryName { get; set; }
        public String Path { get; set; }

        public Library(string path, string fileName, string name = "main")
        {
            FileName = fileName;
            LibraryName = name;
            if (Environment.OSVersion.Platform == PlatformID.Unix)
                Path = path + "/" + fileName;
            else Path = path + "\\" + fileName;

            var scsb = new SqliteConnectionStringBuilder()
            {
                DataSource = Path
            };

            bool isNewDB = false;
            if (!File.Exists(Path))
            {
                isNewDB = true;
                scsb.Mode = SqliteOpenMode.ReadWriteCreate;
            }
            else scsb.Mode = SqliteOpenMode.ReadWrite;

            _options = scsb.ToString();

            try
            {
                this.Connect(isNewDB);
            }
            catch (SqliteException e)
            {
                if (e.Message == "")
                    Console.WriteLine("Cannot Connected to the database.");
                else Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return;
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return;
            }
        }

        /// <summary>
        /// Connect to the database. if isNewDB is true, database file will be created.
        /// </summary>
        /// <param name="isNewDB"></param>
        /// <exception cref="NullReferenceException"></exception>
        private void Connect(bool isNewDB)
        {
            if (_options == null)
                throw new NullReferenceException("NULL options is not accepted.");

            using (_connection = new SqliteConnection(_options))
            {
                _connection.Open();
                if (isNewDB)
                {
                    Reset();
                }
            }
        }

        /// <summary>
        /// Reset all tables and create tables.
        /// </summary>
        private void Reset()
        {
            if (_connection == null) throw new NullReferenceException("There's an error in connection.");

            var dropTables = _connection.CreateCommand();
            dropTables.CommandText =
                @"
                 DROP TABLE IF EXISTS artists;
                 DROP TABLE IF EXISTS album_artists
                 ";

            dropTables.ExecuteNonQuery();

            var addTables = _connection.CreateCommand();
            addTables.CommandText =
                @"
                CREATE TABLE artists
                (
                    id         INTEGER PRIMARY KEY NOT NULL,
                    artist     TEXT
                );
                CREATE TABLE album_artists
                (
                    id              INTEGER PRIMARY KEY NOT NULL,
                    album_artist    TEXT
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
    }
}
