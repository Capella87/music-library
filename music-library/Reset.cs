using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MusicLibrary;

using Microsoft.Data.Sqlite;

namespace MusicLibrary.Commands
{
    public static class Reset
    {
        public static Task ResetDatabase(string _dbPath, string _dbName)
        {
            var library = new Database.Library(_dbPath, _dbName, true);

            try
            {
                using (var connection = library.DBConnection)
                {
                    if (connection == null) throw new NullReferenceException("Database connection is wrong.");
                    library.Reset();
                }
            }
            catch (NullReferenceException e)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.ResetColor();

                Console.WriteLine(e.StackTrace);
                return Task.FromResult(1);
            }

            return Task.FromResult(0);
        }
    }
}
