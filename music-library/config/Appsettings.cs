using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicLibrary.Config
{
    namespace AppSettings
    {
        public sealed class AppSettings : ISettings
        {
            public string? MulibUserSettingsPath { get; set; }
            public string? MulibLoggingSettingsPath { get; set; }
        }
    }
}