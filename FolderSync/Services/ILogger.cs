using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderSync.Services
{
    public interface ILogger
    {
        public void log(string message);
    }
}
