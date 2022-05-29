using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Microsoft.Data.Sqlite;

using MusicLibrary;

namespace MusicLibrary.Commands
{
    public class ImportFile : IGetMedia
    {
        private SqliteConnection? _connection = null;
        private string _dbPath;
        private string _dbName;

        public ImportFile(string dbPath, string dbName)
        {   
            _dbPath = dbPath;
            _dbName = dbName;
        }

        public async Task<int> Import(string path)
        {
            var library = new Database.Library(_dbPath, _dbName, true);

            try
            {
                Result.ImportResult<Uri> result;
                using (_connection = library.DBConnection)
                {
                    if (_connection == null) throw new NullReferenceException("Database connection is wrong.");

                    Uri uriPath;

                    if (!Uri.IsWellFormedUriString(path, UriKind.Absolute) && Utilities.PathTools.IsRelativePath(path))
                    {
                        path = Path.GetFullPath(path);
                    }
                    uriPath = new Uri(path);

                    var targets = new List<Uri>();
                    targets.Add(uriPath);

                    var scan = new Scanner.Scanner(library);

                    result = await scan.UpdateDatabase(State.ScanType.NewEntryScan, targets);

                    result.PrintResult();
                }
            }
            catch (NullReferenceException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.ResetColor();

                Console.WriteLine(e.StackTrace);
                return 1;
            }
            catch (SqliteException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.ResetColor();

                Console.WriteLine(e.StackTrace);
                return 1;
            }
            catch (OperationCanceledException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Import operation is canceled.");
                Console.ResetColor();

                Console.WriteLine(e.StackTrace);
                return 1;
            }
            finally
            {
                library.Disconnect();
            }

            return 0;
        }
    }

    public class ImportDirectory : IGetMedia
    {
        private SqliteConnection? _connection = null;
        private string _dbPath;
        private string _dbName;

        public ImportDirectory(string dbPath, string dbName)
        {
            _dbPath = dbPath;
            _dbName = dbName;
        }

        public async Task<int> Import(string path)
        {
            var library = new Database.Library(_dbPath, _dbName, true);

            try
            {
                Result.ImportResult<Uri> result;
                using (_connection = library.DBConnection)
                {
                    if (_connection == null) throw new NullReferenceException("Database connection is wrong.");

                    Uri uriPath;

                    if (!Uri.IsWellFormedUriString(path, UriKind.Absolute) && Utilities.PathTools.IsRelativePath(path))
                    {
                        path = Path.GetFullPath(path);
                    }
                    uriPath = new Uri(path);

                    var targets = new List<Uri>();
                    targets.Add(uriPath);

                    var scan = new Scanner.Scanner(library);

                    result = await scan.UpdateDatabase(State.ScanType.NewEntryScan, targets);

                    result.PrintResult();
                }
            }
            catch (NullReferenceException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.ResetColor();

                Console.WriteLine(e.StackTrace);
                return 1;
            }
            catch (SqliteException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.ResetColor();

                Console.WriteLine(e.StackTrace);
                return 1;
            }
            catch (OperationCanceledException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Import operation is canceled.");
                Console.ResetColor();

                Console.WriteLine(e.StackTrace);
                return 1;
            }
            finally
            {
                library.Disconnect();
            }

            return 0;
        }
    }
}
