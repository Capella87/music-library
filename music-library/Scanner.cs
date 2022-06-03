using Microsoft.Data.Sqlite;
using MusicLibrary.Utilities;

namespace MusicLibrary.Scanner
{
    public class Scanner
    {
        private Database.Library _library;
        private Database.TrackDatabase _tracks;
        private Database.AlbumDatabase _albums;
        private Database.ArtistDatabase _artists;
        private Report.ImportReport<Uri> _result;
        private Dictionary<Uri, Database.Tag> _retrievedTags;

        private readonly object _scanLock = new Object();

        internal class ScanInfo
        {
            private (Uri, DateTime)[] _target;
            private readonly State.ScanType _scanType;
            private readonly Dictionary<Uri, DateTime> _databaseModifiedTimes;
            private CancellationToken _cts;

            public (Uri, DateTime)[] ScanTarget
            { get { return _target; } }
            public State.ScanType ScanTypeInfo
            { get { return _scanType; } }
            public Dictionary<Uri, DateTime> DatabaseModifiedTimes
            { get { return _databaseModifiedTimes; } }
            public CancellationToken CancellationToken
            { get { return _cts; } }

            public ScanInfo((Uri, DateTime)[] target, State.ScanType scanType, Dictionary<Uri, DateTime> dbTimes, CancellationToken token)
            {
                _target = target;
                _scanType = scanType;
                _databaseModifiedTimes = dbTimes;
                _cts = token;
            }
        }

        public Scanner(Database.Library library)
        {
            if (library == null) throw new ArgumentNullException("Invalid library connection.");
            else _library = library;
            _tracks = new Database.TrackDatabase(_library);
            _albums = new Database.AlbumDatabase(_library);
            _artists = new Database.ArtistDatabase(_library);
        }

        public async Task<Report.ImportReport<Uri>> UpdateDatabase(State.ScanType scanType, List<Uri> targets)
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

                        var subdirectories = Directory.EnumerateDirectories(path, "*",
                            new EnumerationOptions() { IgnoreInaccessible = true });
                        foreach (var sub in subdirectories)
                            q.Enqueue(new Uri(sub));

                        // source: https://gist.github.com/zaus/7454021
                        var musics = FileTools.MusicExtensions.Select(x => "*" + x)
                            .SelectMany(x => Directory.EnumerateFiles
                            (path, x, new EnumerationOptions() { IgnoreInaccessible = true }));
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

            _result = new Report.ImportReport<Uri>(files.Count);

            List<Uri>? databaseEntitiesUri = null;
            if (scanType == State.ScanType.NewEntryScan)
                databaseEntitiesUri = _tracks.GetMusicUris(targets);
            else databaseEntitiesUri = _tracks.GetMusicUris();

            var databaseTracks = _tracks.GetTracksModifiedTime();

            // Use two thread; it can be changed to use more threads depending on systems...
            _retrievedTags = new Dictionary<Uri, Database.Tag>();

            int fileCount = files.Count;
            int splitCount = fileCount / 2;
            var chunks = files.Chunk(splitCount + 1);
            var first = chunks.First();
            var second = chunks.Last();

            // It would be replaced to TaskFactory
            Thread[] scanThreads = new Thread[2];
            var tokenSource = new CancellationTokenSource();

            try
            {
                if (fileCount > 1)
                {
                    scanThreads[0] = new Thread(ScanFile);
                    scanThreads[1] = new Thread(ScanFile);

                    scanThreads[0].Start((Object?)new ScanInfo(first, scanType, databaseTracks, tokenSource.Token));
                    scanThreads[1].Start((Object?)new ScanInfo(second, scanType, databaseTracks, tokenSource.Token));

                    scanThreads[0].Join();
                    scanThreads[1].Join();
                }
                else
                {
                    scanThreads[0] = new Thread(ScanFile);
                    scanThreads[0].Start((Object?)new ScanInfo(first, scanType, databaseTracks, tokenSource.Token));
                    scanThreads[0].Join();
                }
            }
            catch (ArgumentNullException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.ResetColor();

                Console.WriteLine(e.StackTrace);
                return;
            }
            catch (InvalidOperationException e)
            {
                tokenSource.Cancel();
                tokenSource.Dispose();
                return;
            }

            SaveToDatabase();
        }

        private void ScanFile(object? scanInfo)
        {
            try
            {
                ScanInfo? info = (ScanInfo?)scanInfo;
                if (info == null) throw new ArgumentNullException("Invalid Scan informations.");

                foreach (var target in info.ScanTarget)
                {
                    info.DatabaseModifiedTimes.TryGetValue(target.Item1, out DateTime dbTime);

                    if (target.Item2 > dbTime)
                    {
                        var tag = GetTagFromFile(target.Item1);
                        if (tag != null)
                        {
                            lock (_scanLock)
                            {
                                _retrievedTags[target.Item1] = tag;
                            }
                        }
                    }
                    else continue;
                }
            }
            catch (OperationCanceledException e)
            {
                Console.WriteLine("Operation cancellation is requested. Halt this task.");
                throw;
                return;
            }
        }

