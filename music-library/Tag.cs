using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicLibrary.Database
{
    public record class Tag
    {
        public string?      Title { get; init; }
        public Uri          URI { get; init; }
        public string?      Artist { get; init; }
        public string?      AlbumArtist { get; init; }
        public string?      Genre { get; init; }
        public uint         Year { get; init; }
        public DateTime     ImportedTime { get; set; }
        public DateTime     ModifiedTime { get; set; }
        public TimeSpan     Duration { get; init; }
        public int          Bitrates { get; init; }
        public int          AudioSampleRates { get; init; }
        public int          AudioChannels { get; init; }
        public string       AbsolutePath { get; set; }
        public uint         DiskNumber { get; init; }
        public uint         TrackNumber { get; init; }
        public string?      Lyrics { get; init; }
    }
}
