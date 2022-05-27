using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MusicLibrary;

namespace MusicLibrary.Result
{
    public struct ImportResult<T> : IResult<T>
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
                return _total;
            }
        }

        public ImportResult()
        {
            _failedEntries = new List<T>();
        }

        public ImportResult(List<T> t)
        {
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
        public void PrintResult(bool isVerbose = false)
        {
            Console.WriteLine("Import Report:\n");
            Console.BackgroundColor = ConsoleColor.Green;
            Console.WriteLine($"{SuccessCount} files imported");
            Console.BackgroundColor = ConsoleColor.Red;
            Console.WriteLine($"{FailedCount} failed");
            Console.ResetColor();

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
                _total++;
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
                _total++;
            }
        }
    }
}