        private Database.Tag? GetTagFromFile(Uri uri)
        {
            Database.Tag? rt = null;
            string absolutePath = PathTools.GetUnescapedAbsolutePath(uri);
            try
            {
                var target = TagLib.File.Create(absolutePath);

                rt = new Database.Tag
                {
                    Title = target.Tag.Title,
                    URI = uri,
                    Artist = target.Tag.Performers.Length > 0 ? target.Tag.Performers[0] : null,
                    Album = target.Tag.Album,
                    AlbumArtist = target.Tag.AlbumArtists.Length > 0 ? target.Tag.AlbumArtists[0] : null,
                    Genre = target.Tag.Genres.Length > 0 ? target.Tag.Genres[0] : null,
                    Year = target.Tag.Year,
                    ImportedTime = DateTime.UtcNow,
                    ModifiedTime = File.GetLastWriteTimeUtc(absolutePath),
                    Duration = target.Properties.Duration,
                    Bitrates = target.Properties.AudioBitrate,
                    AudioSampleRates = target.Properties.AudioSampleRate,
                    AudioChannels = target.Properties.AudioChannels,
                    AbsolutePath = absolutePath,
                    DiscNumber = target.Tag.Disc,
                    TrackNumber = target.Tag.Track,
                    Lyrics = target.Tag.Lyrics
                };

                target.Dispose();
            }
            catch (TagLib.CorruptFileException e)
            {
                Console.WriteLine($"Failed to load {PathTools.GetUnescapedAbsolutePath(uri)}. This file is corrupted.");
                _result.AddErrorEntryList(uri);
            }
            catch (TagLib.UnsupportedFormatException e)
            {
                Console.WriteLine($"Failed to load {PathTools.GetUnescapedAbsolutePath(uri)}. This file is not supported format.");
                _result.AddErrorEntryList(uri);
            }

            return rt;
        }

        private int SaveToDatabase()
        {
            foreach (var file in _retrievedTags)
            {
                try
                {
                    InsertTuple(file.Value);
                    Console.WriteLine($"Adding {file.Value.AbsolutePath} file to library.");
                    _retrievedTags.Remove(file.Key);
                    _result.AddSuccessCount();
                }
                catch (SqliteException e)
                {
                    /*
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                    */
                    Console.WriteLine($"Failed to add {file.Value.AbsolutePath} file to library.");

                    // Add debug info in logger.

                    _result.AddErrorEntryList(file.Value.URI);
                }
                catch (OperationCanceledException e)
                {
                    _retrievedTags.Clear();
                    return -1;
                }
            }
            _retrievedTags.Clear();
            return 0;
        }

        private void InsertTuple(Database.Tag tag)
        {
            var (albumArtistId, albumId) = InsertAlbum(tag);
            var artistId = InsertArtist(tag);
            var (trackId, genreId) = InsertTrack(tag, albumId, artistId);
        }

        private (long? albumArtistId, long? albumId) InsertAlbum(Database.Tag tag)
        {
            try
            {
                long? albumArtistId = _albums.GetAlbumArtistId(tag.AlbumArtist);
                if (albumArtistId.HasValue && albumArtistId == -1) // -1 -> tag is null.
                    albumArtistId = null;
                else if (albumArtistId == null) // not exist in the database.
                    albumArtistId = _albums.AddAlbumArtist(tag.AlbumArtist);

                long? albumId = _albums.GetAlbumId(tag.Album);
                if (!albumArtistId.HasValue || albumArtistId == -1)
                    albumId = null;
                else if (albumId == null)
                    albumId = _albums.AddAlbum(tag.Album, albumArtistId);

                return (albumArtistId, albumId);
            }
            catch (SqliteException e)
            {
                throw;
            }
            finally
            {
                _library.DBConnection.Close();
            }
        }

        private long? InsertArtist(Database.Tag tag)
        {
            try
            {
                long? artistId = _artists.GetArtistId(tag.Artist);
                if (artistId.HasValue && artistId == -1) // -1 -> tag is null.
                    artistId = null;
                else if (artistId == null) // not exist in the database.
                    artistId = _artists.AddArtist(tag.Artist);

                return artistId;
            }
            catch (SqliteException e)
            {
                throw;
            }
            finally
            {
                _library.DBConnection.Close();
            }
        }

        private (long? trackId, long? genreId) InsertTrack(Database.Tag tag, long? albumId, long? artistId)
        {
            try
            {
                long? genreId = _tracks.GetGenreId(tag.Genre);
                if (genreId.HasValue && genreId == -1) // -1 -> tag is null.
                    genreId = null;
                else if (genreId == null) // not exist in the database.
                    genreId = _tracks.AddGenre(tag.Genre);

                long? trackId = _tracks.GetTrackId(tag.URI);
                if (!trackId.HasValue && trackId == -1)
                    trackId = null;
                else if (trackId == null)
                    trackId = _tracks.AddTrack(tag, albumId, artistId, genreId);

                return (trackId, albumId);
            }
            finally
            {
                _library.DBConnection.Close();
            }
        }
    }
}