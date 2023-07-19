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
        public static int Run()
        {
            return 0;
        }

        public static Task ResetDatabase(string _dbPath, string _dbName)
        {
            var library = new Database.Library(_dbPath, _dbName, true);
            char answer;

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("WARNING: This command will OBLITERATE ALL your data in the database.");
            Console.ResetColor();
            Console.Write("Are you sure to RESET your database [Y/N] : ");

            do
            {
                try
                {
                    answer = Char.Parse(Console.ReadLine());
                    if (answer == 'Y' || answer == 'y')
                    {
                        Console.WriteLine("Reset all of your data in database. All files will NOT be affected.");
                        try
                        {
                            using (var connection = library.DBConnection)
                            {
                                if (connection == null) throw new NullReferenceException("Database connection is wrong.");
                                library.Reset();
                            }
                            Console.WriteLine("Done.");
                        }
                        catch (NullReferenceException e)
                        {
                            Console.BackgroundColor = ConsoleColor.Red;
                            Console.WriteLine(e.Message);
                            Console.ResetColor();

                            Console.WriteLine(e.StackTrace);
                            return Task.FromResult(1);
                        }
                        break;
                    }
                    else if (answer == 'n' || answer == 'n')
                    {
                        Console.WriteLine("Abort.");
                        break;
                    }
                }
                catch (FormatException e)
                {
                    Console.WriteLine("Wrong input.");
                }
            } while (true);

            return Task.FromResult(0);
        }
    }
}
