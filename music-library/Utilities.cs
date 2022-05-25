using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Data.Sqlite;
using System.CommandLine.Parsing;

namespace MusicLibrary.Utilities
{
    public static class PathTools
    {
        /// <summary>
        /// Return proper path depending on operating systems.
        /// </summary>
        /// <param name="directoryPath">A targeted directory path.</param>
        /// <param name="fileName">A file name including extensions.</param>
        /// <returns>Returns Corrected file path.</returns>
        /// This method will be changed in v0.0.2
        public static string GetPath(string directoryPath, string fileName = "")
        {
            var platform = Environment.OSVersion.Platform;
            StringBuilder rt = CorrectPath(directoryPath, platform);

            if (fileName != "")
            {
                rt.Append((platform == PlatformID.Unix) ? "/" : "\\");
                rt.Append(fileName);
            }
            return rt.ToString();
        }

        /// <summary>
        /// Returns correct path suitable to operating system environment of user.
        /// </summary>
        /// <param name="target">A targeted path to be corrected.</param>
        /// <param name="platform">Running platforms.</param>
        /// <returns>Returns a StringBuilder contains corrected path</returns>
        private static StringBuilder CorrectPath(string target, PlatformID platform)
        {
            var rt = new StringBuilder(target);
            if (platform == PlatformID.Unix)
                rt.Replace('\\', '/');
            else
                rt.Replace('/', '\\');

            return rt;
        }

        /// <summary>
        /// Check whether the path is relative or not.
        /// </summary>
        /// <param name="path">A target path to be checked.</param>
        /// <returns>Returns true if the path is relative.</returns>
        public static bool IsRelativePath(string path)
        {
            return path.StartsWith("./") || path.StartsWith(".\\") || path.StartsWith("..");
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