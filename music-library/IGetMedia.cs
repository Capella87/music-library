using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicLibrary.Commands
{
    interface IGetMedia
    {
        public async Task<int> Import(string path)
        {
            return 1;
        }
    }
}
