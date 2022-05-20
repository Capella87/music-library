using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicLibrary.State
{
    /// <summary>
    /// Define Scan type.
    /// FullScan : Full scan; Usually it's used to update database;
    /// NewEntryScan : Only adding new files or directories. "import" command uses this.
    /// </summary>
    public enum ScanType
    {
        FullScan,
        NewEntryScan,
    };

    public enum FileType
    {
        Music,
        Playlist,
        Video,
    };
}
