using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MusicLibrary.Commands
{
    public class ImportFile : IGetMedia
    {
        public static async Task<int> Import(string path)
        {
            // Check file path is URI and its validity;
            if (Uri.IsWellFormedUriString(path, UriKind.Absolute))
            {
                var target = new Uri(path);

                // Scanner
                return await Task.FromResult(0);
            }

            // If the path is not a URI, Check whether the path is relative.
            if (Utilities.PathTools.IsRelativePath(path))
            {

            }
            else
            {
                
            }

            return await Task.FromResult(0);
        }
    }
}
