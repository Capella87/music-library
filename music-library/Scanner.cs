using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Microsoft.Data.Sqlite;

using MusicLibrary;
using MusicLibrary.Utilities;

namespace MusicLibrary.Scanner
{
    public class Scanner
    {
        private Database.Library _library;
        private Database.TrackDatabase _tracks;
        private Result.ImportResult<Uri> _result;

        public Scanner(Database.Library library)
        {
            if (library == null) throw new ArgumentNullException("Invalid library connection.");
            else _library = library;
            _tracks = new Database.TrackDatabase(_library);
        }

        public async Task<Result.ImportResult<Uri>> UpdateDatabase(State.ScanType scanType, List<Uri> targets)
        {
            if (scanType == State.ScanType.FullScan)
            {
                // Get music uris from settings
                
            }

            await Scan(scanType, targets);

            // Show result;

            return _result;
        }

        private (List<(Uri, DateTime)>, List<Uri>, List<Uri>) GetUriObjects(State.ScanType scanType, List<Uri> uris, out List<Uri> failed)
        {
            var files = new List<(Uri, DateTime)>();
            var directories = new List<Uri>();
            var streams = new List<Uri>();
            failed = new List<Uri>();

            var q = new Queue<Uri>();
            
            foreach (var uri in uris)
            {
                if (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
                    streams.Add(uri);
                else if (uri.Scheme == Uri.UriSchemeFile && (File.Exists(uri.AbsolutePath) || Directory.Exists(uri.AbsolutePath)))
                    q.Enqueue(uri);
            }

            while (q.Count > 0)
            {
                var uri = q.Dequeue();
                
                try
                {
                    string path = Utilities.PathTools.GetUnescapedAbsolutePath(uri);

                    if (Utilities.FileTools.IsDirectory(uri))
                    {
                        directories.Add(uri);
                        /*
                        var subdirectories = Directory.EnumerateDirectories(path);
                        */

                        // source: https://gist.github.com/zaus/7454021
                        var musics = FileTools.MusicExtensions.Select(x => "*" + x)
                            .SelectMany(x => Directory.EnumerateFiles(path, x));
                        foreach (var musicFile in musics)
                        {
                            var mf = new Uri(musicFile);
                            files.Add((mf, FileTools.GetModifiedTime(musicFile)));
                        }
                    }
                    else if (FileTools.IsFile(uri))
                    {
                        files.Add((uri, FileTools.GetModifiedTime(uri)));
                    }
                    else failed.Add(uri);
                }
                catch (FileNotFoundException e)
                {
                    // Add 
                }
                catch (DirectoryNotFoundException e)
                {

                }
                catch (NotSupportedException e)
                {

                }
            }

            return (files, directories, streams);
        }

        private async Task Scan(State.ScanType scanType, List<Uri> targets)
        {
            var (files, directories, streams) = GetUriObjects(State.ScanType.NewEntryScan, targets, out List<Uri> failed);

            _result = new Result.ImportResult<Uri>();

            List<Uri>? databaseEntitiesUri = null;
            if (scanType == State.ScanType.NewEntryScan)
                databaseEntitiesUri = _tracks.GetMusicUris(targets);
            else databaseEntitiesUri = _tracks.GetMusicUris();
        }
    }
}
