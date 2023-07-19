using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicLibrary.Database
{
    public record class Tag
    {
        public string?          Title { get; set; }
        public Uri              URI { get; set; }
        public string?          Artist { get; set; }
        public string?          Album { get; set; }
        public string?          AlbumArtist { get; set; }
        public string?          Genre { get; set; }
        public uint?            Year { get; set; }
        public DateOnly?        ReleasedDate { get; set; }
        public DateTime         ImportedTime { get; set; }
        public DateTime         ModifiedTime { get; set; }
        public TimeSpan         Duration { get; set; }
        public int              Bitrates { get; set; }
        public int              AudioSampleRates { get; set; }
        public int              AudioChannels { get; set; }
        public string           AbsolutePath { get; set; }
        public uint?            DiscNumber { get; set; }
        public uint?            DiscCount { get; set; }
        public uint?            TrackNumber { get; set; }
        public uint?            TrackCount { get; set; }
        public string?          UnsyncedLyrics { get; set; }
        public string?          SyncedLyrics { get; set; }
        public bool?            IsCompilation { get; set; }
        public uint?            BeatsPerMinute { get; set; }
        public string?          Composers { get; set; }

        public ExtendedTag?     ExtendedTags { get; set; } = null;
    }

    public record class ExtendedTag
    {
        // MusicBrainz

        // iTunes

        // ReplayGain
    }

}
