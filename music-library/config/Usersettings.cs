using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration.Json;

namespace MusicLibrary.Config
{
    namespace UserSettings
    {
        public class Library
        {
            public List<string>? Directories { get; set; }
            public List<string>? Files { get; set; }

            public string? DatabasePath { get; set; }

            public bool EnableScanSubdirectories { get; set; }
            public bool EnableAutoUpdateLibrary { get; set; }
        }

        public class Player
        {
            public string? DefaultPlayer { get; set; }
            public PlayerEntries? Players { get; set; }
        }

        public class PlayerEntries
        {
            public string? Name { get; set; }
            public string? Description { get; set; }
            public string? PlayerPath { get; set; }
            public Arguments? Args { get; set; }
        }

        public class Arguments
        {
            public string? Option { get; set; }
            public string? ValueType { get; set; }
            public string? Value { get; set; }
        }
    }
}