using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicLibrary.Utilities
{
    public static class PathTools
    {
        /// <summary>
        /// Return proper path depending on operating systems.
        /// </summary>
        /// <param name="directoryPath">int </param>
        /// <param name="fileName">Name of file including extensions.</param>
        /// <returns>Returns Corrected file path.</returns>
        public static string GetPath(string directoryPath, string fileName)
        {
            var rt = new StringBuilder(directoryPath);
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                rt.Replace('\\', '/');
                rt.Append('/');
            }
            else
            {
                rt.Replace('/', '\\');
                rt.Append('\\');
            }

            rt.Append(fileName);
            return rt.ToString();
        }
    }
}
