using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicLibrary.Commands
{
    interface IGetMedia
    {
        public static Task<int> Import(string path)
        {
            return Task.FromResult(0);
        }
    }
}
