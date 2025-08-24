using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderSync.Services
{
    public interface ILogger
    {
        public void start_log();
        public void end_log();
        public void log(string message);
        public void log_change(List<string> messages);
    }
}
