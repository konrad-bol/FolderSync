using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderSync.Services
{
    class FileLogger : ILogger
    {
        private string logFilePath;

        public FileLogger(string logFilePath)
        {
            this.logFilePath = logFilePath;
        }

        public void log(string message)
        {
            var fullMessage = $"[{DateTime.Now}] {message}";
            Console.WriteLine(fullMessage);
            File.AppendAllText(logFilePath, fullMessage + Environment.NewLine);
        }
    }
}
