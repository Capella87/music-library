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
            if (path == null) throw new ArgumentNullException();
            return path.StartsWith("./") || path.StartsWith(".\\") || path.StartsWith("..");
        }

        public static string GetUnescapedAbsolutePath(Uri uri)
        {
            return Uri.UnescapeDataString(uri.AbsolutePath);
        }
    }

    public static class FileTools
    {
        // This can be replaced to Hash?
        public static readonly string[] MusicExtensions =
            { ".mp3", ".m4a", ".wma", ".ogg", ".flac", ".wav", ".ape", ".aac" };

        public static DateTime GetModifiedTime(Uri uri)
        {
            string absolutePath = PathTools.GetUnescapedAbsolutePath(uri);
            if (!File.Exists(absolutePath))
                throw new FileNotFoundException("The file is not found or not permitted to access.");
            try
            {
                return File.GetLastWriteTimeUtc(absolutePath);
            }
            catch (Exception e)
            {
                return new DateTime(0);
            }
        }

        public static DateTime GetModifiedTime(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("The file is not found or not permitted to access.");
            try
            {
                return File.GetLastWriteTimeUtc(path);
            }
            catch (Exception e)
            {
                return new DateTime(0);
            }
        }

        public static bool IsDirectory(Uri uri)
        {
            return Directory.Exists(PathTools.GetUnescapedAbsolutePath(uri)) ? true : false;
        }

        public static bool IsFile(Uri uri)
        {
            return File.Exists(PathTools.GetUnescapedAbsolutePath(uri)) ? true : false;
        }
    }
}