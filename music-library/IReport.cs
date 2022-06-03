using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicLibrary.Report
{
    interface IReport<T>
    {
        public void PrintReport(bool isVerbose);
        public void AddErrorEntryList(T item);
        public void AddSuccessCount();
    }
}
