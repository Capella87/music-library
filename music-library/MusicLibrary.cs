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
            catch (SqliteException)
            {
                Console.WriteLine("Cannot Connected to the database.");
                return;
            }
            catch (NullReferenceException)
            {
                return;
            }
        }

        private void Connect(bool isNewDB)
        {
            if (_options == null)
                throw new NullReferenceException();

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
            
        }
    }
}
