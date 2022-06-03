using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MusicLibrary;

namespace MusicLibrary.Report
{
    public struct ImportReport<T> : IReport<T>
    {
        private List<T> _failedEntries;
        private readonly static object _lock = new();

        private int _successCount = 0;
        private int _failedCount = 0;
        private int _total = 0;

        public int SuccessCount
        {
            get
            {
                lock (_lock)
                {
                    return _successCount;
                }
            }
        }

        public int FailedCount
        {
            get
            {
                lock (_lock)
                {
                    return _failedCount;
                }
            }
        }

        public int Total
        {
            get
            {
                lock (_lock)
                {
                    return _total;
                }
            }
        }

        public ImportReport(int total)
        {
            _total = total;
            _failedEntries = new List<T>();
        }

        public ImportReport(int total, List<T> t)
        {
            _total = total;
            _failedEntries = t;
            lock (_lock)
            {
                _failedCount = t.Count;
            }
        }

        /// <summary>
        /// Show results.
        /// </summary>
        /// <param name="isVerbose">An option to show failed entries. Default is false.</param>
        public void PrintReport(bool isVerbose = false)
        {
            Console.WriteLine("Import Report");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"{SuccessCount} files imported");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($", {FailedCount} failed.");
            Console.ResetColor();
            Console.WriteLine($"{Total - SuccessCount - FailedCount} files were ignored.");

            Console.WriteLine($"Total: {Total}");
        }

        /// <summary>
        /// Add a failed entry into a list and increment _failedCount.
        /// </summary>
        /// <param name="item">A target to be inserted.</param>
        public void AddErrorEntryList(T item)
        {
            lock(_lock)
            {
                _failedEntries.Add(item);
                _failedCount++;
            }
        }

        /// <summary>
        /// Increment _successCount.
        /// </summary>
        public void AddSuccessCount()
        {
            lock(_lock)
            {
                _successCount++;
            }
        }
    }
}
