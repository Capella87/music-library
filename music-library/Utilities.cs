using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Data.Sqlite;

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

    public static class FileTools
    {
        /// <summary>
        /// Get last modified date of targeted files.
        /// </summary>
        /// <param name="rt">Dictionary which contains file and its modified time. out parameter.</param>
        /// <param name="files">File list to get modified time.</param>
        /// <returns>A list containing failed Uris to get modified time.</returns>
        public static List<Uri> GetModifiedTime(out Dictionary<Uri, DateTime> rt, List<Uri> files)
        {
            rt = new Dictionary<Uri, DateTime>();
            List<Uri> failed = new();


            foreach (var e in files)
            {
                try
                {
                    var modifiedTime = File.GetLastAccessTimeUtc(e.LocalPath);
                    rt.Add(e, modifiedTime);
                }
                catch (UnauthorizedAccessException err)
                {
                    // Logging or print error information in the console.

                    // Add to failed;
                    failed.Add(e);
                }
                catch (NotSupportedException err)
                {
                    // Logging or print error information in the console.

                    // Add to failed
                    failed.Add(e);
                }
            }

            return failed;
        }

    }
}
