using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicLibrary.Result
{
    interface IResult<T>
    {
        public void PrintResult(bool isVerbose);
        public void AddErrorEntryList(T item);
        public void AddSuccessCount();
    }
}
