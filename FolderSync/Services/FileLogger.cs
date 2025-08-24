using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderSync.Services
{
    public class FileLogger : ILogger
    {
        private string logFilePath;

        public FileLogger(string logFilePath)
        {
            this.logFilePath = logFilePath;
        }

        public void log(string message)
        {
            Console.WriteLine(message);
            File.AppendAllText(logFilePath, message + Environment.NewLine);
        }
        public void log_change(List<string> messages)
        {
            foreach (var message in messages)
            {
                Console.WriteLine(message);
                File.AppendAllText(logFilePath, message + Environment.NewLine);
            }
        }
        public void start_log()
        {
            Console.WriteLine($"[{DateTime.Now}] Log started.");
            File.AppendAllText(logFilePath, $"[{DateTime.Now}] Log started." + Environment.NewLine);
        }
        public void end_log()
        {
            Console.WriteLine($"[{DateTime.Now}] Log ended.");
            File.AppendAllText(logFilePath, $"[{DateTime.Now}] Log ended." + Environment.NewLine);
        }
    }
}
