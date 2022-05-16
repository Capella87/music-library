using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicLibrary.State
{
    /// <summary>
    /// Define Scan type.
    /// </summary>
    enum ScanType
    {
        FullScan,
        NewFileOnly,
    };

    enum FileType
    {
        Music,
        Playlist,
        Video,
    };
}
